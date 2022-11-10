using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithEFAdventureReverse.Extensions
{
    public static class StartsWithAny
    {
        public static bool StartsWithAnyFromArr(this string str, char[] ch)
        {
            return ch.Any(s=>str.StartsWith(s));
        }

        public static bool StartsWithAnyFrom(this string str, char[] ch)
        {
            foreach (char ch2 in ch)
            {
                return str.StartsWith(ch2);
            }
            return false;
        }
    }
}
