using System;
using TurnupAPI.Models;

namespace TurnupAPI.Models
{
    /// <summary>
    /// La relation entre une piste de musique et une playlist.
    /// </summary>
    public class PlaylistTrack
    {
        /// <summary>
        /// Obtient ou définit l'identifiant unique de la relation piste-playlist.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// La date à laquelle la piste a été ajoutée à la playlist.
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// L'identifiant de la piste associée à cette relation.
        /// </summary>
        public int TrackId { get; set; }

        /// <summary>
        /// La piste associée à cette relation.
        /// </summary>
        public Track? Track { get; set; }

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
