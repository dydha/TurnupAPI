using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Repositories
{
    /// <summary>
    /// Repository pour la gestion des Track.
    /// </summary>
    public class TrackRepository : ITrackRepository
    {
        private readonly TurnupContext _context;
        /// <summary>
        /// Constructeur de la classe.
        /// </summary>
        /// <param name="context">Le contexte de la base de données.</param>
        public TrackRepository(TurnupContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Ajoute un objet Track dans la base de données.
        /// </summary>
        /// <param name="track">L'objet Track à ajouter.</param>
        public async Task AddAsync(Track track)
        {   
                _context.Track.Add(track);
                await _context.SaveChangesAsync();       
        }
        /// <summary>
        /// Récupère une Track par son ID.
        /// </summary>
        /// <param name="id">L'ID de la track  à récupérer.</param>
        /// <returns>La Track trouvée ou null si elle n'existe pas.</returns>
        /// <exception cref="NotFoundException">Si l'artiste n'est pas trouvé.</exception>
        public async Task<Track?> GetAsync(int id)
        {
            var track = await _context.Track
                               .Where(t => t.Id == id)
                               .Include(t => t.UserFavoriteTracks)
                               .AsSplitQuery()
                               .FirstOrDefaultAsync();

            return track;
        }

        /// <summary>
        /// Supprime une Track de la base de données par son ID.
        /// </summary>
        /// <param name="id">L'ID de la Track à supprimer.</param>
        public async Task<bool> DeleteAsync(int id)
        {
            bool result = false;
            var track = await GetAsync(id);
            if(track is not null)
            {
                _context.Track.Remove(track);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;

        }
        /// <summary>
        /// Met à jour une Track dans la base de données.
        /// </summary>
        /// <param name="track">La track à mettre à jour.</param>
        public async Task<bool> UpdateAsync(Track track)
        {
            bool result = false;
            var existingTrack = await GetAsync(track.Id);
            if (existingTrack is not  null)
            {
                _context.Track.Update(existingTrack);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;          
        }

        /// <summary>
        /// Récupère la liste de toutes les tracks.
        /// </summary>
        /// <returns>La liste de toutes les tracks ou une liste vide si aucune playlist n'est trouvée.</returns>
        public async Task<IEnumerable<Track>> GetAllAsync(int offset, int limit)
        {
         
            var tracks = await _context.Track
                                    .Skip(offset)
                                    .Take(limit)
                                    .AsNoTracking()                                   
                                    .ToListAsync();
            return (tracks is not null && tracks.Any()) ? tracks : Enumerable.Empty<Track>();
        }
        public async Task<IEnumerable<Track>> GetNewTracks(int offset, int limit)
        {
            var lastMonth = DateTime.Now.AddMonths(-1);
            var tracks = await _context.Track
                            .Where(t => t.AddedAt >= lastMonth)
                            .OrderByDescending(t => t.AddedAt)
                            .Skip(offset)
                            .Take(limit)  
                            .AsNoTracking()
                            .ToListAsync();
            return (tracks is not null && tracks.Any()) ? tracks : Enumerable.Empty<Track>();
        }
        /// <summary>
        /// Récupère la liste de toutes les tracks d'une playlist.
        /// </summary>
        /// <returns>La liste de toutes les tracks ou une liste vide si aucune playlist n'est trouvée.</returns>
        public async Task<IEnumerable<Track>> GetTracksByPlaylistAsync(int playlistId, int offset, int limit)
        {

            var tracks = await (from track in _context.Track
                          join pt in _context.PlaylistTrack on track.Id equals pt.TrackId
                          where pt.PlaylistId == playlistId
                          select track)
                          .Skip(offset)
                          .Take(limit)
                          .AsNoTracking()
                          .ToListAsync();
            return (tracks is not  null && tracks.Any()) ? tracks : Enumerable.Empty<Track>();
        }

        /// <summary>
        /// Récupère l'historique d'écoute de l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne l'historique d'écoute de l'utilisateur connecté..</returns>
        public async Task<IEnumerable<Track>> GetUserListeningHistory(string userId, int offset, int limit)
        {
            var ids = await GetUserListenedTracksIds(userId);
            var historicTracks = Enumerable.Empty<Track>();
            if (ids.Any())
            {
                historicTracks = (await _context.Track.AsNoTracking().ToListAsync())
                                .Where(t => ids.Contains(t.Id))
                                .OrderBy(t => ids.IndexOf(t.Id))
                                .Skip(offset)
                                .Take(limit)
                                .AsEnumerable();
                                                   
            }
            return historicTracks;
        }
        /// <summary>
        /// Récupère les musique non écoutées par  l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne une suggestion de musique à l'utilisateur connecté</returns>


        public async Task<IEnumerable<Track>> GetDiscoveryAsync(string userId, int offset, int limit)
        {
            var tracksFiltered = Enumerable.Empty<Track>();
            var ids = await GetUserListenedTracksIds(userId);
            if(ids.Any())
            {
                tracksFiltered = (await _context.Track.AsNoTracking().ToListAsync())
                                .Where(t => !ids.Contains(t.Id))
                                .Skip(offset)
                                .Take(limit)
                                .AsEnumerable();
            }
           
            return tracksFiltered;
        }

        // <summary>
        /// Récupère les musiques populaires : les plus écoutes.
        /// </summary>
        /// <returns>Retourne les musiques populaires : les plus écoutes</returns>
        public async Task<IEnumerable<Track>> GetPopularTracksAsync(int offset, int limit)
        {
            var popularTracks = Enumerable.Empty<Track>();
            var ulttrackIds = await _context.UserListennedTrack.Select(ult => ult.TrackId).ToListAsync(); 
            if(ulttrackIds is not null &&  ulttrackIds.Any())
            {
                Dictionary<int, int> trackIdAndListennedCount = new();
                foreach (var trackId in ulttrackIds)
                {
                    if (trackIdAndListennedCount.ContainsKey(trackId))
                    {
                        trackIdAndListennedCount[trackId]++;
                    }
                    else
                    {
                        trackIdAndListennedCount.Add(trackId, 1);
                    }
                }
                var trackIds = trackIdAndListennedCount
                                        .OrderByDescending(kvp => kvp.Value)
                                        .Select(kvp => kvp.Key)
                                        .Take(100)
                                        .ToList();
                if(trackIds is not null && trackIds.Any())
                {
                    popularTracks = (await _context.Track.AsNoTracking().ToListAsync())
                                    .Where(t => trackIds.Contains(t.Id))
                                    .OrderBy(t => trackIds.IndexOf(t.Id))
                                    .Skip(offset)
                                    .Take(limit)
                                    .AsEnumerable();
                }
            }
        
            return popularTracks;
        }
        /// <summary>
        /// Retourne le nombre d'écoute d'une musique.
        /// </summary>
        public int GetTrackListeningNumber(int trackId)
        {
            var number = _context.UserListennedTrack.Where(ult => ult.TrackId == trackId).Count();
            return number;
        }
        /// <summary>
        /// Supprime une musique d'unr playlist.
        /// </summary>
        public async Task<bool> DeleteTrackFromPlaylistAsync(AddTrackToPlaylistForm input, string userId)
        {
            bool result = false;
            var playlist = await _context.Playlist
                                    .Where(p => p.Id == input.PlaylistId && p.UsersId == userId)
                                    .FirstOrDefaultAsync();
            if (playlist is not  null)
            {
                var track = await GetAsync(input.TrackId);
                if(track is not null)
                {
                    var existingTrackInPlaylist = await _context.PlaylistTrack
                                                            .Where(pt => pt.PlaylistId == playlist.Id && pt.TrackId == track.Id)
                                                            .FirstOrDefaultAsync();
                    if (existingTrackInPlaylist is not  null)
                    {
                        _context.PlaylistTrack.Remove(existingTrackInPlaylist);
                        await _context.SaveChangesAsync();
                        result = true;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Récupère et retourne les musique d'un Types.
        /// </summary>
        public async Task<IEnumerable<Track>> GetTracksByTypesAsync(int typesId, int offset, int limit)
        {
            var tracks = Enumerable.Empty<Track>();
            var ids = await _context.TrackType
                                    .Where(tt => tt.TypeId == typesId)
                                    .Select(tt => tt.TrackId)
                                    .ToListAsync();
            if(ids is not null && ids.Any())
            {
                tracks = (await GetAllAsync(offset, limit))
                                   .Where(t => ids.Contains(t.Id))
                                   .AsEnumerable();
            }
            return tracks;
        }

        /// <summary>
        ///Vérifie si une musique existe.
        /// </summary>
        public async Task<bool> TrackExists(int id)
        {
            var track = await _context.Track.FindAsync(id);
            return track != null;
        }

        private async Task<List<int>> GetUserListenedTracksIds(string userId)
        {
            var ids = await _context.UserListennedTrack
                                .Where(item => item.UsersId == userId)
                                .OrderByDescending(item => item.ListennedAt)
                                .Select(item => item.TrackId)
                                .ToListAsync();
            return ids is not null && ids.Any() ? ids : new List<int>();
        }
        public TrackDTO? GetTrackDTO(int trackId)
        {
            var trackDTO = (from track in _context.Track
                            where track.Id == trackId // Assurez-vous que trackId est correctement défini
                            join taPrincipal in _context.TrackArtist on track.Id equals taPrincipal.TrackId
                            where taPrincipal.ArtistRole == Enums.ArtistRole.Principal
                            join aPrincipal in _context.Artist on taPrincipal.ArtistId equals aPrincipal.Id
                            join taFeaturing in _context.TrackArtist on track.Id equals taFeaturing.TrackId
                            where taFeaturing.ArtistRole == Enums.ArtistRole.Featuring
                            join aFeaturing in _context.Artist on taFeaturing.ArtistId equals aFeaturing.Id
                            join ult in _context.UserListennedTrack on track.Id equals ult.TrackId into userListenings
                            select new TrackDTO
                            {
                                Id = track.Id, // Utilisez track.Id pour obtenir l'ID de la piste
                                Title = track.Title,
                                Duration = new TimeSpan(0, track.Minutes, track.Seconds),
                                Source = track.Source,
                                ArtistName = aPrincipal.Name, // Nom de l'artiste principal
                                FeaturingArtists = new List<string> { aFeaturing.Name }, // Liste des artistes en featuring
                                ArtistPicture = aPrincipal.Picture,
                                // Vous pouvez inclure les informations d'écoute de l'utilisateur si nécessaire
                                ListeningCount = userListenings.Count()
                            })
                .FirstOrDefault(); // Vous pouvez utiliser FirstOrDefault() si vous ne vous attendez qu'à un seul résultat

            return trackDTO;
        }
    }
}
