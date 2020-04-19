using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class FilterCardViewModel
    {
        public List<PerformanceViewModel> Cards { get; set; }

        public string FilterPatameter { get; set; }
    }
}
