using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для отображения событий, а также фильтрации
    /// Для использования модели НЕ в качестве отображения или фильтрации нужно написать валидацию данных
    /// </summary>
    public class PerformanceViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название события")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина названия должна быть от 3 до 400 символов")]
        [Display(Name = "Название", Prompt = "Кольца многочленов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Выберите тип события")]
        [Remote(action: "CheckType", controller: "Performance")]
        [Display(Name = "Тип")]
        public string Type { get; set; }

        [StringLength(250, MinimumLength = 3, ErrorMessage = "Длина поля ключевых слов должна быть от 3 до 250 символов")]
        [Display(Name = "Ключевые слова", Prompt = "Неприводимые многочлены")]
        public string KeyWords { get; set; }

        [Required(ErrorMessage = "Опишите событие")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина описания должна быть от 3 до 400 символов")]
        [Display(Name = "О событии", Prompt = "На данном мастер-классе мы рассмотрим неприводимые многочлены над кольцом полиномов")]
        public string Annotation { get; set; }

        [Required(ErrorMessage = "Введите адрес, где будет проходить событие")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина поля адрес должна быть от 3 до 400 символов")]
        [Display(Name = "Адрес", Prompt = "Ярославль, ул. Союзная 141, ауд. 441")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Выберите дату и время начала")]
        [Remote(action: "CheckStartDate", controller: "Performance", AdditionalFields = nameof(SectionId))]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата начала")]
        public DateTime Start { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Секция")]
        public int? SectionId { get; set; }
        public string UserId { get; set; }
        public List<string> UserRoles { get; set; }
        public string UserDataPath { get; set; }

        public string CreatorName { get; set; }

        /// <summary>
        /// Информация о создателе
        /// </summary>
        public string Info { get; set; }
        public string DataPath { get; set; }
        public string PosterName { get; set; }
        public int Traffic { get; set; }
        
        public bool IsSubscribed { get; set; }
        
        
    }
}
