using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для смены имени и фамилии пользователя
    /// </summary>
    public class AccountViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100, ErrorMessage = "Имя должно состоять минимум из 1 символа", MinimumLength = 1)]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(100, ErrorMessage = "Фамилия должна состоять минимум из 1 символа", MinimumLength = 1)]
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }

        [StringLength(100, ErrorMessage = "Длина информации о вас должна быть от 3 до 300 символов", MinimumLength = 1)]
        [Display(Name = "О вас")]
        public string UserInfo { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ReturnUrl { get; set; }
    }
}
