using System.ComponentModel.DataAnnotations.Schema;

namespace TurnupAPI.DTO
{
    /// <summary>
    /// Représente un objet de transfert de données (DTO) pour un type.
    /// </summary>
    [NotMapped]
    public class TypesDTO
    {
        /// <summary>
        /// Obtient ou définit l'identifiant unique du type.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtient ou définit le nom du type.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Obtient ou définit l'URL de l'image du type.
        /// </summary>
        public string Picture { get; set; } = string.Empty;
    }
}
