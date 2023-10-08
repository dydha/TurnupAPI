using System.Collections.Generic;
using System.Threading.Tasks;
using TurnupAPI.Forms;
using TurnupAPI.Models;

namespace TurnupAPI.Interfaces
{
    /// <summary>
    /// Interface pour la gestion des opérations CRUD (Create, Read, Update, Delete) sur les pistes musicales.
    /// </summary>
    public interface ITrackRepository
    {
        /// <summary>
        /// Ajoute une piste musicale.
        /// </summary>
        /// <param name="track">L'objet Track à ajouter.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddAsync(Track track);

        /// <summary>
        /// Récupère une piste musicale en fonction de son ID.
        /// </summary>
        /// <param name="id">L'ID de la piste musicale à récupérer.</param>
        /// <returns>Une tâche asynchrone qui renvoie l'objet Track correspondant à l'ID spécifié.</returns>
        Task<Track> GetAsync(int id);
        /// <summary>
        /// Retourne le nombre d'écoute d'une musique.
        /// </summary>
        int GetTrackListeningNumber(int trackId);

        /// <summary>
        /// Supprime une piste musicale en fonction de son ID.
        /// </summary>
        /// <param name="id">L'ID de la piste musicale à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Met à jour les informations d'une piste musicale.
        /// </summary>
        /// <param name="track">L'objet Track contenant les informations mises à jour.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task UpdateAsync(Track track);

        /// <summary>
        /// Récupère toutes les pistes musicales disponibles.
        /// </summary>
        /// <returns>Une tâche asynchrone qui renvoie une liste de toutes les pistes musicales.</returns>
        Task<List<Track>> GetAllAsync();

        /// <summary>
        /// Récupère toutes les pistes musicales d'une playlist.
        /// </summary>
        /// <returns>Une tâche asynchrone qui renvoie une liste de toutes les pistes musicales d'une playlist.</returns>

        Task<List<Track>> GetTracksByPlaylistAsync(int playlistId);
        /// <summary>
        /// Récupère l'historique d'écoute de l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne l'historique d'écoute de l'utilisateur connecté</returns>
        Task<List<Track>> GetUserListeningHistory(string userId);

        /// <summary>
        /// Récupère les musique non écoutées par  l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne une suggestion de musique à l'utilisateur connecté</returns>
        Task<List<Track>> GetDiscoveryAsync(string userId);

        /// <summary>
        /// Récupère les musiques populaires : les plus écoutes.
        /// </summary>
        /// <returns>Retourne les musiques populaires : les plus écoutes</returns>
        Task<List<Track>> GetPopularTracksAsync();

        /// <summary>
        /// Supprime une musique d'unr playlist.
        /// </summary>
        
        Task DeleteTrackFromPlaylistAsync(AddTrackToPlaylistForm input, string userId);

        /// <summary>
        /// Récupère et retourne les musique d'un Types.
        /// </summary>

        Task<List<Track>> GetTracksByTypesAsync(int typesId);
    }
}
