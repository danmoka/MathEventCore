using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathEvent.Models
{
    [Table("Conferences")]
    public class Conference
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Длина названия должна быть от 3 до 300 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите адрес, где будет проходить конференция")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина поля адрес должна быть от 3 до 400 символов")]
        [Display(Name = "Адрес")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Выберите дату и время начала")]
        [Remote(action: "CheckStartDate", controller: "Conference", AdditionalFields = nameof(End))]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата начала")]
        public DateTime Start { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string DataPath { get; set; }

        [Required(ErrorMessage = "Выберите дату и время конца")]
        [Remote(action: "CheckEndDate", controller: "Conference", AdditionalFields = nameof(Start))]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата конца")]
        public DateTime End { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Секции")]
        public ICollection<Section> Sections { get; set; }

        [ForeignKey("ApplicationUser")]
        [HiddenInput(DisplayValue = false)]
        public string ManagerId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ApplicationUser Manager { get; set; }
    }
}
