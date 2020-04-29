using System;
using System.Collections.Generic;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется только для отображения конференция
    /// Для использования модели НЕ в качестве отображения нужно написать валидацию данных
    /// </summary>
    public class ConferenceViewModel
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime Start { get; set; }
        public List<SectionViewModel> SectionViewModels { get; set; }
    }
}
