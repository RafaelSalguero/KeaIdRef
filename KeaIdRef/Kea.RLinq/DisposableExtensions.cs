using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Extensions
{
    /// <summary>
    /// Disposable extensions
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Provides a useful sintax for manipulating short-lived database conections
        /// </summary>
        /// <typeparam name="TDisposable">Context type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="Db">Context factory</param>
        /// <param name="Func">A function that uses the short-lived context</param>
        /// <returns>Func result</returns>
        public static TResult Using<TDisposable, TResult>(this Func<TDisposable> Db, Func<TDisposable, TResult> Func)
          where TDisposable : IDisposable
        {
            using (var C = Db())
            {
                return Func(C);
            }
        }
    }
}
