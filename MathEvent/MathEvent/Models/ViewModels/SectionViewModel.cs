using System;
using System.Collections.Generic;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для отображения секций
    /// Для использования модели НЕ в качестве отображения нужно написать валидацию данных
    /// </summary>
    public class SectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Location { get; set; }
        public string DataPath { get; set; }
        public string UserId { get; set; }
        public List<string> UserRoles { get; set; }
        public int ConferenceId { get; set; }
        public List<PerformanceViewModel> PerformanceViewModels { get; set; }
    }
}
