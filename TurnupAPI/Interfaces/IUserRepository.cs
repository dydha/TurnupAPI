using System.Threading.Tasks;
using TurnupAPI.Areas.Identity.Data;

namespace TurnupAPI.Interfaces
{
    /// <summary>
    /// Interface pour la gestion des opérations liées aux utilisateurs.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Récupère un utilisateur connecté en fonction de son adresse e-mail.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur à récupérer.</param>
        /// <returns>Une tâche asynchrone qui renvoie l'objet Users correspondant à l'adresse e-mail spécifiée.</returns>
        Task<Users> GetLoggedUserAsync(string email);
        /// <summary>
        /// Récupère l'id d'un utilisateur connecté en fonction de son adresse e-mail.
        /// </summary>
        /// <param name="email">L'adresse e-mail de l'utilisateur à récupérer.</param>
        /// <returns>Une tâche asynchrone qui renvoie l'objet l'id de l'utiliseur connecté correspondant à l'adresse e-mail spécifiée.</returns>
        Task<string> GetLoggedUserIdAsync(string email);
        /// <summary>
        /// Récupère un utilisateur par son id.
        /// </summary>
        /// <param name="id">L'id de l'utilisateur.</param>
        /// <returns>retourne un utilisateur.</returns>
         Task<Users> GetUserAsync(string id);
    }
}
