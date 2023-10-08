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
          
            try
            {
                _context.UserFavoriteTrack.Add(uft);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(ex.Message);
            }
        }

        /// <summary>
        /// Supprime  une Track des favoris dans la base de données.
        /// </summary>
        /// <param name="uft">L'artiste à ajouter.</param>
        public async Task RemoveTrackLikeAsync(UserFavoriteTrack uft)
        {
           
            try
            {
                _context.UserFavoriteTrack.Remove(uft);
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
        /// Retourne la liste des musique en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des musique en favoris d'un utilisateur</returns>
        public async  Task<List<Track>> GetUserFavoriteTracks(string userId)
        {
            var tracks = await _context.Track.ToListAsync();
            var favoriteTracks = (from track in tracks
                                  join ft in _context.UserFavoriteTrack on track.Id equals ft.TrackId
                                  where ft.UsersId == userId
                                  select track).ToList();
            return (favoriteTracks != null &&  favoriteTracks.Any() )? favoriteTracks : throw new EmptyListException();
        }
        /// <summary>
        /// Retourne la liste  des ids des musique en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des ids des musique en favoris d'un utilisateur</returns>
        public async Task<List<int>> GetUserFavoriteTracksIdsList(string userId)
        {
            var tracks = await _context.Track.ToListAsync();
            var favoriteTracksId = await _context.UserFavoriteTrack.Where(uft => uft.UsersId == userId).Select(uft => uft.TrackId).ToListAsync();
            return favoriteTracksId.Any() ? favoriteTracksId : throw new EmptyListException();
        }
        /// <summary>
        /// Retourne un UserFavoriteTrack.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <param name="trackId">L'id de la musique.</param>
        /// <returns>Retourne un UserFavoriteTrack.</returns>
        public async Task<UserFavoriteTrack> GetExistingTrackLike(string userId, int trackId)
        {
            var existingLike = await _context.UserFavoriteTrack.Where(uft => uft.UsersId == userId && uft.TrackId == trackId).FirstOrDefaultAsync();
            return existingLike ?? throw new NotFoundException();
        }
        //----------------------END LIKE TRACK--------------------------------
        //----------------------LIKE ARTIST--------------------------------
        /// <summary>
        /// Ajoute un un artiste en favoris dans la base de données.
        /// </summary>
        /// <param name="ufa">L'artiste à ajouter.</param>
        public async Task AddArtistLikeAsync(UserFavoriteArtist ufa)
        {
           
            try
            {
                _context.UserFavoriteArtist.Remove(ufa);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(ex.Message);
            }
        }

        /// <summary>
        /// Supprime  un artiste des favoris dans la base de données.
        /// </summary>
        /// <param name="ufa">L'artiste à ajouter.</param>
        public async Task RemoveArtistLikeAsync(UserFavoriteArtist ufa)
        {
           
            try
            {
                _context.UserFavoriteArtist.Remove(ufa);
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
        /// Retourne la liste des artistes favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des artistes en favoris d'un utilisateur</returns>
        public async Task<List<Artist>> GetUserFavoriteArtists(string userId)
        {
            var artists = await _context.Artist.ToListAsync();
            var favoriteArtists = (from artist in artists
                                     join ufa in _context.UserFavoriteArtist on artist.Id equals ufa.ArtistId
                                     where ufa.UsersId == userId
                                     select artist).ToList();
            return favoriteArtists.Any() ? favoriteArtists : throw new EmptyListException();
        }
        /// <summary>
        /// Retourne la liste  des ids des artistes  favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des ids des artistes favoris d'un utilisateur</returns>
        public async Task<List<int>> GetUserFavoriteArtistsIdsList(string userId)
        {
            var artists = await _context.Artist.ToListAsync();
            var favoriteArtistsId = await _context.UserFavoriteArtist.Where(ufa => ufa.UsersId == userId).Select(ufa => ufa.ArtistId).ToListAsync();
            return favoriteArtistsId.Any() ? favoriteArtistsId : throw new EmptyListException();
        }
        /// <summary>
        /// Retourne un UserFavoritArtist.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <param name="artistId">L'id de l'artist.</param>
        /// <returns>Retourne un UserFavoriteArtist.</returns>
        public async Task<UserFavoriteArtist> GetExistingArtistLike(string userId, int artistId)
        {
            var existingLike = await _context.UserFavoriteArtist.Where(ufa => ufa.UsersId == userId && ufa.ArtistId == artistId).FirstOrDefaultAsync();
            return existingLike ?? throw new NotFoundException();
        }
        //----------------------END LIKE ARTIST--------------------------------
        //----------------------LIKE PLAYLIST--------------------------------
        /// <summary>
        /// Ajoute un une playlist en favoris dans la base de données.
        /// </summary>
        /// <param name="ufp">L'artiste à ajouter.</param>
        public async Task AddPlaylistLikeAsync(UserFavoritePlaylist ufp)
        {
           
            try
            {
                _context.UserFavoritePlaylist.Remove(ufp);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(ex.Message);
            }
        }
        /// <summary>
        /// Supprime  une playlist des favoris dans la base de données.
        /// </summary>
        /// <param name="ufp">L'artiste à ajouter.</param>
        public async Task RemovePlaylistLikeAsync(UserFavoritePlaylist ufp)
        {
           
            try
            {
                _context.UserFavoritePlaylist.Remove(ufp);
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
        /// Retourne la liste des playlists en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des playlists en favoris d'un utilisateur</returns>
        public async Task<List<Playlist>> GetUserFavoritePlaylists(string userId)
        {
            var playlists = await _context.Playlist.ToListAsync();
            var favoritePlaylists = (from playlist in playlists
                                  join ufp in _context.UserFavoritePlaylist on playlist.Id equals ufp.PlaylistId
                                  where ufp.UsersId == userId
                                  select playlist).ToList();
            return favoritePlaylists.Any() ? favoritePlaylists : throw new EmptyListException();
        }
        /// <summary>
        /// Retourne la liste  des ids des playlists en favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <returns>La liste des ids des playlistsen favoris d'un utilisateur</returns>
        public async Task<List<int>> GetUserFavoritePlaylistsIdsList(string userId)
        {
            var tracks = await _context.Track.ToListAsync();
            var favoriteTracksId = await _context.UserFavoriteTrack.Where(uft => uft.UsersId == userId).Select(uft => uft.TrackId).ToListAsync();
            return favoriteTracksId.Any() ? favoriteTracksId : throw new EmptyListException();
        }
        /// <summary>
        /// Retourne un UserFavoritePlaylist.
        /// </summary>
        /// <param name="userId">L'id de l'utilisateur.</param>
        /// <param name="playlistId">L'id de la musique.</param>
        /// <returns>Retourne un UserFavoritePlaylist.</returns>
        public async Task<UserFavoritePlaylist> GetExistingPlaylistLike(string userId, int playlistId)
        {
            var existingLike = await _context.UserFavoritePlaylist.Where(ufp => ufp.UsersId == userId && ufp.PlaylistId == playlistId).FirstOrDefaultAsync();
            return existingLike ?? throw new NotFoundException();
        }
        //----------------------END LIKE PLAYLIST--------------------------------
    }
}
