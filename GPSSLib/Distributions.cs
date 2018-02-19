using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSSLib
{    
    class Distributions
    {
        public static string Dist(string dist, params double[] values)
        {
            return string.Format("{0}({1}}", dist, string.Join(",", values));
        }
    }
}
