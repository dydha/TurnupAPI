namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente un album musical.
    /// </summary>
    public class Album
    {
        /// <summary>
        /// l'identifiant unique de l'album.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// le nom de l'album.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// la description de l'album.
        /// </summary>
        public string? Description { get; set; }


        /// <summary>
        /// La date de sortie de l'album.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        //---------RELATIONS------------

        /// <summary>
        /// La liste des pistes associées à cet album.
        /// </summary>
        public List<Track>? Tracks { get; set; }

        /// <summary>
        /// La liste des artistes associés à cet album.
        /// </summary>
        public List<ArtistAlbum>? Albums { get; set; }
    }
}
