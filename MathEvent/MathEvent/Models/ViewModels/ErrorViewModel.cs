namespace MathEvent.Models
{
    /// <summary>
    /// Данная модель стандартная
    /// Если не будет используется, то можно удалить
    /// </summary>
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
