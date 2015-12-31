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
        public class Statistics
        {
            public long VirtualPropertyGetterTime;
            public long NonVirtualPropertyGetterTime;
        }
        public static bool EnableStatistics = false;
        public static Statistics Stats = new Statistics();

        [ThreadStatic]
        private static ProxyGenerator _generator;
        private static ProxyGenerator generator
        {
            get
            {
                if (_generator == null)
                    _generator = new ProxyGenerator();
                return _generator;
            }
        }

        class ProxyInterceptor : IInterceptor
        {
            /// <summary>
            /// Contains all properties that should be copied back to the original model
            /// </summary>
            readonly HashSet<string> copyProperties;


            readonly object Source;
            public ProxyInterceptor(object Source, HashSet<string> copyProperties, Func<PropertyInfo, bool> IncludeProperty)
            {
                this.copyProperties = copyProperties;
                this.Source = Source;

                //Add all non-virtual propertiest that accept write, because this properties setters can't be intercepted
                foreach (var P in Source.GetType().GetProperties().Where(IncludeProperty).Where(p => p.CanWrite && !p.GetGetMethod().IsVirtual).Select(x => x.Name))
                    copyProperties.Add(P);
            }


            Dictionary<string, object> virtualPropertiesValues = new Dictionary<string, object>();
            public void AddVirtualPropertyValue(string PropertyName, object Value)
            {
                virtualPropertiesValues[PropertyName] = Value;
            }

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

        #region Property predicates

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


        private static Func<PropertyInfo, bool> virtuals = x => x.GetGetMethod().IsVirtual;

        /// <summary>
        /// Returns a predicate that passes all virtual properties
        /// </summary>
        public static Func<PropertyInfo, bool> Virtuals
        {
            get
            {
                return virtuals;
            }
        }
        #endregion



        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <param name="Instance">Instance to copy</param>
        /// <param name="IncludeProperty">Predicate for properties that should be copied to the memento</param>
        /// <param name="ExcludeProperty">Properties that will be explicitly copied to the memento as null or default values at the moment of its creation.
        /// <param name="CopyProperties">Contains all properties that passed the IncludeProperty predicate and that should be copied back to the original view model when the change is commited</param>
        /// <returns></returns>
        public static object Create(Type Type, object Instance, out HashSet<string> CopyProperties, Func<PropertyInfo, bool> IncludeProperty, Func<PropertyInfo, bool> ExcludeProperty)
        {
            var Method = typeof(MementoFactory).GetMethods()
                .Where(
                x => x.Name == nameof(Create) &&
                x.IsGenericMethodDefinition &&
                x.GetParameters().Length == 3
                ).Single().MakeGenericMethod(Type);

            dynamic R = Method.Invoke(null, new object[] { Instance, IncludeProperty, ExcludeProperty });
            CopyProperties = R.ModifiedProperties;
            return R.Instance;
        }

        static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Create an instance of type T that have a copy of all properties that pass the IncludeProperty predicate, and property proxies the other ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <typeparam name="T">Type to proxy</typeparam>
        /// <param name="Instance">Instance to copy</param>
        /// <param name="CopyProperties">Contains all properties that should be copied back to the original view model when the change is commited</param>
        /// <param name="IncludeProperty">Properties that will be explicitly copied to the memento at the moment of its creation</param>
        /// <param name="ExcludeProperty">Properties that will be explicitly copied to the memento as null or default values at the moment of its creation.
        /// ExcludedProperties takes precedence over IncludeProperty</param>
        /// <returns></returns>
        public static T Create<T>(T Instance, out HashSet<string> CopyProperties, Func<PropertyInfo, bool> IncludeProperty, Func<PropertyInfo, bool> ExcludeProperty)
            where T : class
        {
            var cP = new HashSet<string>();
            CopyProperties = cP;
            var Interceptor = new ProxyInterceptor(Instance, cP, IncludeProperty);
            var ret = (T)generator.CreateClassProxy(typeof(T), Interceptor);

            //Copy all non virtual properties:
            foreach (var P in typeof(T).GetProperties().Where(x => IncludeProperty(x) || ExcludeProperty(x)))
            {
                object value;

                if (ExcludeProperty(P))
                {
                    value = GetDefault(P.PropertyType);
                }
                else
                {
                    try
                    {
                        value = P.GetValue(Instance);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new ArgumentException($"Exception while calling the property getter '{P.Name}'", ex.InnerException);
                    }
                }
                if (P.CanWrite)
                {
                    try
                    {
                        P.SetValue(ret, value);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new ArgumentException($"Exception while calling the property setter '{P.Name}'", ex.InnerException);
                    }
                }
                else
                {
                    if (!P.GetGetMethod().IsVirtual)
                    {
                        throw new ArgumentException($"Read only property '{P.Name}' can't be used by the memento facatory. Only read only virtual properties are allowed");
                    }
                    else
                        Interceptor.AddVirtualPropertyValue(P.Name, value);
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
        /// <param name="IncludeProperty">Properties that will be explicitly copied to the memento at the moment of its creation</param>
        /// <param name="ExcludeProperty">Properties that will be explicitly copied to the memento as null or default values at the moment of its creation.
        /// ExcludedProperties takes precedence over IncludeProperty</param>
        /// <param name="Instance">Instance to copy</param>
        /// <returns></returns>
        public static Memento<T> Create<T>(T Instance, Func<PropertyInfo, bool> IncludeProperty, Func<PropertyInfo, bool> ExcludeProperty)
            where T : class
        {
            HashSet<string> copy;
            T result = Create(Instance, out copy, IncludeProperty, ExcludeProperty);
            return new Memento<T> { Instance = result, ModifiedProperties = copy };
        }

        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <typeparam name="T">Type to proxy</typeparam>
        /// <param name="IncludeProperty">Properties that will be explicitly copied to the memento at the moment of its creation</param>
        /// ExcludedProperties takes precedence over IncludeProperty</param>
        /// <param name="Instance">Instance to copy</param>
        /// <returns></returns>
        public static Memento<T> Create<T>(T Instance, Func<PropertyInfo, bool> IncludeProperty)
            where T : class
        {
            return Create(Instance, IncludeProperty, P => false);
        }
    }
}
