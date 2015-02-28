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
        public static void Assign<TSource1, TSource2>(this Tuple<TSource1, TSource2> source, Action<TSource1, TSource2> assignment)
        {
            if (source != null) assignment(source.Item1, source.Item2);
        }
        public static A Get<T, A>(this IDictionary<T, A> dict, T value, A def = default(A))
        {
            return dict.ContainsKey(value) ? dict[value] : def;
        }
        public static T Random<T>(this List<T> items)
        {
            return items[(new Random()).Next(0, items.Count)];
        }
    }
}
