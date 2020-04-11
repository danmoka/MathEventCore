using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class PersonalAreaViewModel
    {
        public ICollection<Conference> Conferences { get; set; }

        public ICollection<Performance> Performances { get; set; }
    }
}
