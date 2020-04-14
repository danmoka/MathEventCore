using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class CardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Annotation { get; set; }
        public DateTime Start { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string DataPath { get; set; }
        public string PosterName { get; set; }
        public int Traffic { get; set; }
        public string UserId { get; set; }
        public bool IsSignedUp { get; set; }
    }
}
