using AppCompras.Data;
using AppCompras.Models.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCompras.Services
{
    public class LogService(AppDbContext context)
    {
        /// <summary>
        /// Registra uma ação do usuário no banco de dados para fins de auditoria.
        /// </summary>
        public async Task RecordLog(int userId, string username, string action, string details, string? ipAddress = null)
        {
            var log = new SystemLog
            {
                UserId = userId,
                Username = username,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            context.Logs.Add(log);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera todos os logs do sistema ordenados pelo mais recente.
        /// </summary>
        public async Task<List<SystemLog>> GetLogs()
        {
            return await context.Logs.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }
    }
}