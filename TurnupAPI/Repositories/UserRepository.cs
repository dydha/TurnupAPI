using Microsoft.EntityFrameworkCore;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Data;
using TurnupAPI.Exceptions;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Repository
{
    /// <summary>
    /// Implémentation du repository pour la gestion des utilisateurs.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly TurnupContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="UserRepository"/>.
        /// </summary>
        /// <param name="context">Le contexte de la base de données.</param>
        public UserRepository(TurnupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère un utilisateur connecté par son adresse e-mail.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur.</param>
        /// <returns>L'utilisateur connecté.</returns>
        public async Task<Users?> GetLoggedUserAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }
        /// <summary>
        /// Récupère un utilisateur connecté par son adresse e-mail.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur.</param>
        /// <returns>L'utilisateur connecté.</returns>
        public async Task<string> GetLoggedUserIdAsync(string email)
        {
            var userId = await (from u in _context.Users where u.Email == email select u.Id).FirstOrDefaultAsync() ;
            return userId ?? string.Empty;
        }
        /// <summary>
        /// Récupère un utilisateur par son id.
        /// </summary>
        /// <param name="id">L'id de l'utilisateur.</param>
        /// <returns>retourne un utilisateur.</returns>
        public async Task<Users?> GetUserAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

      
    }
}
