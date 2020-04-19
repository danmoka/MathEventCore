using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class SectionViewModel
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public string Location { get; set; }
        public List<PerformanceViewModel> PerformanceViewModels { get; set; }
    }
}
