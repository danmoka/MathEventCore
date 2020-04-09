using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models
{
    [Table("Sections")]
    public class Section
    {
        [Key]
        [Required]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название секции")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Длина названия секции должна быть от 3 до 300 символов")]
        [Display(Name = "Секция")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите адрес секции")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина поля адрес должна быть от 3 до 400 символов")]
        [Display(Name = "Адрес")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Выберите дату и время начала")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата начала")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "Выберите дату и время конца")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата конца")]
        public DateTime End { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string DataPath { get; set; }

        [Required]
        [ForeignKey("Conference")]
        [HiddenInput(DisplayValue = false)]
        public int ConferenceId { get; set; }
        [HiddenInput(DisplayValue = false)]
        public Conference Conference { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ICollection<Performance> Performances { get; set; }

        [ForeignKey("ApplicationUser")]
        [HiddenInput(DisplayValue = false)]
        public string ManagerId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ApplicationUser Manager { get; set; }
    }
}
