using TurnupAPI.Enums;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente la relation entre une piste de musique et un artiste.
    /// </summary>
    public class TrackArtist
    {
        /// <summary>
        /// L'identifiant unique de la relation piste-artiste.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// L'identifiant de la piste associée à cette relation.
        /// </summary>
        public int TrackId { get; set; }

        /// <summary>
        /// La piste associée à cette relation.
        /// </summary>
        public Track? Track { get; set; }

        /// <summary>
        /// L'identifiant de l'artiste associé à cette relation.
        /// </summary>
        public int ArtistId { get; set; }

        /// <summary>
        /// l'artiste associé à cette relation.
        /// </summary>
        public Artist? Artist { get; set; }

        /// <summary>
        /// le rôle de l'artiste dans cette piste (ex : rtiste principal, featuring).
        /// </summary>
        public ArtistRole? ArtistRole { get; set; }
    }
}
