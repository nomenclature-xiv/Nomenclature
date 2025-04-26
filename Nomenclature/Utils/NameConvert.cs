using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nomenclature.Utils
{
    public class NameConvert
    {
        public static (string, string) ToTuple(string name)
        {
            var arr = name.Split("@");
            return (arr[0], arr[1]);
        }
        public static string ToString((string, string) name)
        {
            return name.Item1 + "@" + name.Item2;
        }
    }
}
