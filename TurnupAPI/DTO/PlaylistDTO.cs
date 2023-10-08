using System.ComponentModel.DataAnnotations.Schema;

namespace TurnupAPI.DTO
{
    /// <summary>
    /// Représente un objet de transfert de données (DTO) pour une playlist.
    /// </summary>
    [NotMapped]
    public class PlaylistDTO
    {
        /// <summary>
        /// Obtient ou définit l'identifiant unique de la playlist.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le nom de la playlist.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Obtient ou définit une valeur indiquant si la playlist est privée.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Obtient ou définit l'id du propriéraire de la playlist.
        /// </summary>
        public string? OwnerId { get; set; }
        /// <summary>
        /// Obtient ou définit la photo du propriéraire de la playlist.
        /// </summary>
        public byte[]? OwnerPicture { get; set; }
        /// <summary>
        /// Obtient ou définit le nom du propriéraire de la playlist.
        /// </summary>
        public string? OwnerName { get; set; }
        /// <summary>
        /// Obtient ou définit si la playlist est déjà aimé par l'utilisateur connecté.
        /// </summary>
        public bool IsLiked { get; set; }
        /// <summary>
        /// Obtient ou définit la date de création de la playlist.
        /// </summary>
        public DateTime CreatdAt { get; set; }
    }
}
