using System.Collections.Generic;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для фильтрации карточек событий
    /// </summary>
    public class FilterCardViewModel
    {
        public List<PerformanceViewModel> Cards { get; set; }
        public string FilterPatameter { get; set; }
    }
}
