using System;
using TurnupAPI.Areas.Identity.Data;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente la relation entre un utilisateur et un artiste qu'il a ajouté en favori.
    /// </summary>
    public class UserFavoriteArtist
    {
        /// <summary>
        /// L'identifiant unique de la relation utilisateur-artiste favori.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// La date à laquelle l'artiste a été ajouté en favori par l'utilisateur.
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
        /// L'identifiant de l'artiste associé à cette relation.
        /// </summary>
        public int ArtistId { get; set; }

        /// <summary>
        /// l'artiste associé à cette relation.
        /// </summary>
        public Artist? Artist { get; set; }
    }
}
