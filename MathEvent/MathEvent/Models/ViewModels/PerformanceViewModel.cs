using System;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для отображения событий, а также фильтрации
    /// Для использования модели НЕ в качестве отображения или фильтрации нужно написать валидацию данных
    /// </summary>
    public class PerformanceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Annotation { get; set; }
        public string KeyWords { get; set; }
        public string Location { get; set; }
        public DateTime Start { get; set; }
        public string CreatorName { get; set; }
        public string DataPath { get; set; }
        public string PosterName { get; set; }
        public int Traffic { get; set; }
        public string UserId { get; set; }
        public bool IsSubscribed { get; set; }
        public string Type { get; set; }
        public string Info { get; set; }
    }
}
