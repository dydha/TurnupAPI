using Microsoft.EntityFrameworkCore;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Repositories
{
    /// <summary>
    /// Repository pour la gestion des likes.
    /// </summary>
    public class LikeRepository : ILikeRepository
    {
        private readonly TurnupContext _context;

        /// <summary>
        /// Constructeur de la classe.
        /// </summary>
        /// <param name="context">Le contexte de la base de données.</param>
        public LikeRepository(TurnupContext context)
        {
            _context = context;
        }

        //---------------------- LIKE TRACK--------------------------------
        /// <summary>
        /// Ajoute un une Track en favoris dans la base de données.
        /// </summary>
        /// <param name="uft">L'artiste à ajouter.</param>
        public async Task AddTrackLikeAsync(UserFavoriteTrack uft)
        {                  
                _context.UserFavoriteTrack.Add(uft);
                await _context.SaveChangesAsync();         
        }

        /// <summary>
        /// Supprime  une Track des favoris dans la base de données.
        /// </summary>
        /// <param name="uft">L'artiste à ajouter.</param>
        public async Task<bool> RemoveTrackLikeAsync(UserFavoriteTrack uft)
        {

            var result = false;
            var existingUft = await _context.UserFavoriteTrack.FirstOrDefaultAsync(u =>  u.Id == uft.Id);
            if (existingUft is not null) { }
            {
                _context.UserFavoriteTrack.Remove(uft);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
            
        }
        /// <summary>
        /// Retourne la liste des musique en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des musique en favoris d'un utilisateur</returns>
        public async  Task<IEnumerable<Track>> GetUserFavoriteTracks(string userId, int offset, int limit)
        {
            var tracks = Enumerable.Empty<Track>();
            var userFavoriteTracksIds = await GetUserFavoriteTracksIdsList(userId);
            if(userFavoriteTracksIds.Any()) 
            {
                 tracks = await _context.Track
                                .Where(t => userFavoriteTracksIds.Contains(t.Id))
                                .Skip(offset)
                                .Take(limit)
                                .AsNoTracking()
                                .ToListAsync();
            }
            return tracks ;
         
        }
        /// <summary>
        /// Retourne la liste  des ids des musique en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des ids des musique en favoris d'un utilisateur</returns>
        public async Task<IEnumerable<int>> GetUserFavoriteTracksIdsList(string userId)
        {
            var tracks = await _context.Track.ToListAsync();
            var favoriteTracksId = await _context.UserFavoriteTrack
                                            .Where(uft => uft.UsersId == userId)
                                            .Select(uft => uft.TrackId)
                                            .ToListAsync();
            return favoriteTracksId is not null &&  favoriteTracksId.Any() ? favoriteTracksId : Enumerable.Empty<int>();
        }
        public async Task<bool> IsLoggedUserLikeThisTrack(string userId, int trackId)
        {
            var loggedUserFavoriteTracksIds = (await GetUserFavoriteTracksIdsList(userId)).Contains(trackId);
            return loggedUserFavoriteTracksIds;
        }
        /// <summary>
        /// Retourne un UserFavoriteTrack.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <param name="trackId">L'id de la musique.</param>
        /// <returns>Retourne un UserFavoriteTrack.</returns>
        public async Task<UserFavoriteTrack?> GetExistingTrackLike(string userId, int trackId)
        {
            var existingLike = await _context.UserFavoriteTrack
                                                .Where(uft => uft.UsersId == userId && uft.TrackId == trackId)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync();
            return existingLike;
        }
        //----------------------END LIKE TRACK--------------------------------
        //----------------------LIKE ARTIST--------------------------------
        /// <summary>
        /// Ajoute un un artiste en favoris dans la base de données.
        /// </summary>
        /// <param name="ufa">L'artiste à ajouter.</param>
        public async Task AddArtistLikeAsync(UserFavoriteArtist ufa)
        {         
                _context.UserFavoriteArtist.Remove(ufa);
                await _context.SaveChangesAsync();
            
        }

        /// <summary>
        /// Supprime  un artiste des favoris dans la base de données.
        /// </summary>
        /// <param name="ufa">L'artiste à ajouter.</param>
        public async Task<bool> RemoveArtistLikeAsync(UserFavoriteArtist ufa)
        {
            bool result = false;
            var existingUfa = await _context.UserFavoriteArtist.FirstOrDefaultAsync(u => u.Id == ufa.Id);
            if(existingUfa is not  null) 
            {
                _context.UserFavoriteArtist.Remove(ufa);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
           
        }
        /// <summary>
        /// Retourne la liste des artistes favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des artistes en favoris d'un utilisateur</returns>
        public async Task<IEnumerable<Artist>> GetUserFavoriteArtists(string userId, int offset, int limit)
        {
            var artists = Enumerable.Empty<Artist>();
            var userFavoriteArtistsIds = await GetUserFavoriteArtistsIdsList(userId);
            if(userFavoriteArtistsIds.Any()) 
            {
                 artists = await _context.Artist
                            .Where(a => userFavoriteArtistsIds.Contains(a.Id))
                            .Skip(offset)
                            .Take(limit)
                            .AsNoTracking()
                            .ToListAsync();
            }
            return artists;
            
        }
        /// <summary>
        /// Retourne la liste  des ids des artistes  favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des ids des artistes favoris d'un utilisateur</returns>
        public async Task<IEnumerable<int>> GetUserFavoriteArtistsIdsList(string userId)
        {
            var artists = await _context.Artist.ToListAsync();
            var favoriteArtistsIds = await _context.UserFavoriteArtist
                                            .Where(ufa => ufa.UsersId == userId)
                                            .Select(ufa => ufa.ArtistId)
                                            .ToListAsync();
            return favoriteArtistsIds is not null &&  favoriteArtistsIds.Any() ? favoriteArtistsIds : Enumerable.Empty<int>();
        }
        /// <summary>
        /// Retourne un UserFavoritArtist.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <param name="artistId">L'id de l'artist.</param>
        /// <returns>Retourne un UserFavoriteArtist.</returns>
        public async Task<UserFavoriteArtist?> GetExistingArtistLike(string userId, int artistId)
        {
            var existingLike = await _context.UserFavoriteArtist
                                                .Where(ufa => ufa.UsersId == userId && ufa.ArtistId == artistId)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync();
            return existingLike;
        }
        //----------------------END LIKE ARTIST--------------------------------
        //----------------------LIKE PLAYLIST--------------------------------
        /// <summary>
        /// Ajoute un une playlist en favoris dans la base de données.
        /// </summary>
        /// <param name="ufp">L'artiste à ajouter.</param>
        public async Task AddPlaylistLikeAsync(UserFavoritePlaylist ufp)
        {       
            _context.UserFavoritePlaylist.Remove(ufp);
            await _context.SaveChangesAsync();        
        }
        /// <summary>
        /// Supprime  une playlist des favoris dans la base de données.
        /// </summary>
        /// <param name="ufp">L'artiste à ajouter.</param>
        public async Task<bool> RemovePlaylistLikeAsync(UserFavoritePlaylist ufp)
        {
            var result = false;
            var existingUfp = await _context.UserFavoritePlaylist.FirstOrDefaultAsync(u => u.Id == ufp.Id);
            if(existingUfp is not null) 
            {
                _context.UserFavoritePlaylist.Remove(ufp);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Retourne la liste des playlists en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des playlists en favoris d'un utilisateur</returns>
        public async Task<IEnumerable<Playlist>> GetUserFavoritePlaylists(string userId, int offset, int limit)
        {
            var playlists = Enumerable.Empty<Playlist>();
           var userFavoritePlaylistsIds = await GetUserFavoritePlaylistsIdsList(userId);
            if(userFavoritePlaylistsIds.Any())
            {
                playlists = await _context.Playlist
                                .Where(p => userFavoritePlaylistsIds.Contains(p.Id))
                                .Skip(offset)
                                .Take(limit)
                                .AsNoTracking()
                                .ToListAsync();
            }
            return playlists;
        }
        /// <summary>
        /// Retourne la liste  des ids des playlists en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des ids des playlistsen favoris d'un utilisateur</returns>
        public async Task<IEnumerable<int>> GetUserFavoritePlaylistsIdsList(string userId)
        {
            var tracks = await _context.Track.ToListAsync();
            var favoritePlaylistsIds = await _context.UserFavoriteTrack
                                                .Where(uft => uft.UsersId == userId)
                                                .Select(uft => uft.TrackId)
                                                .ToListAsync();
            return favoritePlaylistsIds is not null && favoritePlaylistsIds.Any() ? favoritePlaylistsIds : Enumerable.Empty<int>();
        }
        /// <summary>
        /// Retourne un UserFavoritePlaylist.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <param name="playlistId">L'id de la musique.</param>
        /// <returns>Retourne un UserFavoritePlaylist.</returns>
        public async Task<UserFavoritePlaylist?> GetExistingPlaylistLike(string userId, int playlistId)
        {
            var existingLike = await _context.UserFavoritePlaylist
                                                .FirstOrDefaultAsync(ufp => ufp.UsersId == userId && ufp.PlaylistId == playlistId);
                                                
            return existingLike;
        }
        //----------------------END LIKE PLAYLIST--------------------------------
    }
}
