using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire pour ajouter une piste à une playlist.
    /// </summary>
    [NotMapped]
    public class AddTrackToPlaylistForm
    {
        /// <summary>
        /// Définit l'identifiant unique de la piste à ajouter.
        /// </summary>
        [Required]
        public int TrackId { get; set; }

        /// <summary>
        /// Définit l'identifiant unique de la playlist à laquelle ajouter la piste.
        /// </summary>
        [Required]
        public int PlaylistId { get; set; }
    }
}
