using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire pour la création ou la mise à jour d'un artiste.
    /// </summary>
    public class ArtistForm
    {
        /// <summary>
        /// Définit le nom de l'artiste. Ce champ est requis.
        /// </summary>
        [Required(ErrorMessage = "Le nom de l'artiste est requis.")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Définit la description de l'artiste. Ce champ est requis.
        /// </summary>
        [Required(ErrorMessage = "La description de l'artiste est requise.")]
        public string? Description { get; set; }

        /// <summary>
        /// Définit le pays d'origine de l'artiste. Ce champ est requis.
        /// </summary>
        [Required(ErrorMessage = "Le pays d'origine de l'artiste est requis.")]
        public string Country { get; set; } = null!;

        /// <summary>
        /// Définit l'image de l'artiste.
        /// </summary>
         [Required]
        public string Picture { get; set; } = null!;
    }
}
