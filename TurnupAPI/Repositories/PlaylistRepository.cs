using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurnupAPI.Data;
using TurnupAPI.Exceptions;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Repositories
{
    /// <summary>
    /// Repository pour la gestion des playlists.
    /// </summary>
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly TurnupContext _context;
        /// <summary>
        /// Constructeur de la classe.
        /// </summary>
        /// <param name="context">Le contexte de la base de données.</param>
        public PlaylistRepository(TurnupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ajoute une playlistdans la base de données.
        /// </summary>
        /// <param name="playlist">La playlist à ajouter.</param>
        public async Task AddAsync(Playlist playlist)
        {          
                _context.Playlist.Add(playlist);
                await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Supprime une playlist de la base de données par son ID.
        /// </summary>
        /// <param name="id">L'ID de l'artiste à supprimer.</param>
        public async Task<bool> DeleteAsync(int id)
        {
            bool result = false;
            var playlist = await GetAsync(id);
            if(playlist is not null)
            {
                _context.Playlist.Remove(playlist);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Récupère une playlist par son ID.
        /// </summary>
        /// <param name="id">L'ID de la playlist à récupérer.</param>
        /// <returns>La playlist trouvé ou null si elle n'existe pas.</returns>
        /// <exception cref="NotFoundException">Si l'artiste n'est pas trouvé.</exception>
        public async Task<Playlist?> GetAsync(int id)
        {
            var playlist = await _context.Playlist
                                .Include(p => p.Users)
                                .AsSplitQuery()
                                .FirstOrDefaultAsync(p => p.Id == id);
            return playlist ;
        }
        /// <summary>
        /// Récupère une playlis à base de filtre.
        /// </summary>
        /// <returns>L'artiste trouvé ou null s'il n'existe pas.</returns>
        /// <exception cref="NotFoundException">Si la playlist n'est pas trouvée.</exception>
        public async Task<Playlist?> GetFilteredPlaylistAsync(Expression<Func<Playlist, bool>> filter)
        {
            var playlist = await _context.Playlist
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(filter); 
            return playlist; 
        }
        /// <summary>
        /// Récupère la liste de toutes les playlists.
        /// </summary>
        /// <returns>La liste de toutes les playlists ou une liste vide si aucune playlist n'est trouvée.</returns>
        public async Task<IEnumerable<Playlist>> GetAllAsync()
        {
            
            var playlists = await _context.Playlist
                                                .AsNoTracking()
                                                .ToListAsync();
            return (playlists is not null && playlists.Any()) ? playlists : Enumerable.Empty<Playlist>();
        }

        /// <summary>
        /// Met à jour une playlist dans la base de données.
        /// </summary>
        /// <param name="playlist">La plalist à mettre à jour.</param>
        public async Task<bool> UpdateAsync(Playlist playlist)
        {

            bool result = false;
            var existingPlaylist = await GetAsync(playlist.Id);
            if (existingPlaylist is not  null)
            {
                _context.Playlist.Update(playlist);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Récupère la liste des playlists d'un utilisateur. 
        /// </summary>
        /// <returns>La liste de toutes les playlists ou une liste vide si aucune playlist n'est trouvée.</returns>
        public async Task<IEnumerable<Playlist>> GetPlaylistByUserIdAsync(string userId)
        {
            var playlists = await _context.Playlist
                                        .Where(p => p.UsersId == userId)
                                        .AsNoTracking()
                                        .ToListAsync();
            return (playlists is not null && playlists.Any()) ? playlists : Enumerable.Empty<Playlist>();
        }
    }
}
