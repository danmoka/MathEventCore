using System.ComponentModel.DataAnnotations.Schema;

namespace MathEvent.Models
{
    /// <summary>
    /// Данная модель используется для связи событий и записанных на них пользователей
    /// Изменяется в коде, для использования пользователем нужно написать валидацию
    /// </summary>
    [Table("ApplicationUserPerformances")]
    public class ApplicationUserPerformance
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int PerformanceId { get; set; }
        public Performance Performance { get; set; }
    }
}
