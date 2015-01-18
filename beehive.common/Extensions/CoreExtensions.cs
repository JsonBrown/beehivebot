using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.common.Extensions
{
    public static class CoreExtensions
    {
        public static TResult Select<TSource1, TSource2, TResult>(this Tuple<TSource1, TSource2> source, Func<TSource1, TSource2, TResult> selector)
        {
            return (source != null) ? selector(source.Item1, source.Item2) : default(TResult);
        }
        public static A Get<T, A>(this Dictionary<T, A> dict, T value, A def = default(A))
        {
            return dict.ContainsKey(value) ? dict[value] : def;
        }
    }
}
