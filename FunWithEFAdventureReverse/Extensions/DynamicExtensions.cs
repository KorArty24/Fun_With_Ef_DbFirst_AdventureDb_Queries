using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithEFAdventureReverse.Extensions
{
    public static class DynamicExtensions
    {
        public static T? DynamicSum<T>(this IEnumerable<T?> source) where T : struct
        {
            T total = default(T);
            foreach (T? item in source)
            {
                if (item != null)
                {
                    dynamic value = item.Value;
                    total = (T) (total + value);
                }
            }
            return total;
        }
        public static dynamic VeryDynamicSum(this object source)        
        {
            dynamic dynamicSource = source;
            return DynamicSum(dynamicSource);
        }
    }
}
