using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace MathEvent.Models.ViewModels
{
    /// <summary>
    /// Данная модель используется для отправки сообщений организатору
    /// </summary>
    public class EmailSendMessageViewModel
    {
        [Required(ErrorMessage = "Не удается определить событие")]
        [HiddenInput(DisplayValue = false)]
        public int PerformanceId { get; set; }
        [Required(ErrorMessage = "Не удается определить организатора")]
        [HiddenInput(DisplayValue = false)]
        public string CreatorEmail { get; set; }

        [Required(ErrorMessage = "Введите ваш Email")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        [Display(Name = "Ваш Email")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Введите сообщение")]
        [StringLength(400, MinimumLength = 3, ErrorMessage = "Длина сообщения должна быть от 3 до 400 символов")]
        [Display(Name = "Сообщение")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Введите тему")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Длина поля темы должна быть от 3 до 150 символов")]
        [Display(Name = "Тема")]
        public string Content { get; set; }
    }
}
