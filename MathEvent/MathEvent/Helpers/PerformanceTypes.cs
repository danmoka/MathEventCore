using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers
{
    public class PerformanceTypes: ADataEnum<string>
    {
        public PerformanceTypes()
        {
            values = new List<string>()
            {
                "Доклад",
                "Мастер-класс",
                "Семинар"
            };
        }
    }
}
