using System.ComponentModel.DataAnnotations;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для регистрации пользователя
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage ="Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100, ErrorMessage = "Имя должно состоять минимум из 1 символа", MinimumLength = 1)]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(100, ErrorMessage = "Фамилия должна состоять минимум из 1 символа", MinimumLength = 1)]
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }


        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль должен состоять минимум из 6 символов", MinimumLength = 6)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        public string PasswordConfirm { get; set; }
    }
}
