
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnupAPI.DTO;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using TurnupAPI.Forms;
using TurnupAPI.Data;
using Microsoft.Extensions.Caching.Distributed;
using TurnupBlazor.Models;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;


namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des artistes.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    //[EnableCors("MyCorsPolicy")]
    public class TrackController : BaseController
    {
        private readonly ITypesRepository _typesRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<TrackController> _logger;
        /// <summary>
        /// Constructeur du contrôleur de l'artiste.
        /// </summary>
        public TrackController(
            ITrackRepository trackRepository,
            ITypesRepository typesRepository,
            IArtistRepository artistRepository,
            ILikeRepository likeRepository,
            TurnupContext context,
            IUserRepository userRepository,
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            ILogger<TrackController> logger,
            IMapper mapper

            ) : base(userRepository,artistRepository,trackRepository,context, mapper)
        {
            _typesRepository = typesRepository;
            _likeRepository = likeRepository;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _logger = logger;
      
        }
       
        /// <summary>
        /// Récupère une Track par son ID.
        /// </summary>
        /// <param name="id">ID de la musique à récupérer.</param>
        /// <returns>Retourne un objet Track.</returns>

        [HttpGet("get-track/{id}")]
        public async Task<ActionResult<TrackDTO>> GetTrack(int id)
        {
            
            try    
            {
                _logger.LogInformation("Requete pour récupérer une Track par son id.");
                var track = await _trackRepository.GetAsync(id);
                if(track is null ) 
                {
                    return NotFound();                  
                }
                var trackDTO = MapToTrackDTO(track);
                return Ok(trackDTO);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
       

        /// <summary>
        /// Récupère l'historique d'écoute de l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne l'historique d'écoute de l'utilisateur connecté</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("listening-history/{userId}")] 
        public async Task<ActionResult<IEnumerable<TrackDTO>>> ListeningHistory(string userId, int offset = 0, int limit = 20)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }
            _logger.LogInformation("Requete pour l'historique d'écoute d'un utilisateur.");
           
            try
            {
                var historicTracksDTO = Enumerable.Empty<TrackDTO>();
                var user = await _userRepository.GetUserAsync(userId);
                if (user is null)
                {
                    return StatusCode(500);
                }
                var historicTracks = await _trackRepository.GetUserListeningHistory(user.Id, offset, limit);
                if(historicTracks.Any())
                {
                    historicTracksDTO = MapToEnumerableTrackDTO(historicTracks);
                }              
                return Ok(historicTracksDTO);
            }               
            catch (Exception ex)
            {
                _logger.LogError(ex,"Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
            
            
        }

        /// <summary>
        /// Récupère les musique non écouté par  l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne les musique non écouté par  l'utilisateur connecté</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("discovery")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> Discovery(int offset = 0, int limit = 20)
        {
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForTop20Tracks();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if (data is null)
            {
                try
                {
                    _logger.LogInformation("Requete pour récupérer la playlist Dicovery.");
                    var loggedUserId = await GetLoggedUserIdAsync();
                    if (string.IsNullOrEmpty(loggedUserId))
                    {
                        return StatusCode(500);
                    }
                    var tracksFiltered = await _trackRepository.GetDiscoveryAsync(loggedUserId, offset, limit);
                    if(tracksFiltered.Any()) 
                    {
                        tracksDTOs = MapToEnumerableTrackDTO(tracksFiltered);
                        await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                    }
                    return Ok(tracksDTOs);
                
                }                     
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit).AsEnumerable();
            return Ok(tracksDTOs);
        }
        /// <summary>
        /// Récupère les musiques populaires avec le plus d'écoute.
        /// </summary>
        /// <returns>Retourne les musiques populaires</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> Popular(int offset = 0, int limit = 20)
        {
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForPopularTracks();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if (data is null)
            {
                try
                {
                    _logger.LogInformation("Requete pour récupérer la playlist Popular.");             
                    var tracks = await _trackRepository.GetPopularTracksAsync(offset, limit);
                    if(tracks.Any())
                    {
                        tracksDTOs = MapToEnumerableTrackDTO(tracks);
                        await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                    }                
                    return Ok(tracksDTOs);
               
                }  
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit).AsEnumerable();
            return Ok(tracksDTOs);
        }

        /// <summary>
        /// Récupère les musiques récemment ajoutées.
        /// </summary>
        /// <returns>Retourne les musiques récemment ajoutées. </returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("new-tracks-playlist")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> NewTracksPlaylist(int offset = 0, int limit = 20)
        {
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForNewTracks();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if (data is null)
            {
                try
                {
                    _logger.LogInformation("Requete pour récupérer la playlist NewTrackPlaylist.");
                    var lastMonth = DateTime.Now.AddMonths(-1); 
                    var tracks = await _trackRepository.GetNewTracks(offset, limit);
                    if(tracks.Any())
                    {
                        tracksDTOs = MapToEnumerableTrackDTO(tracks);
                        await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                    }                          
                    return Ok(tracksDTOs);
               
                }           
           
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit).AsEnumerable();
            return Ok(tracksDTOs);
        }

        /// <summary>
        /// Récupère le top 20 des musiques les plus écoutées.
        /// </summary>
        /// <returns>Retourne le top 20 des musiques les plus écoutées. </returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("top-20")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> Top20(int offset = 0, int limit = 20)
        {
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForTop20Tracks();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if (data is null)
            {
                try
                {
                    _logger.LogInformation("Requete pour récupérer la playlist Top20.");
                    var tracks = await _trackRepository.GetPopularTracksAsync(offset, limit);
                    if(tracks.Any())
                    {
                        tracksDTOs = MapToEnumerableTrackDTO(tracks);
                        await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                    }
                    return Ok(tracksDTOs);        
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit).AsEnumerable();
            return Ok(tracksDTOs);
        }
        /// <summary>
        /// Supprime une musique d'une liste.
        /// </summary>

        [HttpPost("delete-track-from-playlist")]
        public async Task<ActionResult> DeleteTrackFromPlaylist(AddTrackToPlaylistForm input)
        {
            try
            {
                _logger.LogInformation("Requete pour supprimer une musique d'une playlist");
                var loggedUserId = await GetLoggedUserIdAsync();
                if(string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);                
                }
                bool result = await _trackRepository.DeleteTrackFromPlaylistAsync(input, loggedUserId);
                var message = result ? "La musique a été supprimée de la playlist avec succès." : "La musique n'a pas pu etre supprimée de la playlist.";
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        ///  récupère et retourne les musique d'un artiste.
        /// </summary>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-tracks-by-artist/{id}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetTracksByArtist(int id, int offset =0, int limit = 20) 
        {
            _logger.LogInformation("Requete pour récupérer toutes les musiques d'un artiste");
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForArtistTracks(id);
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if(data is null)
            {
                try
                {
                    var artist = await _artistRepository.GetAsync(id);
                    if (artist is null)
                    {
                        return NotFound();
                    }

                    var loggedUserId = await GetLoggedUserIdAsync();
                    if (string.IsNullOrEmpty(loggedUserId))
                    {
                        return StatusCode(500);
                    }
                    
                    var ids = await _context.TrackArtist.Where(tt => tt.ArtistId == artist.Id).Select(tt => tt.TrackId).ToListAsync();
                    if (ids is not null && ids.Any())
                    {
                        var tracks = await _context.Track.Where(t => ids.Contains(t.Id)).Skip(offset).Take(limit).ToListAsync();
                        if (tracks is not null && tracks.Any())
                        {
                            tracksDTOs = MapToEnumerableTrackDTO(tracks);
                            await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                        }
                    }
                    
                    return Ok(tracksDTOs);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit).AsEnumerable();
            return Ok(tracksDTOs);
            
            
        }



        /// <summary>
        ///  récupère et retourne les musiques d'un Type.
        /// </summary>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-tracks-by-types/{typesId}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetTracksByTypes(int typesId, int offset = 0, int limit = 20)
        {
            _logger.LogInformation("Requete pour récupérer toutes les musiques d'un genre");
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForTypesTracks(typesId);
            byte[]? data = _distributedCache.Get(cacheKey);
            if(data is null)
            {
                try
                {                
                    var type = await _typesRepository.GetAsync(typesId);
                    if (type is not null)
                    {
                        var tracks = await _trackRepository.GetTracksByTypesAsync(typesId, offset, limit);
                        if (tracks.Any())
                        {
                            tracksDTOs = MapToEnumerableTrackDTO(tracks);
                            await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                        }
                    }
                    return Ok(tracksDTOs);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
               
            }
            tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit);
            return Ok(tracksDTOs);

        }
        /// <summary>
        ///  Crée une nouvelle UserListennedTrack.
        /// </summary>
        [HttpPost("play-track")] // Méthode qui augmente le nombre de vu de la musique 
        public async Task<IActionResult> PlayTrack([FromBody] int trackId)
        {
            try
            {
                _logger.LogInformation("Une musique est en train de jouer.");
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
               
                var track = await _trackRepository.GetAsync(trackId); 
                if(track is  null)
                {
                    return NotFound();                   
                }
                var userLT = new UserListennedTrack
                {
                    TrackId = trackId,
                    UsersId = loggedUserId,
                };
                _context.UserListennedTrack.Add(userLT);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }

        }

        /// <summary>
        /// Retourne toutes les tracks d'une playlist.
        /// </summary>
        /// <returns>Toutes les musiques (Track) d'une  playlists.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-tracks-by-playlist/{playlistId}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetTracksByPlaylist(int playlistId, int offset = 0, int limit = 20)
        {
            try
            {
                var playlist = await _context.Playlist.FindAsync(playlistId);
                if (playlist is null)
                {
                    return NotFound();
                }
               
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
                
                if (LoggedUserNotAuthorizedToSeeThisPlaylist(playlist, loggedUserId))
                {
                    return Unauthorized(); //StatusCode 401;
                }
                var playlistTracks = Enumerable.Empty<TrackDTO>();
                var tracks = await _trackRepository.GetTracksByPlaylistAsync(playlist.Id, offset, limit);
                if (tracks.Any())
                {
                    playlistTracks = MapToEnumerableTrackDTO(tracks);
                }

                return Ok(playlistTracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Récupère les tracks favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">ID de l'utilisateur.</param>
        /// <returns>Retourne  les tracks favoris d'un utilisateur.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-favorite-tracks/{userId}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetFavoriteTracks(string userId, int offset = 0, int limit = 20)
        {
            _logger.LogInformation("Requete pour récupérer les musiques favoris d'un utilisateur.");
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForUserFavoriteTracks(userId);
            if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<TrackDTO>? tracks))
            {
                tracksDTOs = tracks is not null && tracks.Any() ? tracks.Skip(offset).Take(limit) : tracksDTOs;
                return Ok(tracksDTOs);
            }
          
            try
            {
                var favoriteTracks = await _likeRepository.GetUserFavoriteTracks(userId, offset, limit);
                if(favoriteTracks.Any())
                {
                    tracksDTOs = MapToEnumerableTrackDTO(favoriteTracks);
                    var cacheEntryOptions = GetMemoryCacheOptions();
                    _memoryCache.Set(cacheKey, tracksDTOs, cacheEntryOptions);
                }
                return Ok(tracksDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
            
           
        }

        

      
        

        //------------------------------------GESTION CACHE-----------------------------------------------------

        /// <summary>
        /// Méthode qui récupère la derniere liste de musique en lecture depuis le cache.
        /// </summary>
        /// <returns>retourne la dernière liste de musique en lecture de l'utilisateur connecté.</returns>
        [HttpGet("get-last-playing-tracks")]
        public async Task<ActionResult<PlaylingTracks>> GetLastPlayingTracks()
        {
            _logger.LogInformation("Requete pour récupérer la deniere liste de musique jouée par l'utilisateur connecté.");
            try
            {
                var lastPlayingTracks = new PlaylingTracks();
                var loggedUser = await GetLoggedUserAsync();
                if (loggedUser is null)
                {
                    return StatusCode(500);
                }
                var userLastPlayingTracksCacheKey = CacheKeyForUserLastPlayingTracks(loggedUser.Id);
                var data = await _distributedCache.GetAsync(userLastPlayingTracksCacheKey);
                if (data is not null)
                {
                    lastPlayingTracks = DeserializeData<PlaylingTracks>(data);

                }               
                return Ok(lastPlayingTracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Méthode qui met à jour la derniere liste de musique en lecture dansle cache.
        /// </summary>
        [HttpPost("set-last-playing-tracks")]
        public async Task<ActionResult> SetLastPlayingTracks([FromBody] PlaylingTracks playingTracks)
        {
            _logger.LogInformation("Requete pour enregistrer la derniere liste de musique jouée par l'utilisateur connecté.");
            try
            {
                var loggedUserId = await GetLoggedUserIdAsync();
                if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    var userLastPlayingTracksCacheKey = CacheKeyForUserLastPlayingTracks(loggedUserId);
                    await _distributedCache.SetAsync(userLastPlayingTracksCacheKey, SerializeData(playingTracks), GetCacheOptions());
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
            return NoContent();
        }
    }
}
