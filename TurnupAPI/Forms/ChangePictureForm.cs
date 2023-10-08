using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    public class ChangePictureForm
    {
        /// <summary>
        /// Définit une image de profil de l'utilisateur.
        /// </summary>
        [Required]
        public IFormFile? Picture { get; set; }
    }
}
