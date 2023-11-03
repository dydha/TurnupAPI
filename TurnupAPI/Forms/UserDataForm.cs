using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire de données utilisateur.
    /// </summary>
    public class UserDataForm
    {
        /// <summary>
        /// Définit le prénom de l'utilisateur.
        /// </summary>
        [Required]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Définit le nom de famille de l'utilisateur.
        /// </summary>
        [Required]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Définit le pays de l'utilisateur.
        /// </summary>
        [Required]
        public string Country { get; set; } = null!;

        /// <summary>
        /// Définit le genre de l'utilisateur.
        /// </summary>
        [Required]
        public string Gender { get; set; } = null!;

        /// <summary>
        /// Définit la date de naissance de l'utilisateur.
        /// </summary>
        [Required]
        public DateTime Birthdate { get; set; }


        /// <summary>
        /// Définit l'adresse e-mail de l'utilisateur.
        /// </summary>
        [Required]
        public string Email { get; set; } = null!;
   
        public bool IsDarkTheme { get; set; }
    }
}
