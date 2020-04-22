using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.ViewModels
{
    public class EmailSendMessageViewModel
    {
        public int PerformanceId { get; set; }
        public string CreatorEmail { get; set; }

        [Required(ErrorMessage = "Введите ваш Email")]
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
