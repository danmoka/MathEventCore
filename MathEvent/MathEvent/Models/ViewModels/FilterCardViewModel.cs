using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для фильтрации карточек событий
    /// </summary>
    public class FilterCardViewModel
    {
        [Required]
        public List<PerformanceViewModel> Cards { get; set; }

        public string FilterPatameter { get; set; }
    }
}
