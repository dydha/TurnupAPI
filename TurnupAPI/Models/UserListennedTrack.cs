using System;
using TurnupAPI.Areas.Identity.Data;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente la relation entre un utilisateur et une piste de musique qu'il a écoutée.
    /// </summary>
    public class UserListennedTrack
    {
        /// <summary>
        /// L'identifiant unique de la relation utilisateur-piste écoutée.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// La date à laquelle la piste a été écoutée par l'utilisateur.
        /// </summary>
        public DateTime ListennedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// L'identifiant de l'utilisateur associé à cette relation.
        /// </summary>
        public string? UsersId { get; set; }

        /// <summary>
        /// L'utilisateur associé à cette relation.
        /// </summary>
        public Users? Users { get; set; }

        /// <summary>
        /// l'identifiant de la piste associée à cette relation.
        /// </summary>
        public int TrackId { get; set; }

        /// <summary>
        /// La piste associée à cette relation.
        /// </summary>
        public Track? Track { get; set; }
    }
}
