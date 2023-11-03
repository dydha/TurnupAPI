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
                    if(!string.IsNullOrEmpty(loggedUserId)) 
                    {
                        var trackExists = await _trackRepository.GetAsync(trackId);
                        if (trackExists is not null)
                        {
                            var isLiked = await _likeRepository.IsLoggedUserLikeThisTrack(loggedUserId, trackId);
                            return Ok(isLiked);
                        }
                    }                    
                    return NoContent();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            else
            {
                return NoContent();
            }
        }
        /// <summary>
        /// Récupère les id des tracks favoris d'un utilisateur.
        /// </summary>
        /// <returns>Retourne  les ids des tracks favoris d'un utilisateur.</returns>
        [HttpGet("get-logged-user-favorite-tracks-ids")]
        public async Task<ActionResult<List<int>>> GetLoggedUserFavoriteTracksIds()
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les ids des artistes favoris d'un utilisateur.");
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    var ids = await _likeRepository.GetUserFavoriteTracksIdsList(loggedUserId);
                    return Ok(ids);
                }
                else
                {
                    _logger.LogWarning("Aucun id n'a été trouvé.");
                    return NoContent(); //StatusCode 204
                }

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
                    if (!string.IsNullOrEmpty(loggedUserId))
                    {
                        var existingLike = await _likeRepository.GetExistingTrackLike(loggedUserId, trackId);
                        if (existingLike is not null)
                        {
                            await _likeRepository.RemoveTrackLikeAsync(existingLike);  
                        }
                        else
                        {
                            var like = new UserFavoriteTrack() 
                            {
                                UsersId = loggedUserId,
                                TrackId = trackId,
                            };
                            await _likeRepository.AddTrackLikeAsync(like);
                        }
                        // Suppression du cahce
                        var cacheKey = CacheKeyForUserFavoriteTracks(loggedUserId);
                        if (_memoryCache.TryGetValue(cacheKey, out _))
                        {
                            // La clé existe dans le cache, alors nous pouvons la supprimer
                            _memoryCache.Remove(cacheKey);
                        }
                       return NoContent();
                    }
                    throw new Exception("L'utilisateur n'a pas été trouvé.");
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
        [HttpGet("get-logged-user-favorite-playlists-ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetLoggedUserFavoritePlaylistsIds()
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les ids des olaylists favoris d'un utilisateur.");
                var ids = Enumerable.Empty<int>();
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(!string.IsNullOrEmpty(loggedUserId) ) 
                {
                     ids = await _likeRepository.GetUserFavoritePlaylistsIdsList(loggedUserId);    
                }
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
                if(!string.IsNullOrEmpty (loggedUserId) ) 
                {
                    var existingLike = await _likeRepository.GetExistingPlaylistLike(loggedUserId, playlistId);
                    if(existingLike is not  null) 
                    {
                        await _likeRepository.RemovePlaylistLikeAsync(existingLike);
                    }
                    else
                    {
                        var like = new UserFavoritePlaylist()
                        {
                            UsersId = loggedUserId,
                            PlaylistId = playlistId,
                            LikedAt = DateTime.Now,
                        };
                        await _likeRepository.AddPlaylistLikeAsync(like);
                    }
                    return NoContent();
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");

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
        [HttpGet("get-logged-user-favorite-artists-ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetLoggedUserFavoriteArtistsIds()
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer les ids des artistes favoris d'un utilisateur.");
                var ids = Enumerable.Empty<int>();  
                var loggedUserId = await GetLoggedUserIdAsync(); 
                if(!string.IsNullOrEmpty(loggedUserId))
                {
                    ids = await _likeRepository.GetUserFavoriteArtistsIdsList(loggedUserId);
                }
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
                if(!string.IsNullOrEmpty(loggedUserId))
                {
                    var existingLike = await _likeRepository.GetExistingArtistLike(loggedUserId, artistId);
                    if(existingLike is not null) 
                    {
                        await _likeRepository.RemoveArtistLikeAsync(existingLike);
                    }
                    else
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
                    return NoContent();
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");

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
                    var fansNumber = 0;
                    if (artiste is not null)
                    {
                        var fans = artiste.UserFavoriteArtists;
                        fansNumber = fans is not null && fans.Any() ? fans.Count : fansNumber;

                    }
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
