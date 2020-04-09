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
        [Required]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название события")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина названия должна быть от 3 до 400 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Выберите тип события")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Недопустимый тип события")]
        public string Type { get; set; }

        [StringLength(250, MinimumLength = 3, ErrorMessage = "Длина поля ключевых слов должна быть от 3 до 250 символов")]
        [Display(Name = "Ключевые слова")]
        public string KeyWords { get; set; }

        [Required(ErrorMessage = "Опишите событие")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина описания должна быть от 3 до 400 символов")]
        [Display(Name = "О событии")]
        public string Annotation { get; set; }

        [Required(ErrorMessage = "Выберите дату и время начала")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата начала")]
        public DateTime Start { get; set; }

        [ForeignKey("Section")]
        [HiddenInput(DisplayValue = false)]
        public int? SectionId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public Section Section { get; set; }

        [ForeignKey("ApplicationUser")]
        [HiddenInput(DisplayValue = false)]
        public string CreatorId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ApplicationUser Creator { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string DataPath { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PosterName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ICollection<ApplicationUserPerformance> ApplicationUserPerformances { get; set; }
    }
}
