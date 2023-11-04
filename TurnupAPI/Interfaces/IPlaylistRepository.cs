using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TurnupAPI.Forms;
using TurnupAPI.Models;

namespace TurnupAPI.Interfaces
{
    /// <summary>
    /// Interface pour la gestion des opérations CRUD (Create, Read, Update, Delete) sur les playlist.
    /// </summary>
    public interface IPlaylistRepository
    {
        /// <summary>
        /// Ajoute une playlist.
        /// </summary>
        /// <param name="playlist">L'objet Playlist à ajouter.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddAsync(Playlist playlist);

        /// <summary>
        /// Supprime une playlist en fonction de son ID.
        /// </summary>
        /// <param name="id">L'ID de la playlist à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Récupère une playlist en fonction de son ID.
        /// </summary>
        /// <param name="id">L'ID de la playlist à récupérer.</param>
        /// <returns>Une tâche asynchrone qui renvoie l'objet Playlist correspondant à l'ID spécifié.</returns>
        Task<Playlist?> GetAsync(int id);

        /// <summary>
        /// Obtient une playlist via un filtre.
        /// </summary>
        /// <param name="filter">L'expression.</param>
        /// <returns>La playlist correspondant à la recherche.</returns>
        Task<Playlist?> PlaylistExistsAsync(PlaylistForm input, string loggedUserId);

        /// <summary>
        /// Récupère toutes les listes de lecture.
        /// </summary>
        /// <returns>Une tâche asynchrone qui renvoie une liste de toutes les listes de lecture disponibles.</returns>
        Task<IEnumerable<Playlist>> GetAllAsync(int offset, int limit);

        /// <summary>
        /// Met à jour les informations d'une playlist.
        /// </summary>
        /// <param name="playlist">L'objet Playlist contenant les informations mises à jour.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task<bool> UpdateAsync(Playlist playlist);

        /// <summary>
        /// Récupère toutes les playlists d'un utilisateur.
        /// </summary>
        /// <returns>Une tâche asynchrone qui renvoie une liste de toutes les playlists d'un utilisateur.</returns>

        Task<IEnumerable<Playlist>> GetPlaylistByUserIdAsync(string userId, int offset, int limit);
    }
}
