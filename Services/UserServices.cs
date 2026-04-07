using AppCompras.Data;
using AppCompras.Models;
using AppCompras.Models.Data;
using AppCompras.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppCompras.Services
{
    public class UserServices(AppDbContext context)
    {
        public async Task<ServiceResult<UserViewModel>> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return ServiceResult<UserViewModel>.Fail("Usuário e senha obrigatórios");
            }

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username);

            // Validação explícita para evitar avisos de nulidade (CS8602)
            if (user == null)
            {
                return ServiceResult<UserViewModel>.Fail("Usuário ou senha inválidos");
            }

            // Verifica se a senha coincide usando o hash do BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return ServiceResult<UserViewModel>.Fail("Usuário ou senha inválidos");
            }

            if (user.Status == UserStatus.Inativo)
            {
                return ServiceResult<UserViewModel>.Fail("Usuário inativo. Contate o administrador.");
            }

            if (user.Status == UserStatus.Bloqueado)
            {
                return ServiceResult<UserViewModel>.Fail("Usuário bloqueado. Contate o administrador.");
            }

            // Se o usuário for encontrado e a senha verificada, cria o ViewModel
            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Username = user.Username ?? string.Empty,
                Role = user.Role // Carrega o perfil do banco de dados
            };

            return ServiceResult<UserViewModel>.Ok(model, "Login feito com sucesso");
        }

        public async Task<ServiceResult<RegisterViewModel>> Register(RegisterViewModel model)
        {
            if (model == null)
                return ServiceResult<RegisterViewModel>.Fail("Dados Inválidos");

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                return ServiceResult<RegisterViewModel>.Fail("O nome de usuário é obrigatório", "Username");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return ServiceResult<RegisterViewModel>.Fail("A senha é obrigatória", "Password");
            }

            var userExists = await context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Username == model.Username);

            if (userExists)
                return ServiceResult<RegisterViewModel>.Fail("Este nome de usuário já está em uso", "Username");

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // Criptografa a senha antes de salvar no banco
                Name = model.Name,
                Role = UserRole.Comprador, // Todo novo cadastro começa como Comprador por padrão
                Status = UserStatus.Ativo
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return ServiceResult<RegisterViewModel>.Ok(model, "Usuário cadastrado com sucesso");
        }
    }
}
