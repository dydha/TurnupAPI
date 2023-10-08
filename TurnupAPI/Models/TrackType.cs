using TurnupAPI.Enums;

namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente la relation entre une piste de musique et un type de piste.
    /// </summary>
    public class TrackType
    {
        /// <summary>
        /// L'identifiant unique de la relation piste-type.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// L'identifiant de la piste associée à cette relation.
        /// </summary>
        public int TrackId { get; set; }

        /// <summary>
        /// La piste associée à cette relation.
        /// </summary>
        public Track? Track { get; set; }

        /// <summary>
        /// L'identifiant du type de piste associé à cette relation.
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Le type de piste associé à cette relation.
        /// </summary>
        public Types? Type { get; set; }
    }
}
