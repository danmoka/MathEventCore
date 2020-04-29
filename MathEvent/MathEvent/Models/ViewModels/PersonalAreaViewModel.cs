using System.Collections.Generic;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для отображения данных в кабинете пользователя
    /// Для использования модели НЕ в качестве отображения нужно написать валидацию данных
    /// </summary>
    public class PersonalAreaViewModel
    {
        public ICollection<Conference> Conferences { get; set; }

        public ICollection<Performance> Performances { get; set; }
    }
}
