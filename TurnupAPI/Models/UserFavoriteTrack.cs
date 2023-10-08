using System;
using TurnupAPI.Areas.Identity.Data;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente la relation entre un utilisateur et une piste de musique qu'il a ajoutée en favori.
    /// </summary>
    public class UserFavoriteTrack
    {
        /// <summary>
        /// L'identifiant unique de la relation utilisateur-piste favori.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// la date à laquelle la piste a été ajoutée en favori par l'utilisateur.
        /// </summary>
        public DateTime LikedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// l'identifiant de l'utilisateur associé à cette relation.
        /// </summary>
        public string? UsersId { get; set; }

        /// <summary>
        /// l'utilisateur associé à cette relation.
        /// </summary>
        public Users? Users { get; set; }

        /// <summary>
        /// l'identifiant de la piste associée à cette relation.
        /// </summary>
        public int TrackId { get; set; }

        /// <summary>
        /// la piste associée à cette relation.
        /// </summary>
        public Track? Track { get; set; }
    }
}
