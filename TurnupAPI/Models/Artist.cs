using System.Collections.Generic;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente un artiste.
    /// </summary>
    public class Artist
    {
        /// <summary>
        /// L'identifiant unique de l'artiste.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Le nom de l'artiste.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// la description de l'artiste.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// le pays d'origine de l'artiste.
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Le chemin de l'image de l'artiste.
        /// </summary>
        public string? Picture { get; set; }

        //------------RELATIONS---------------

        /// <summary>
        /// La liste des utilisateurs ayant cet artiste en favori.
        /// </summary>
        public List<UserFavoriteArtist>? UserFavoriteArtists { get; set; }

        /// <summary>
        /// la liste des pistes associées à cet artiste.
        /// </summary>
        public List<TrackArtist>? TrackArtists { get; set; }

        /// <summary>
        /// la liste des albums associés à cet artiste.
        /// </summary>
        public List<ArtistAlbum>? Albums { get; set; }
    }
}
