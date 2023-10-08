using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire d'inscription utilisateur.
    /// </summary>
    public class RegisterForm
    {
        /// <summary>
        /// Définit le prénom de l'utilisateur.
        /// </summary>
        [Required]
        public string? FirstName { get; set; }

        /// <summary>
        /// Définit le nom de famille de l'utilisateur.
        /// </summary>
        [Required]
        public string? LastName { get; set; }

        /// <summary>
        /// Définit le pays de l'utilisateur.
        /// </summary>
        [Required]
        public string? Country { get; set; }

        

        /// <summary>
        /// Définit le genre de l'utilisateur.
        /// </summary>
        [Required]
        public string? Gender { get; set; }

        /// <summary>
        /// Définit la date de naissance de l'utilisateur.
        /// </summary>
        [Required]
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Définit une image de profil de l'utilisateur.
        /// </summary>
        [Required]
        public IFormFile? Picture { get; set; }

        /// <summary>
        /// Définit l'adresse e-mail de l'utilisateur.
        /// </summary>
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// Définit le mot de passe de l'utilisateur.
        /// </summary>
        [Required]
        public string? Password { get; set; }

        /// <summary>
        /// Définit la confirmation du mot de passe de l'utilisateur.
        /// </summary>
        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
