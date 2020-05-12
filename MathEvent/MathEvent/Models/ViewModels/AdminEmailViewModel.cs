using System.ComponentModel.DataAnnotations;

namespace MathEvent.Models.ViewModels
{
    public class AdminEmailViewModel
    {
        [Required]
        public string EmailTo { get; set; }

        [Required(ErrorMessage = "Введите тему")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Введите сообщение")]
        public string Message { get; set; }
    }
}
