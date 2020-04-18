using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class FilterViewModel
    {
        public List<CardViewModel> Cards { get; set; }

        public string FilterPatameter { get; set; }
    }
}
