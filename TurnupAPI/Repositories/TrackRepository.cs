using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.ComponentModel;
using TurnupAPI.Data;
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
          
            try
            {
                _context.Track.Add(track);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(ex.Message);
            }
        }
        /// <summary>
        /// Récupère une Track par son ID.
        /// </summary>
        /// <param name="id">L'ID de la track  à récupérer.</param>
        /// <returns>La Track trouvée ou null si elle n'existe pas.</returns>
        /// <exception cref="NotFoundException">Si l'artiste n'est pas trouvé.</exception>
        public async Task<Track> GetAsync(int id)
        {
            var track = await _context.Track
                                .Where(t => t.Id == id)
                               .Include(t => t.UserFavoriteTracks)
                               .FirstOrDefaultAsync();
            return track ?? throw new NotFoundException();
        }

        /// <summary>
        /// Supprime une Track de la base de données par son ID.
        /// </summary>
        /// <param name="id">L'ID de la Track à supprimer.</param>
        public async Task DeleteAsync(int id)
        {
           
            try
            {
                var track = await GetAsync(id);
                _context.Track.Remove(track);
                await _context.SaveChangesAsync();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(ex.Message);
            }

        }
        /// <summary>
        /// Met à jour une Track dans la base de données.
        /// </summary>
        /// <param name="track">La track à mettre à jour.</param>
        public async Task UpdateAsync(Track track)
        {
           
            try
            {
               
                var existingTrack = await GetAsync(track.Id);
                if (existingTrack != null)
                {
                    _context.Track.Update(existingTrack);
                    await _context.SaveChangesAsync();
                }
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(ex.Message);
            }
        }

        /// <summary>
        /// Récupère la liste de toutes les tracks.
        /// </summary>
        /// <returns>La liste de toutes les tracks ou une liste vide si aucune playlist n'est trouvée.</returns>
        public async Task<List<Track>> GetAllAsync()
        {
         
            var tracks = await _context.Track
                                    .Include(t => t.UserListennedTracks)
                                    .ToListAsync();
            return (tracks != null && tracks.Count> 0) ? tracks : throw new EmptyListException();
        }
        /// <summary>
        /// Récupère la liste de toutes les tracks d'une playlist.
        /// </summary>
        /// <returns>La liste de toutes les tracks ou une liste vide si aucune playlist n'est trouvée.</returns>
        public async Task<List<Track>> GetTracksByPlaylistAsync(int playlistId)
        {

            var tracks = await (from track in _context.Track
                          join pt in _context.PlaylistTrack on track.Id equals pt.TrackId
                          where pt.PlaylistId == playlistId
                          select track).ToListAsync();
            return (tracks != null && tracks.Count > 0) ? tracks : throw new EmptyListException();
        }

        /// <summary>
        /// Récupère l'historique d'écoute de l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne l'historique d'écoute de l'utilisateur connecté..</returns>
        public async Task<List<Track>> GetUserListeningHistory(string userId)
        {
            var ids = await _context.UserListennedTrack.Where(item => item.UsersId == userId)
                                                                .OrderByDescending(item => item.ListennedAt)
                                                                .Select(item => item.TrackId)
                                                                .ToListAsync();
            var historicTracks = (await GetAllAsync()).Where(t => ids.Contains(t.Id))
                                                      .OrderBy(t => ids.IndexOf(t.Id))
                                                      .ToList(); // Je filtre les tracks en récupérant que celle écoutées par l'utilisateur connecté
            return (historicTracks != null && historicTracks.Count > 0) ? historicTracks : throw new EmptyListException();
        }
        /// <summary>
        /// Récupère les musique non écoutées par  l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne une suggestion de musique à l'utilisateur connecté</returns>

        public async Task<List<Track>> GetDiscoveryAsync(string userId)
        {
            var tracks = await GetAllAsync();
            var ids = await _context.UserListennedTrack.Where(item => item.UsersId == userId).Select(item => item.TrackId).ToListAsync();
            var tracksFiltered = tracks.Where(t => !ids.Contains(t.Id)).ToList();
            return tracksFiltered!= null && tracksFiltered.Any() ? tracksFiltered : throw new EmptyListException();
        }

        // <summary>
        /// Récupère les musiques populaires : les plus écoutes.
        /// </summary>
        /// <returns>Retourne les musiques populaires : les plus écoutes</returns>
        public async Task<List<Track>> GetPopularTracksAsync()
        {
            var ulttrackIds = await _context.UserListennedTrack.Select(ult => ult.TrackId).ToListAsync(); //Je récupère toutes les userListennedTrack.
            Dictionary<int, int> trackIdAndListennedCount = new();
            foreach (var trackId in ulttrackIds)
            {
                    if(trackIdAndListennedCount.ContainsKey(trackId))
                    {
                        trackIdAndListennedCount[trackId]++;
                    }
                    else
                    {
                      trackIdAndListennedCount.Add(trackId, 1);
                    }
            }
            // Triez le dictionnaire par order décroissant du nombre d'écoute et récupérer les kvp.Key = id des tracks.
            var trackIds = trackIdAndListennedCount.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).Take(100).ToList();
            var popularTracks = (await GetAllAsync()).Where(t => trackIds.Contains(t.Id)).OrderBy(t=> trackIds.IndexOf(t.Id)).ToList();    
            return (popularTracks != null &&  popularTracks.Any()) ? popularTracks : throw new EmptyListException();
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
        public async Task DeleteTrackFromPlaylistAsync(AddTrackToPlaylistForm input, string userId)
        {
            var playlist = (await _context.Playlist.ToListAsync())
                                                     .Where(p => p.Id == input.PlaylistId && p.UsersId == userId)
                                                     .FirstOrDefault();
            if (playlist != null)
            {
                var track = await GetAsync(input.TrackId);
                var existingTrackInPlaylist = await _context.PlaylistTrack.Where(pt => pt.PlaylistId == playlist.Id && pt.TrackId == track.Id).FirstOrDefaultAsync();
                if (existingTrackInPlaylist != null)
                {
                    _context.PlaylistTrack.Remove(existingTrackInPlaylist);
                    await _context.SaveChangesAsync();
                }
            }
        }


        /// <summary>
        /// Récupère et retourne les musique d'un Types.
        /// </summary>
        public async Task<List<Track>> GetTracksByTypesAsync(int typesId)
        {
            var ids = await _context.TrackType.Where(tt => tt.TypeId == typesId).Select(tt => tt.TrackId).ToListAsync();
            var tracks = (await GetAllAsync()).Where(t => ids.Contains(t.Id)).ToList();

            return (tracks != null && tracks.Count > 0) ? tracks : throw new EmptyListException();
        }
    }
}
