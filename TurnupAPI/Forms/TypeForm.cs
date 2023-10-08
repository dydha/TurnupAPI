using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire de création de type de piste musicale.
    /// </summary>
    public class TypeForm
    {
        /// <summary>
        /// Définit le nom du type.
        /// </summary>
        [Required]
        public string? Name { get; set; }

        /// <summary>
        /// Définit l'image associée au type.
        /// </summary>
        [Required]
        public string? Picture { get; set; }
    }
}
