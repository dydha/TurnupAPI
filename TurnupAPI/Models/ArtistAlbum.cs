namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente une relation entre un artiste et un album.
    /// </summary>
    public class ArtistAlbum
    {
        /// <summary>
        /// L'identifiant unique de la relation artiste-album.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// L'identifiant de l'artiste associé à cette relation.
        /// </summary>
        public int ArtsistId { get; set; }

        /// <summary>
        /// L'artiste associé à cette relation.
        /// </summary>
        public Artist? Artist { get; set; }

        /// <summary>
        /// L'identifiant de l'album associé à cette relation.
        /// </summary>
        public int AlbumId { get; set; }

        /// <summary>
        /// L'album associé à cette relation.
        /// </summary>
        public Album? Album { get; set; }
    }
}
