using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class FilterViewModel
    {
        public IEnumerable<CardViewModel> Cards { get; set; }

        public string Type { get; set; }
    }
}
