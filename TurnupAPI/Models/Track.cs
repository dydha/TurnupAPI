using System;
using System.Collections.Generic;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente une piste de musique.
    /// </summary>
    public class Track
    {
        /// <summary>
        /// L'identifiant unique de la piste.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Le titre de la piste.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Le nombre de minutes de la piste.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Le nombre de secondes de la piste.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// La source de la piste.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// La date à laquelle la piste a été ajoutée.
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.Now;

        //---------------RELATIONS----------------

        /// <summary>
        /// L'identifiant de l'album associé à cette piste.
        /// </summary>
        public int? AlbumId { get; set; }

        /// <summary>
        /// L'album associé à cette piste.
        /// </summary>
        public Album? Album { get; set; }

        /// <summary>
        /// La liste des écoutes utilisateur associées à cette piste.
        /// </summary>
        public List<UserListennedTrack>? UserListennedTracks { get; set; }

        /// <summary>
        /// La liste des pistes favorites utilisateur associées à cette piste.
        /// </summary>
        public List<UserFavoriteTrack>? UserFavoriteTracks { get; set; }

        /// <summary>
        /// La liste des relations piste-playlist associées à cette piste.
        /// </summary>
        public List<PlaylistTrack>? PlaylistTacks { get; set; }

        /// <summary>
        /// La liste des types de piste associés à cette piste.
        /// </summary>
        public List<TrackType>? TrackTypes { get; set; }

        /// <summary>
        /// La liste des artistes associés à cette piste.
        /// </summary>
        public List<TrackArtist>? TrackArtists { get; set; }
    }
}
