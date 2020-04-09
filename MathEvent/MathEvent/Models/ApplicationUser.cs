using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models
{
    [Table("ApplicationUsers")]
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Введите имя")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Длина имени должна быть от 2 до 150 символов")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Длина фамилии должна быть от 2 до 150 символов")]
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }

        [StringLength(300, MinimumLength = 3, ErrorMessage = "Длина информации о вас должна быть от 3 до 300 символов")]
        [Display(Name = "О вас")]
        public string Info { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string DataPath { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ICollection<ApplicationUserPerformance> ApplicationUserPerformances { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ICollection<Section> Sections { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ICollection<Performance> CreatedPerformances { get; set; }

        [HiddenInput(DisplayValue = false)]
        public ICollection<Conference> Conferences { get; set; }
    }
}
