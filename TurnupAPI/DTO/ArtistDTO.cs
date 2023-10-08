using System.ComponentModel.DataAnnotations.Schema;

namespace TurnupAPI.DTO
{
    /// <summary>
    /// Représente un objet de transfert de données (DTO) pour un artiste.
    /// </summary>
    [NotMapped]
    public class ArtistDTO
    {
        /// <summary>
        /// Obtient l'identifiant unique de l'artiste.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le nom de l'artiste.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Obtient ou définit la description de l'artiste.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Obtient ou définit le pays d'origine de l'artiste.
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Obtient ou définit l'URL de l'image de l'artiste.
        /// </summary>
        public string? Picture { get; set; }
        /// <summary>
        ///Définit si l'utilisateur connecté aime cet artiste.
        /// </summary>
        public bool IsLiked { get; set; }
        /// <summary>
        /// Définit le nombre de fans de l'artiste.
        /// </summary>
        public int FansNumber { get; set; }
    }
}
