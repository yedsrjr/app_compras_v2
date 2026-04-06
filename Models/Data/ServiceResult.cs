namespace AppCompras.Models.Data
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty; // Inicializado para evitar CS8618
        public string Field { get; set; } = string.Empty;   // Inicializado para evitar CS8618
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "")
            => new() { Success = true, Data = data, Message = message };

        public static ServiceResult<T> Fail(string message = "", string field = "")
            => new() { Success = false, Message = message, Field = field };
    }
}
