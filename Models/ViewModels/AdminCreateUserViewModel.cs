using System.ComponentModel.DataAnnotations;
using AppCompras.Models.Data;

namespace AppCompras.Models.ViewModels
{
    public class AdminCreateUserViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome deve ter no máximo 50 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "O nome de usuário deve ter entre 3 e 30 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "O perfil é obrigatório")]
        public UserRole Role { get; set; } = UserRole.Comprador;

        [Required(ErrorMessage = "O status é obrigatório")]
        public UserStatus Status { get; set; } = UserStatus.Ativo;
    }
}
