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
                 ILogger<LikeController> logger
            ) : base(userRepository,null,null,context,mapper)
        {
            _likeRepository = likeRepository;
            _memoryCache = memoryCache;
            _logger = logger;
           
        }

        //---------------------------TRACK--------------------------------------------------
        [HttpGet("logged-user-like-this-track/{trackId}")]
        public async Task<ActionResult<bool>> LoggedUserLikeThisTrack(int trackId)
        {
            try
            {
                Console.WriteLine($"L'id du track arrivé jusqu'à l'API : {trackId}");
                _logger.LogInformation("Requete qui vérife si l'utilisateur connecté aime cette musique.");
                var loggedUserId = await GetLoggedUserIdAsync();
                try
                {
                    var trackExists = await _trackRepository.GetAsync(trackId);
                    if (trackExists != null)
                    {
                        var isLike = await _likeRepository.IsLoggedUserLikeThisTrack(loggedUserId, trackId);
                        return Ok(isLike);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    return NoContent();
                }
                
                return NoContent();
              
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
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
                var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
                try
                {
                    //Je récupère toutes les relations UserFavoriteTrack et je vérifie si l'utilisateur aime déja la musique
                    var ids = await _likeRepository.GetUserFavoriteTracksIdsList(loggedUserId);
                    return Ok(ids);
                }
                catch (EmptyListException)
                {
                    _logger.LogWarning( "Aucun id n'a été trouvé.");
                    return NoContent(); //StatusCode 204
                }
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Ajoute ou supprime un like a une musique.
        /// </summary>
        [HttpPost("like-track")] 
        public async Task<ActionResult> LikeTrack([FromBody] int trackId)
        {
            Console.WriteLine(trackId);
           if(await _trackRepository.TrackExists(trackId))
            {
                try
                {
                    _logger.LogInformation("Ajout/Suppression d'une musique aux/des favoris.");
                    var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
                                                                     //Je récupère toutes les relations UserFavoriteTrack et je vérifie si l'utilisateur aime déja la musique
                    try
                    {
                        var existingLike = await _likeRepository.GetExistingTrackLike(loggedUserId, trackId);
                        await _likeRepository.RemoveTrackLikeAsync(existingLike); // Si le loggedUser aime déjà la musique, on supprime le like     

                        return NoContent(); //Indique le dislike
                    }
                    catch (NotFoundException) // Si l'utilisateur connecté n'a pas cette musique en favoris.
                    {
                        try
                        {
                            var like = new UserFavoriteTrack() // Une UserFavoriteTrack est créee.
                            {
                                UsersId = loggedUserId,
                                TrackId = trackId,
                            };
                            await _likeRepository.AddTrackLikeAsync(like);
                            return Ok(); //Indique le like
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Une erreur est survenue.");
                            return StatusCode(500, $"Internal Server Error: {ex.Message}");
                        }
                    }
                    finally
                    {
                        // Utilisez userId comme clé de mise en cache
                        var cacheKey = CacheKeyForUserFavoriteTracks(loggedUserId);
                        if (_memoryCache.TryGetValue(cacheKey, out _))
                        {
                            // La clé existe dans le cache, alors nous pouvons la supprimer
                            _memoryCache.Remove(cacheKey);
                        }
                    }


                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    //  Utiliser ex.Message pour obtenir le message d'erreur
                    // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                    // Utiliser ex.StackTrace pour obtenir la pile d'appels
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
           else
            {
                return NoContent();
            }
        }
        //--------------------------------------------END TRACK-----------------------------------------
        //-------------------------------------------- PLAYLIST-----------------------------------------
        /// <summary>
        /// Récupère les id des playlist favoris d'un utilisateur.
        /// </summary>
        /// <returns>Retourne  les ids des playlistfavoris d'un utilisateur.</returns>
        [HttpGet("get-logged-user-favorite-playlists-ids")]
        public async Task<ActionResult<List<int>>> GetLoggedUserFavoritePlaylistsIds()
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les ids des olaylists favoris d'un utilisateur.");
                var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
                try
                {
                    //Je récupère toutes les relations UserFavoriteTrack et je vérifie si l'utilisateur aime déja la musique
                    var ids = await _likeRepository.GetUserFavoritePlaylistsIdsList(loggedUserId);
                    return Ok(ids);
                }
                catch (EmptyListException)
                {
                    _logger.LogWarning("Aucun id n'a été trouvé.");
                    return NoContent(); //StatusCode 204
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
                    var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
                                                                 //Je récupère toutes les relations UserFavoriteTrack et je vérifie si l'utilisateur aime déja la musique
                    try
                    {
                        var existingLike = await _likeRepository.GetExistingPlaylistLike(loggedUserId, playlistId);
                        await _likeRepository.RemovePlaylistLikeAsync(existingLike);
                    }
                    catch (NotFoundException)
                    {
                        try
                        {
                            var like = new UserFavoritePlaylist()
                            {
                                UsersId = loggedUserId,
                                PlaylistId = playlistId,
                                LikedAt = DateTime.Now,
                            };
                            await _likeRepository.AddPlaylistLikeAsync(like);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Une erreur est survenue.");
                            return StatusCode(500, $"Internal Server Error: {ex.Message}");
                        }

                    }

                    return NoContent();
                }
                
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    //  Utiliser ex.Message pour obtenir le message d'erreur
                    // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                    // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
        public async Task<ActionResult<List<int>>> GetLoggedUserFavoriteArtistsIds()
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer les ids des artistes favoris d'un utilisateur.");
                var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
               try
                {
                    //Je récupère toutes les relations UserFavoriteTrack et je vérifie si l'utilisateur aime déja la musique
                    var ids = await _likeRepository.GetUserFavoriteArtistsIdsList(loggedUserId);
                    return Ok(ids);
                }
                catch (EmptyListException)
                {
                    _logger.LogWarning("Aucun id n'a été trouvé.");
                    return NoContent(); //StatusCode 204
                }
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
                var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
                                                             //Je récupère toutes les relations UserFavoriteTrack et je vérifie si l'utilisateur aime déja la musique
                try
                {
                    var existingLike = await _likeRepository.GetExistingArtistLike(loggedUserId, artistId);
                    await _likeRepository.RemoveArtistLikeAsync(existingLike);
                    return NoContent(); //Indique le dislike
                }
                catch (NotFoundException)
                {

                    try
                    {
                        var like = new UserFavoriteArtist()
                        {
                            UsersId = loggedUserId,
                            ArtistId = artistId,
                            LikedAt = DateTime.Now,
                        };
                        _context.UserFavoriteArtist.Add(like);
                        await _context.SaveChangesAsync();
                        return Ok(); //Indique le like
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Une erreur est survenue.");
                        return StatusCode(500, $"Internal Server Error: {ex.Message}");
                    }
                }

               
            }          
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

       
        //---------------------------END ARTIST--------------------------------------------------
    }
}
