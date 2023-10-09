using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnupAPI.DTO
{
    /// <summary>
    /// Représente un objet de transfert de données (DTO) pour un utilisateur.
    /// </summary>
    [NotMapped]
    public class UserDTO
    {
        /// <summary>
        /// Obtient ou définit l'identifiant unique de l'utilisateur.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Obtient ou définit le genre de l'utilisateur.
        /// </summary>
        public string? Gender { get; set; }


        /// <summary>
        /// Obtient ou définit le pays de résidence de l'utilisateur.
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Obtient ou définit l'image de profil de l'utilisateur sous forme de tableau d'octets.
        /// </summary>
        public byte[]? Picture { get; set; }

        /// <summary>
        /// Obtient ou définit le nom complet de l'utilisateur.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Obtient ou définit une valeur indiquant si l'utilisateur utilise un thème sombre.
        /// </summary>
        public bool IsDarkTheme { get; set; }

        /// <summary>
        /// Obtient ou définit la date de naissance de l'utilisateur.
        /// </summary>
        public DateTime BirthDate { get; set; }
    }
}
