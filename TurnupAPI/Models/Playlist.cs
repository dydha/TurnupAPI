using System;
using System.Collections.Generic;
using TurnupAPI.Areas.Identity.Data;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente une playlist de musique.
    /// </summary>
    public class Playlist
    {
        /// <summary>
        /// L'identifiant unique de la playlist.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Le nom de la playlist.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// La date de création de la playlist.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Obtient ou définit si la playlist est privée.
        /// </summary>
        public bool IsPrivate { get; set; }

        //---------------RELATIONS---------------------

        /// <summary>
        /// L'identifiant de l'utilisateur associé à cette playlist.
        /// </summary>
        public string? UsersId { get; set; }

        /// <summary>
        /// L'utilisateur associé à cette playlist.
        /// </summary>
        public Users? Users { get; set; }

        /// <summary>
        /// La liste des playlists favorites associées à cette playlist.
        /// </summary>
        public List<UserFavoritePlaylist>? UserFavoritePlaylists { get; set; }

        /// <summary>
        /// Liste des pistes associées à cette playlist.
        /// </summary>
        public List<PlaylistTrack>? PlaylistTacks { get; set; }
    }
}
