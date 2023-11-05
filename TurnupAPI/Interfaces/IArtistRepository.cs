using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TurnupAPI.Forms;
using TurnupAPI.Models;

namespace TurnupAPI.Interfaces
{
    /// <summary>
    /// Interface pour la gestion des opérations sur les artistes.
    /// </summary>
    public interface IArtistRepository
    {
        /// <summary>
        /// Ajoute un artiste.
        /// </summary>
        /// <param name="artist">L'artiste à ajouter.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddAsync(Artist artist);

        /// <summary>
        /// Supprime un artiste par ID.
        /// </summary>
        /// <param name="id">L'ID de l'artiste à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Obtient un artiste par ID.
        /// </summary>
        /// <param name="id">L'ID de l'artiste à obtenir.</param>
        /// <returns>L'artiste correspondant à l'ID spécifié.</returns>
        Task<Artist?> GetAsync(int id);

        /// <summary>
        /// Obtient un artiste via un filtre.
        /// </summary>
        /// <param name="filter">L'ID de l'artiste à obtenir.</param>
        /// <returns>L'artiste correspondant à l'ID spécifié.</returns>
        Task<bool> ArtistExistsAsync(ArtistForm artistForm);

        /// <summary>
        /// Obtient la liste de tous les artistes.
        /// </summary>
        /// <returns>Une liste d'artistes.</returns>
        Task<IEnumerable<Artist>> GetAllAsync(int offset, int limit);

        /// <summary>
        /// Met à jour les informations d'un artiste.
        /// </summary>
        /// <param name="artist">L'artiste avec les informations mises à jour.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task<bool> UpdateAsync(Artist artist);

        /// <summary>
        /// retourne le nom de l'artiste principal d'une piste de musique (Track).
        /// </summary>
        /// <param name="trackId">L'id d'une piste de musique.</param>
        /// <returns>Le nom d'un artiste</returns>
        string GetPrincipalArtistNameByTrackId(int trackId);

        /// <summary>
        /// retourne les nom des artistes en featuring d'une piste de musique (Track).
        /// </summary>
        /// <param name="trackId">L'id d'une piste de musique.</param>
        /// <returns>Une liste de noms des artistes</returns>
        IEnumerable<string> GetFeaturingArtistsNamesByTrackId(int trackId);
        /// <summary>
        /// retourne la photo de l'artiste principal d'une piste de musique (Track).
        /// </summary>
        /// <param name="trackId">L'id d'une piste de musique.</param>
        /// <returns>Le photo d'un artiste</returns>
        string GetPrincipalArtistPictureByTrackId(int trackId);
        /// <summary>
        /// Retourne le nombre de fans d'un artiste.
        /// </summary>
        /// <param name="artistId">L'id de l'artiste.</param>
        /// <returns>Retourne le nombre de fans d'un artiste.</returns>
        int GetArtistFans(int artistId);
    }
}
