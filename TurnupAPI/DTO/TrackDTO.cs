using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations.Schema;
using TurnupAPI.Models;
using System;
using System.Collections.Generic;

namespace TurnupAPI.DTO
{
    /// <summary>
    /// Représente un objet de transfert de données (DTO) pour une piste musicale.
    /// </summary>
    [NotMapped]
    public class TrackDTO
    {
        /// <summary>
        /// Obtient ou définit l'identifiant unique de la piste musicale.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le titre de la piste musicale.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Obtient ou définit la durée de la piste musicale.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Obtient ou définit la source de la piste musicale.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Obtient ou définit le nom de l'artiste de la piste musicale.
        /// </summary>
        public string ArtistName { get; set; } = null!;

        /// <summary>
        /// Obtient ou définit une liste des noms d'artistes en featuring sur la piste musicale.
        /// </summary>
        public IEnumerable<string> FeaturingArtists { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Obtient ou définit une valeur indiquant si la piste musicale est aimée par l'utilisateur.
        /// </summary>
        public bool IsLiked { get; set; }

        /// <summary>
        /// Obtient ou définit l'URL de l'image de l'artiste de la piste musicale.
        /// </summary>
        public string ArtistPicture { get; set; } = null!;
        /// <summary>
        /// Le nombre d'écoute de lamusique.
        /// </summary>
        public int ListeningCount { get; set; }
    }
}
