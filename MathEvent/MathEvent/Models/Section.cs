using MathEvent.Models.Validators;
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
    //[Date]
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
        [Remote(action: "CheckStartDate", controller: "Section", AdditionalFields = "End,ConferenceId")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата начала")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "Выберите дату и время конца")]
        [Remote(action: "CheckEndDate", controller: "Section", AdditionalFields = "Start,ConferenceId")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата конца")]
        public DateTime End { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string DataPath { get; set; }

        [Required]
        [ForeignKey("Conference")]
        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Конференция")]
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

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    List<ValidationResult> errors = new List<ValidationResult>();

        //    if (Start < DateTime.Now)
        //    {
        //        errors.Add(new ValidationResult("Дата начала меньше текущей даты!", new List<string>() { "Start" }));
        //    }
        //    if (Start > End)
        //    {
        //        errors.Add(new ValidationResult("Дата начала больше даты конца",
        //            new List<string>() { "Start", "End" }));
        //    }
        //    if (End < DateTime.Now)
        //    {
        //        errors.Add(new ValidationResult("Дата конца меньше текущей даты!", new List<string>() { "End" }));
        //    }

        //    return errors;
        //}
    }
}
