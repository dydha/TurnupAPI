using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur des likes.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    public class LikeController : BaseController
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<LikeController> _logger;
        /// <summary>
        /// Constructeur du contrôleur de l'artiste.
        /// </summary>
        public LikeController
            (
                ILikeRepository likeRepository,
                IMemoryCache memoryCache,
                IUserRepository userRepository,
                TurnupContext context,
                IMapper mapper,
                ILogger<LikeController> logger,
                IArtistRepository artistRepository
            ) : base(userRepository,artistRepository,null,context,mapper)
        {
            _likeRepository = likeRepository;
            _memoryCache = memoryCache;
            _logger = logger;
           
        }

        //---------------------------TRACK--------------------------------------------------
        [HttpGet("logged-user-like-this-track/{trackId}")]
        public async Task<ActionResult<bool>> LoggedUserLikeThisTrack(int trackId)
        {
            if(trackId > 0)
            {
                try
                {                  
                    _logger.LogInformation("Requete qui vérife si l'utilisateur connecté aime cette musique.");
                    var loggedUserId = await GetLoggedUserIdAsync();
                    if(string.IsNullOrEmpty(loggedUserId))
                    {
                        return StatusCode(500);
                    }
                    var existingTrack = await _trackRepository.GetAsync(trackId);
                    if(existingTrack is null)
                    {
                        return NotFound();
                    }
                    
                    bool isLiked = await _likeRepository.IsLoggedUserLikeThisTrack(loggedUserId, trackId);
                    return Ok(isLiked);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }

            return NotFound();
            
        }
        /// <summary>
        /// Récupère les id des tracks favoris d'un utilisateur.
        /// </summary>
        /// <returns>Retourne  les ids des tracks favoris d'un utilisateur.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-logged-user-favorite-tracks-ids")]
        public async Task<ActionResult<List<int>>> GetLoggedUserFavoriteTracksIds()
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les ids des artistes favoris d'un utilisateur.");
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(string.IsNullOrEmpty(loggedUserId)) 
                {
                    _logger.LogWarning("Aucun id n'a été trouvé.");
                    return NoContent(); //StatusCode 204
                  
                }
                var ids = await _likeRepository.GetUserFavoriteTracksIdsList(loggedUserId);
                return Ok(ids);
                

            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Ajoute ou supprime un like a une musique.
        /// </summary>
        [HttpPost("like-track")] 
        public async Task<ActionResult> LikeTrack([FromBody] int trackId)
        {
            try
            {
                _logger.LogInformation("Ajout/Suppression d'une musique aux/des favoris.");
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
                var existingLike = await _likeRepository.GetExistingTrackLike(loggedUserId, trackId);
                if (existingLike is null)
                {
                    var like = new UserFavoriteTrack()
                    {
                        UsersId = loggedUserId,
                        TrackId = trackId,
                    };
                    await _likeRepository.AddTrackLikeAsync(like);                  
                }
                else
                {
                    await _likeRepository.RemoveTrackLikeAsync(existingLike);
                }
                   
                var cacheKey = CacheKeyForUserFavoriteTracks(loggedUserId);
                if (_memoryCache.TryGetValue(cacheKey, out _))
                {                      
                    _memoryCache.Remove(cacheKey);
                }
                return NoContent();
                   
            }  
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
           
        }
        //--------------------------------------------END TRACK-----------------------------------------
        //-------------------------------------------- PLAYLIST-----------------------------------------
        /// <summary>
        /// Récupère les id des playlist favoris d'un utilisateur.
        /// </summary>
        /// <returns>Retourne  les ids des playlistfavoris d'un utilisateur.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-logged-user-favorite-playlists-ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetLoggedUserFavoritePlaylistsIds()
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les ids des olaylists favoris d'un utilisateur.");
                var ids = Enumerable.Empty<int>();
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(string.IsNullOrEmpty(loggedUserId) ) 
                {
                    return StatusCode(500);                  
                }
                ids = await _likeRepository.GetUserFavoritePlaylistsIdsList(loggedUserId);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }

        }
        /// <summary>
        /// Ajoute ou supprime un like a une playlist.
        /// </summary>
        [HttpPost("like-playlist")]
        public async Task<ActionResult> LikePlaylist([FromBody] int playlistId)
        {    
            try
            {
                _logger.LogInformation("Requete pour ajouter/supprimer une playlist aux/des favoris.");
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(string.IsNullOrEmpty(loggedUserId) )
                {
                    return StatusCode(500);
                }
                var existingLike = await _likeRepository.GetExistingPlaylistLike(loggedUserId, playlistId);
                if(existingLike is null) 
                {
                    var like = new UserFavoritePlaylist()
                    {
                        UsersId = loggedUserId,
                        PlaylistId = playlistId,
                        LikedAt = DateTime.Now,
                    };
                    await _likeRepository.AddPlaylistLikeAsync(like);
                }
                else
                {
                    await _likeRepository.RemovePlaylistLikeAsync(existingLike);
                   
                }
                return NoContent();
               

            }               
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }

        }
        //--------------------------------------------END PLAYLIST-----------------------------------------
        //---------------------------ARTIST--------------------------------------------------
        /// <summary>
        /// Récupère les id des artistes favoris d'un utilisateur.
        /// </summary>
        /// <returns>Retourne  les ids des artistes favoris d'un utilisateur.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-logged-user-favorite-artists-ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetLoggedUserFavoriteArtistsIds()
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer les ids des artistes favoris d'un utilisateur.");  
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);                   
                }
                var ids = await _likeRepository.GetUserFavoriteArtistsIdsList(loggedUserId);
                return Ok(ids);              
            }         
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Ajoute un artiste en favoris.
        /// </summary>
        /// <returns>Ajoute un artiste en favoris.</returns>
        [HttpPost("like-artist")]
        public async Task<ActionResult> LikeArtist([FromBody]  int artistId)
        {
          
            try
            {
                _logger.LogInformation( "Requete pour ajouter/supprimer un artiste aux/des favoris.");
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
                var existingLike = await _likeRepository.GetExistingArtistLike(loggedUserId, artistId);
                if(existingLike is  null) 
                {
                    var like = new UserFavoriteArtist()
                    {
                        UsersId = loggedUserId,
                        ArtistId = artistId,
                        LikedAt = DateTime.Now,
                    };
                    _context.UserFavoriteArtist.Add(like);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    await _likeRepository.RemoveArtistLikeAsync(existingLike);                  
                }
                return NoContent();               
            }          
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère un artiste et retourne son nombre de fans.
        /// </summary>
        /// <returns>Retourne  le nombre de fans d'un artiste.</returns>
        [HttpGet("get-artist-fans-number/{artistId:int}")]
        public async Task<ActionResult<int>> GetArtistFansNumber(int artistId)
        {
            if(artistId > 0)
            {
                try
                {
                    var artiste = await _artistRepository.GetAsync(artistId);
                    if (artiste is  null)
                    {
                        return NotFound();
                    }
                    var fans = artiste.UserFavoriteArtists;
                    int  fansNumber = fans is null || fans.Count is 0 ? 0 : fans.Count ;
                    return Ok(fansNumber);
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }

            }
            return StatusCode(500);
        }

        //---------------------------END ARTIST--------------------------------------------------
    }
}
