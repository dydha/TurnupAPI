using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire pour la création ou la mise à jour d'une playlist.
    /// </summary>
    public class PlaylistForm
    {
        /// <summary>
        ///  Définit le nom de la playlist.
        /// </summary>
        [Required]
        public string? Name { get; set; }

        /// <summary>
        /// Définit une valeur indiquant si la playlist est privée.
        /// </summary>
        [Required]
        public bool IsPrivate { get; set; }
    }
}
