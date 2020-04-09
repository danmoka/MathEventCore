using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models
{
    [Table("ApplicationUserPerformances")]
    public class ApplicationUserPerformance
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int PerformanceId { get; set; }

        public Performance Performance { get; set; }
    }
}
