namespace TurnupAPI.Models
{
    /// <summary>
    /// Représente un type de piste de musique.
    /// </summary>
    public class Types
    {
        /// <summary>
        /// L'identifiant unique du type de piste.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Le nom du type de piste.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Le chemin de l'image du type de piste.
        /// </summary>
        public string Picture { get; set; } = null!;


        /// <summary>
        /// La liste des relations entre ce type de piste et les pistes associées.
        /// </summary>
        public List<TrackType>? TrackTypes { get; set; }
    }
}
