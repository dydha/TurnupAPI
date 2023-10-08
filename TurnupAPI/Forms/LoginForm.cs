using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire de connexion.
    /// </summary>
    public class LoginForm
    {
        /// <summary>
        /// Définit l'adresse e-mail de l'utilisateur.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        /// <summary>
        /// Définit le mot de passe de l'utilisateur.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
