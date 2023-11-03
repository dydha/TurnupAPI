using TurnupAPI.DTO;

namespace TurnupBlazor.Models
{
    /// <summary>
    /// Représente une liste de lecture de pistes musicales.
    /// </summary>
    public class PlaylingTracks
    {
        /// <summary>
        /// Obtient ou définit le nom de la liste de lecture.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Obtient ou définit la liste des pistes musicales de la liste de lecture.
        /// </summary>
        public IEnumerable<TrackDTO> Tracks { get; set; } = null!;

        /// <summary>
        /// Obtient ou définit l'index de la piste musicale actuellement en cours de lecture.
        /// </summary>
        public int CurrentTrackIndex { get; set; }
    }
}
