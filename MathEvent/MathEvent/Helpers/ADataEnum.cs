using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers
{
    public abstract class ADataEnum<T>
    {
        protected IEnumerable<T> values = new List<T>() { };

        public IEnumerable<T> GetValues()
        {
            return values;
        }
    }
}
