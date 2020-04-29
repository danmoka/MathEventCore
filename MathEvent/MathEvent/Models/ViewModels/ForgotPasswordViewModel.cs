using System.ComponentModel.DataAnnotations;

namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется при вводе Email пользователя при смене пароля
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
