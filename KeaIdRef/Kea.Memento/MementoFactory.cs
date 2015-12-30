using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Kea
{
    /// <summary>
    /// Provides a method for creating dynamic mementos
    /// </summary>
    public static class MementoFactory
    {
        [ThreadStatic]
        private static ProxyGenerator generator = new ProxyGenerator();

        class ProxyInterceptor : IInterceptor
        {
            /// <summary>
            /// Contains all properties that should be copied back to the original model
            /// </summary>
            public readonly HashSet<string> copyProperties;


            public readonly object Source;
            public ProxyInterceptor(object Source, HashSet<string> copyProperties, Func<PropertyInfo, bool> IncludeProperty)
            {
                this.copyProperties = copyProperties;
                this.Source = Source;

                //Add all non-virtual propertiest that accept write, because this properties setters can't be intercepted
                foreach (var P in Source.GetType().GetProperties().Where(IncludeProperty).Where(p => p.CanWrite && !p.GetGetMethod().IsVirtual).Select(x => x.Name))
                    copyProperties.Add(P);
            }


            public Dictionary<string, object> virtualPropertiesValues = new Dictionary<string, object>();

            public void Intercept(IInvocation invocation)
            {
                bool isGet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("get_");
                bool isSet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("set_");
                var propName = (isGet || isSet) ? invocation.Method.Name.Substring("get_".Length) : null;

                if (isSet)
                {
                    var Value = invocation.Arguments[0];
                    virtualPropertiesValues[propName] = Value;
                    copyProperties.Add(propName);
                    return;
                }

                object retValue;
                if (isGet)
                {
                    if (virtualPropertiesValues.TryGetValue(propName, out retValue))
                    {
                        invocation.ReturnValue = retValue;
                        return;
                    }
                }

                object RetValue = invocation.Method.Invoke(Source, invocation.Arguments);

                if (isGet)
                    virtualPropertiesValues.Add(propName, RetValue);

                invocation.ReturnValue = RetValue;
            }
        }

        private static Func<PropertyInfo, bool> allNonVirtualReadWrite = x => x.CanRead && x.CanWrite && !x.GetGetMethod().IsVirtual;

        /// <summary>
        /// Returns a predicate that passes all non virtual properties that have getters and setters. Usefull for creating mementos of ORM models
        /// </summary>
        public static Func<PropertyInfo, bool> AllNonVirtualReadWrite
        {
            get
            {
                return allNonVirtualReadWrite;
            }
        }

        private static Func<PropertyInfo, bool> allNonVirtualReadWriteOrMementoInclude = x =>
        x.GetCustomAttribute<MementoIncludeAttribute>() != null ||
        (x.CanRead && x.CanWrite && !x.GetGetMethod().IsVirtual);

        /// <summary>
        /// Returns a predicate that passes all properties that have the MementoInclude attribute or non virtual properties that have getters and setters.
        /// Usefull for create serialization mementos
        /// </summary>
        public static Func<PropertyInfo, bool> AllNonVirtualReadWriteOrMementoInclude
        {
            get
            {
                return allNonVirtualReadWriteOrMementoInclude;
            }
        }

        private static Func<PropertyInfo, bool> serializationInclude = x =>
         (x.CanRead && x.CanWrite && !x.GetGetMethod().IsVirtual) ||
         (x.CanRead && x.GetGetMethod().IsVirtual);

        /// <summary>
        /// Return a predicate that passes all
        /// non virtual read/write properties or
        /// virtual read only properties. 
        ///  Usefull for serialization
        /// </summary>
        public static Func<PropertyInfo, bool> SerializationInclude
        {
            get
            {
                return serializationInclude;
            }
        }



        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <param name="Instance">Instance to copy</param>
        /// <param name="IncludeProperty">Predicate for properties that should be copied to the memento</param>
        /// <param name="CopyProperties">Contains all properties that passed the IncludeProperty predicate and that should be copied back to the original view model when the change is commited</param>
        /// <returns></returns>
        public static object Create(Type Type, object Instance, out HashSet<string> CopyProperties, Func<PropertyInfo, bool> IncludeProperty)
        {
            var Method = typeof(MementoFactory).GetMethods()
                .Where(
                x => x.Name == nameof(Create) &&
                x.IsGenericMethodDefinition &&
                x.GetParameters().Length == 2
                ).Single().MakeGenericMethod(Type);

            dynamic R = Method.Invoke(null, new object[] { Instance, IncludeProperty });
            CopyProperties = R.ModifiedProperties;
            return R.Instance;
        }

        /// <summary>
        /// Create an instance of type T that have a copy of all properties that pass the IncludeProperty predicate, and property proxies the other ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <typeparam name="T">Type to proxy</typeparam>
        /// <param name="Instance">Instance to copy</param>
        /// <param name="CopyProperties">Contains all properties that should be copied back to the original view model when the change is commited</param>
        /// <returns></returns>
        public static T Create<T>(T Instance, out HashSet<string> CopyProperties, Func<PropertyInfo, bool> IncludeProperty)
            where T : class
        {
            var cP = new HashSet<string>();
            CopyProperties = cP;
            var Interceptor = new ProxyInterceptor(Instance, cP, IncludeProperty);
            var ret = (T)generator.CreateClassProxy(typeof(T), Interceptor);

            //Copy all non virtual properties:
            foreach (var P in typeof(T).GetProperties().Where(IncludeProperty))
            {
                var value = P.GetValue(Instance);
                if (P.CanWrite)
                    P.SetValue(ret, value);
                else
                {
                    if (!P.GetGetMethod().IsVirtual)
                    {
                        throw new ArgumentException($"Read only property '{P.Name}' can't be used by the memento facatory. Only read only virtual properties are allowed");
                    }
                    else
                        Interceptor.virtualPropertiesValues.Add(P.Name, value);
                }
            }
            return ret;
        }

        /// <summary>
        /// The result of a memento creation
        /// </summary>
        /// <typeparam name="T">Type of the memento instance</typeparam>
        public class Memento<T>
        {

            /// <summary>
            /// Memento instance
            /// </summary>
            public T Instance { get; internal set; }

            /// <summary>
            /// Contains all properties that have been modified on the menento instance, in case of commit, this properties 
            /// should be copied back to the original instalce
            /// </summary>
            public IEnumerable<string> ModifiedProperties { get; internal set; }
        }

        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <typeparam name="T">Type to proxy</typeparam>
        /// <param name="Instance">Instance to copy</param>
        /// <returns></returns>
        public static Memento<T> Create<T>(T Instance, Func<PropertyInfo, bool> IncludeProperty)
            where T : class
        {
            HashSet<string> copy;
            T result = Create(Instance, out copy, IncludeProperty);
            return new Memento<T> { Instance = result, ModifiedProperties = copy };
        }
    }
}
