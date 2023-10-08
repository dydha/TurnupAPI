using System.Threading.Tasks;
using TurnupAPI.DTO;
using TurnupAPI.Models;

namespace TurnupAPI.Interfaces
{
    /// <summary>
    /// Interface pour la gestion des opérations de favoris (likes) sur les pistes, les artistes et les listes de lecture.
    /// </summary>
    public interface ILikeRepository
    {
        //----------------- TRACK---------------------------------
        /// <summary>
        /// Ajoute un like pour une piste.
        /// </summary>
        /// <param name="uft">L'objet UserFavoriteTrack représentant le like sur la piste.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddTrackLikeAsync(UserFavoriteTrack uft);

        /// <summary>
        /// Supprime un like d'une piste.
        /// </summary>
        /// <param name="uft">L'objet UserFavoriteTrack représentant le like sur la piste à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task RemoveTrackLikeAsync(UserFavoriteTrack uft);

        /// <summary>
        /// Retourne toutes les track aimé par un utilisateur.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>les musique favoris d'un utilisateur.</returns>
        Task<List<Track>> GetUserFavoriteTracks(string userId);

        /// <summary>
        /// Retourne tous les ids des track aimé par un utilisateur.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>Retourne tous les ids des track aimé par un utilisateur</returns>
        Task<List<int>> GetUserFavoriteTracksIdsList(string userId);

        /// <summary>
        /// Retourne un existingTackLike.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>Retourne un userFavoriteTrack s'il existe sinon leve une exception</returns>

        Task<UserFavoriteTrack> GetExistingTrackLike(string userId, int trackId);

        //-----------------END TRACK---------------------------------
        //-----------------Playlist---------------------------------
        /// <summary>
        /// Ajoute un like pour une liste de lecture.
        /// </summary>
        /// <param name="ufp">L'objet UserFavoritePlaylist représentant le like sur la liste de lecture.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddPlaylistLikeAsync(UserFavoritePlaylist ufp);

        /// <summary>
        /// Supprime un like d'une playlist.
        /// </summary>
        /// <param name="ufp">L'objet UserFavoritePlaylist représentant le like sur la liste de lecture à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task RemovePlaylistLikeAsync(UserFavoritePlaylist ufp);

        /// <summary>
        /// Retourne toutes les playlists aimé par un utilisateur.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>les playlists favoris d'un utilisateur.</returns>
        Task<List<Playlist>> GetUserFavoritePlaylists(string userId);

        /// <summary>
        /// Retourne tous les ids des playlist aimé par un utilisateur.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>Retourne tous les ids des playlist aimé par un utilisateur</returns>
        Task<List<int>> GetUserFavoritePlaylistsIdsList(string userId);

        /// <summary>
        /// Retourne un existingPlaylistLike.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <param name="playlistId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>Retourne un userFavoritePlaylist s'il existe sinon leve une exception</returns>

        Task<UserFavoritePlaylist> GetExistingPlaylistLike(string userId, int trackId);

        //-----------------END  Playlist---------------------------------
        //-----------------ARTIST---------------------------------------
        /// <summary>
        /// Ajoute un like pour un artiste.
        /// </summary>
        /// <param name="ufa">L'objet UserFavoriteArtist représentant le like sur l'artiste.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddArtistLikeAsync(UserFavoriteArtist ufa);

        /// <summary>
        /// Supprime un like d'un artiste.
        /// </summary>
        /// <param name="ufa">L'objet UserFavoriteArtist représentant le like sur l'artiste à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task RemoveArtistLikeAsync(UserFavoriteArtist ufa);
        /// <summary>
        /// Retourne tous artistes favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>les artistes favoris d'un utilisateur.</returns>
        Task<List<Artist>> GetUserFavoriteArtists(string userId);

        /// <summary>
        /// Retourne tous les ids des artistes favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>Retourne tous les ids des artistes favoris d'un utilisateur</returns>
        Task<List<int>> GetUserFavoriteArtistsIdsList(string userId);

        /// <summary>
        /// Retourne un existingArtistLike.
        /// </summary>
        /// <param name="userId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <param name="artistId"> userId représente l'identifiant de l'utilisateur.</param>
        /// <returns>Retourne un userFavoriteArtist s'il existe sinon leve une exception</returns>

        Task<UserFavoriteArtist> GetExistingArtistLike(string userId, int artistId);

        //---------------------------END ARTIST---------------------------------
    }
}
