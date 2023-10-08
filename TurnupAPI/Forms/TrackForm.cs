using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TurnupAPI.Forms
{
    /// <summary>
    /// Représente un formulaire de création de piste musicale.
    /// </summary>
    public class TrackForm
    {
        /// <summary>
        /// Définit le titre de la piste.
        /// </summary>
        [Required]
        public string? Title { get; set; }

        /// <summary>
        /// Définit la durée en minutes de la piste.
        /// </summary>
        [Required]
        public int Minutes { get; set; }

        /// <summary>
        /// Définit la durée en secondes de la piste.
        /// </summary>
        [Required]
        public int Seconds { get; set; }

        /// <summary>
        /// Définit la source de la piste.
        /// </summary>
        [Required]
        public string? Source { get; set; }

        /// <summary>
        /// Définit l'ID de l'artiste principal de la piste.
        /// </summary>
        [Required]
        public int PrincipalArtistId { get; set; }

        /// <summary>
        /// Définit la liste des IDs des artistes en featuring sur la piste.
        /// </summary>
        public List<int>? FeaturingArtists { get; set; }

        /// <summary>
        /// Définit la liste des IDs des types de la piste.
        /// </summary>
        public List<int>? TrackTypes { get; set; }
    }
}
