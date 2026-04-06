using System;

namespace AppCompras.Models.Data
{
    public class SystemLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Ex: "Consulta por Solicitação", "Busca por Item"
        public string Details { get; set; } = string.Empty; // Detalhes da busca (ex: Código consultado)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? IpAddress { get; set; }
    }
}