using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models
{
    [Table("Performances")]
    public class Performance
    {
        [Key]
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

        [ForeignKey("Section")]
        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Секция")]
        public int? SectionId { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Секция")]
        public Section Section { get; set; }


        [ForeignKey("ApplicationUser")]
        [HiddenInput(DisplayValue = false)]
        public string CreatorId { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Организатор")]
        public ApplicationUser Creator { get; set; }


        [HiddenInput(DisplayValue = false)]
        public string DataPath { get; set; }


        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Афиша")]
        public string PosterName { get; set; }

        [Required(ErrorMessage = "Некорректное значение")]
        [Range(0, int.MaxValue, ErrorMessage = "Некорректное значение")]
        [Display(Name = "Количество записавшихся")]
        public int Traffic { get; set; }


        [HiddenInput(DisplayValue = false)]
        public ICollection<ApplicationUserPerformance> ApplicationUserPerformances { get; set; }
    }
}
