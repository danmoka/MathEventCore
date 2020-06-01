using System.Collections.Generic;


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
