using System;
using TurnupAPI.Areas.Identity.Data;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente la relation entre un utilisateur et une playlist qu'il a ajoutée en favori.
    /// </summary>
    public class UserFavoritePlaylist
    {
        /// <summary>
        /// L'identifiant unique de la relation utilisateur-playlist favori.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// La date à laquelle la playlist a été ajoutée en favori par l'utilisateur.
        /// </summary>
        public DateTime LikedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// L'identifiant de l'utilisateur associé à cette relation.
        /// </summary>
        public string? UsersId { get; set; }

        /// <summary>
        /// L'utilisateur associé à cette relation.
        /// </summary>
        public Users? Users { get; set; }

        /// <summary>
        /// L'identifiant de la playlist associée à cette relation.
        /// </summary>
        public int PlaylistId { get; set; }

        /// <summary>
        /// La playlist associée à cette relation.
        /// </summary>
        public Playlist? Playlist { get; set; }
    }
}
