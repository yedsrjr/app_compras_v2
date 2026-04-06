using AppCompras.Models.Data;

namespace AppCompras.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Manter para compatibilidade, mas não deve ser preenchido com a senha real
        public UserRole Role { get; set; }
    }
}