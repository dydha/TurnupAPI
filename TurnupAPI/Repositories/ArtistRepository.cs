using Microsoft.EntityFrameworkCore;
using TurnupAPI.Data;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using TurnupAPI.Exceptions;
using TurnupAPI.Enums;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TurnupAPI.Repositories
{
    /// <summary>
    /// Repository pour la gestion des artistes.
    /// </summary>
    public class ArtistRepository : IArtistRepository
    {
        private readonly TurnupContext _context;

        /// <summary>
        /// Constructeur de la classe.
        /// </summary>
        /// <param name="context">Le contexte de la base de données.</param>
        public ArtistRepository(TurnupContext context)
        {
            _context = context; // Injection du contexte de base de données par le constructeur
        }

        /// <summary>
        /// Ajoute un artiste dans la base de données.
        /// </summary>
        /// <param name="artist">L'artiste à ajouter.</param>
        public async Task AddAsync(Artist artist)
        {        
                _context.Artist.Add(artist);
                await _context.SaveChangesAsync();            
        }

        /// <summary>
        /// Supprime un artiste de la base de données par son ID.
        /// </summary>
        /// <param name="id">L'ID de l'artiste à supprimer.</param>
        public async Task<bool> DeleteAsync(int id)
        {
            bool result = false;
            var artist = await GetAsync(id);
            if(artist is not null)
            {
                _context.Artist.Remove(artist);
                await _context.SaveChangesAsync();
                result =  true;
            }
            return result;
                      
        }


        /// <summary>
        /// Récupère un artiste par son ID.
        /// </summary>
        /// <param name="id">L'ID de l'artiste à récupérer.</param>
        /// <returns>L'artiste trouvé ou null s'il n'existe pas.</returns>
       
        public async Task<Artist?> GetAsync(int id)
        {
            var artist = await _context.Artist.Where(a => a.Id == id)
                                .Include(a => a.UserFavoriteArtists)
                                .AsSplitQuery()
                                .FirstOrDefaultAsync();
            return artist;
        }

        /// <summary>
        /// Récupère un artiste à base de filtre.
        /// </summary>
        /// <returns>L'artiste trouvé ou null s'il n'existe pas.</returns>

        public async Task<Artist?> GetFilteredArtistAsync(Expression<Func<Artist, bool>> filter )
        {
            var artist = await _context.Artist.FirstOrDefaultAsync(filter);
            return artist;
        }
        /// <summary>
        /// Récupère la liste de tous les artistes.
        /// </summary>
        /// <returns>La liste de tous les artistes ou une liste vide si aucun artiste n'est trouvé.</returns>
        public async Task<IEnumerable<Artist>> GetAllAsync()
        {
           
                var artists = await _context.Artist
                                                .Include(a => a.UserFavoriteArtists)
                                                .Include(a => a.TrackArtists)
                                                .AsSplitQuery()
                                                .ToListAsync();
                return artists != null && artists.Count > 0 ? artists : Enumerable.Empty<Artist>();
           
        }

        /// <summary>
        /// Met à jour un artiste dans la base de données.
        /// </summary>
        /// <param name="artist">L'artiste à mettre à jour.</param>
        public async Task<bool> UpdateAsync(Artist artist)
        {
            bool result = false;
            var existingArtist = await GetAsync(artist.Id);
            if(existingArtist is not null)
            {
                _context.Artist.Update(existingArtist);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
                
        }
        /// <summary>
        /// Retourne le nom de l'artiste principal d'une musique
        /// </summary>
        /// <returns>Le nom de l'artiste principal.</returns>
        public string GetPrincipalArtistNameByTrackId(int trackId)
        {
            var artistName =  ( from ta in _context.TrackArtist
                               join artist in _context.Artist on ta.ArtistId equals artist.Id
                               where ta.TrackId == trackId && ta.ArtistRole == ArtistRole.Principal
                               select artist.Name
                             )
                             .AsNoTracking()
                             .FirstOrDefault();

            return artistName ?? string.Empty;
        }

        /// <summary>
        /// Retourne la photo de l'artiste principal d'une musique
        /// </summary>
        /// <returns>La photo de l'artiste principal.</returns>
        public string GetPrincipalArtistPictureByTrackId(int trackId)
        {
            var artistPicture= (from ta in _context.TrackArtist
                                    join artist in _context.Artist on ta.ArtistId equals artist.Id
                                    where ta.TrackId == trackId && ta.ArtistRole == ArtistRole.Principal
                                    select artist.Picture
                             )
                             .AsNoTracking()
                             .FirstOrDefault();

            return artistPicture ?? string.Empty;
        }

        /// <summary>
        /// Retourne les noms des artistes en featuring d'une musique
        /// </summary>
        /// <returns>Les noms des artistes en featuring.</returns>
        public IEnumerable<string> GetFeaturingArtistsNamesByTrackId(int trackId)
        {
            var artistNames =  (from ta in _context.TrackArtist
                                    join artist in _context.Artist on ta.ArtistId equals artist.Id
                                    where ta.TrackId == trackId && ta.ArtistRole == ArtistRole.Featuring
                                    select artist.Name
                             )
                             .AsNoTracking()
                             .ToList();

            return artistNames is not null && artistNames.Any() ? artistNames : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Retourne le nombre de fans d'un artiste.
        /// </summary>
        /// <returns>Retourne le nombre de fans d'un artiste.</returns>
        public int GetArtistFans(int artistId)
        {
            var artistFans = _context.UserFavoriteArtist.Where(ufa => ufa.ArtistId == artistId).ToList();       
            return artistFans.Count;
        }
    }
}
