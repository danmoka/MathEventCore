using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers
{
    public class DataFactory
    {
        public static PerformanceTypes GetPerformanceTypes()
        {
            return new PerformanceTypes();
        }
    }
}
