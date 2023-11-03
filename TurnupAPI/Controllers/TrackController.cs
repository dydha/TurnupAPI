
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnupAPI.DTO;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using TurnupAPI.Enums;
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
                var loggedUserId = await GetLoggedUserIdAsync();
                if(track is not null &&  loggedUserId is not  null) 
                {
                    var trackDTO = MapToTrackDTO(track, loggedUserId);
                    return Ok(trackDTO);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        /// <summary>
        /// Ajoute un objet Track.
        /// </summary>
        /// <param name="input">Formulaire de la playlist.</param>
        /// <returns>Le résultat de l'opération.</returns>
        
        [HttpPost("add-track")]
        public async Task<ActionResult> AddTrack([FromBody] TrackForm input)
        {
            _logger.LogInformation("Requete pour ajouter une Track dans la base de données.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Le formulaire n'est pas valide.");
                return BadRequest(ModelState);
            }
             using var transaction = _context.Database.BeginTransaction(); 
            try
            {                            
                var track = MapToTrack(input);              
                await _trackRepository.AddAsync(track);
                await AddTrackPrincipalArtist(track.Id, input.PrincipalArtistId);
                if (input.FeaturingArtists != null && input.FeaturingArtists.Any())
                {
                    await AddTrackFeaturingArtists(track.Id, input.FeaturingArtists);
                }
                if (input.TrackTypes != null && input.TrackTypes.Any())
                {
                    await AddTrackTypes(track.Id, input.TrackTypes);
                }
                transaction.Commit();
                return NoContent();
                
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Mise çà jour d'un objet Track.
        /// </summary>
        /// <param name="track"> l'objet trackDTO.</param>
        /// <param name="id"> l'id trackDTO.</param>
        /// <returns>Met à jour l'objet Track.</returns>
        [Authorize(Roles = "admin")]
        [HttpPut("update-track/{id}")]
        public async Task<ActionResult> UpdateTrack(int id, Track track)
        {
            if (id != track.Id)
            {
                return BadRequest();
            }
            try
            {
                _logger.LogInformation("Requete pour modifier une musique.");
                bool result =  await _trackRepository.UpdateAsync(track);
                string message = result ? "La musique a été mise à jour avec succès." : "La musique n'a pas pu etre mise à jour.";
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Suppression d'un objet Track.
        /// </summary>
        /// <param name="id"> l'id de l'objet Track</param>
        /// <returns>Supprime un objet Track.</returns>
        [Authorize(Roles = "admin")]
        [HttpDelete("delete-track/{id}")]
        public async Task<ActionResult> DeleteTrack(int id)
        {
            try
            {
                _logger.LogInformation("Requete pour supprimer une musique.");
                bool result =  await _trackRepository.DeleteAsync(id);
                string message = result ? "La musique a été supprimée avec succès." : "La musique n'a pas pu etre supprimée.";
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère tous les tracks.
        /// </summary>
        /// <returns>Retourne tous les Track</returns>
       //[ServiceFilter(typeof(CheckIdleTimeout))]
        [HttpGet("get-all-tracks")] //Méthode qui retourne toutes les musiques
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetAllTracks(int offset = 0, int limit = 20)
        {
           
            try
            {
               
                var loggedUserId = await GetLoggedUserIdAsync();
                var tracksMapped = Enumerable.Empty<TrackDTO>();
                if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    var tracks = await _trackRepository.GetAllAsync(offset, limit);
                    _logger.LogInformation("Requete pour récupérer toutes les musiques.", loggedUserId);
                    tracksMapped = MapToListTrackDTO(tracks, loggedUserId);
                }
                return Ok(tracksMapped);
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex,"Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère l'historique d'écoute de l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne l'historique d'écoute de l'utilisateur connecté</returns>
        [HttpGet("listening-history/{userId}")] //Méthode qui retourne l'historique d'écoute d'un utilisateur
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
                if (user is not null)
                {
                    var historicTracks = await _trackRepository.GetUserListeningHistory(user.Id, offset, limit);
                    if(historicTracks.Any())
                    {
                        historicTracksDTO = MapToListTrackDTO(historicTracks, user.Id);
                        var cacheEntryOptions = GetMemoryCacheOptions();
                    }
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
        [HttpGet("discovery")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> Discovery(int offset = 0, int limit = 20)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer la playlist Dicovery.");
                var discorveryTracks = Enumerable.Empty<TrackDTO>();
                var loggedUserId = await GetLoggedUserIdAsync();
                if(!string.IsNullOrEmpty(loggedUserId))
                {
                    var tracksFiltered = await _trackRepository.GetDiscoveryAsync(loggedUserId, offset, limit);
                    discorveryTracks = MapToListTrackDTO(tracksFiltered, loggedUserId);
                }
                return Ok(discorveryTracks);
                
            }                     
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Récupère les musiques populaires avec le plus d'écoute.
        /// </summary>
        /// <returns>Retourne les musiques populaires</returns>
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> Popular(int offset = 0, int limit = 20)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer la playlist Popular.");
                var tracksDTO = Enumerable.Empty<TrackDTO>();
                var loggedUserId = await GetLoggedUserIdAsync();
                if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    var tracks = (await _trackRepository.GetPopularTracksAsync(offset, limit)).ToList();
                    tracksDTO = MapToListTrackDTO(tracks, loggedUserId);
                }
                return Ok(tracksDTO);
               
            }  
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère les musiques récemment ajoutées.
        /// </summary>
        /// <returns>Retourne les musiques récemment ajoutées. </returns>
        [HttpGet("new-tracks-playlist")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> NewTracksPlaylist(int offset = 0, int limit = 20)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer la playlist NewTrackPlaylist.");
                var tracksDTO = Enumerable.Empty<TrackDTO>();
                var loggedUserId = await GetLoggedUserIdAsync();
                if (!string.IsNullOrEmpty(loggedUserId))
                {
                    var lastMonth = DateTime.Now.AddMonths(-1); // Obtenir la date du mois dernier
                    var tracks = (await _trackRepository.GetAllAsync(offset, limit))
                        .Where(t => t.AddedAt >= lastMonth) // Filtrez les éléments ajoutés après le mois dernier
                        .OrderByDescending(t => t.AddedAt)
                        .ToList();

                    tracksDTO = MapToListTrackDTO(tracks, loggedUserId);
                }
                return Ok(tracksDTO);
               
            }           
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère le top 20 des musiques les plus écoutées.
        /// </summary>
        /// <returns>Retourne le top 20 des musiques les plus écoutées. </returns>
        [HttpGet("top-20")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> Top20(int offset = 0, int limit = 20)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer la playlist Top20.");
                var tracksDTO = Enumerable.Empty<TrackDTO>();
                var loggedUserId = await GetLoggedUserIdAsync();
                if (!string.IsNullOrEmpty(loggedUserId))
                {
                    var tracks = (await _trackRepository.GetPopularTracksAsync(offset, limit)).Take(20).ToList();
                    tracksDTO = MapToListTrackDTO(tracks, loggedUserId);
                }
                return Ok(tracksDTO);        
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
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
                string message = string.Empty;
                if(!string.IsNullOrEmpty(loggedUserId))
                {
                   bool result =  await _trackRepository.DeleteTrackFromPlaylistAsync(input, loggedUserId);
                    message = result ? "La musique a été supprimée de la playlist avec succès." : "La musique n'a pas pu etre supprimée de la playlist.";
                }
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
        [HttpGet("get-tracks-by-artist/{id}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetTracksByArtist(int id, int offset =0, int limit = 20) //Id de l'artiste
        {
            _logger.LogInformation("Requete pour récupérer toutes les musiques d'un artiste");
            var cacheKey = CacheKeyForArtistTracks(id);
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if(data != null)
            {
                var tracks = DeserializeData<List<TrackDTO>>(data);
                return Ok(tracks);
            }
            else
            {
                try
                {
                    var tracksDTO = Enumerable.Empty<TrackDTO>();
                    var loggedUserId = await GetLoggedUserIdAsync();
                    if(!string.IsNullOrEmpty(loggedUserId)) 
                    {
                        var artist = await _artistRepository.GetAsync(id);
                        if(artist is not null)
                        {
                            var ids = await _context.TrackArtist.Where(tt => tt.ArtistId == artist.Id).Select(tt => tt.TrackId).ToListAsync();
                            if(ids is not null && ids.Any())
                            {
                                var tracks = await _context.Track.Where(t => ids.Contains(t.Id)).Skip(offset).Take(limit).ToListAsync();
                                if(tracks is not null && tracks.Any())
                                {
                                    tracksDTO = MapToListTrackDTO(tracks, loggedUserId);
                                    await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTO), GetCacheOptions());
                                }
                            }
                        }
                    }
                    return Ok(tracksDTO);
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            
        }



        /// <summary>
        ///  récupère et retourne les musiques d'un Type.
        /// </summary>
        [HttpGet("get-tracks-by-types/{typesId}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetTracksByTypes(int typesId, int offset = 0, int limit = 20)
        {
            _logger.LogInformation("Requete pour récupérer toutes les musiques d'un genre");
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForTypesTracks(typesId);
            byte[]? data = _distributedCache.Get(cacheKey);
            if(data != null)
            {
                tracksDTOs = (DeserializeData<IEnumerable<TrackDTO>>(data)).Skip(offset).Take(limit);
                return Ok(tracksDTOs);
            }
            else
            {
                try
                {
                    var loggedUserId = await GetLoggedUserIdAsync();
                    if(!string.IsNullOrEmpty(loggedUserId)) 
                    {
                        var type = await _typesRepository.GetAsync(typesId);
                        if(type is not null)
                        {
                            var tracks = await _trackRepository.GetTracksByTypesAsync(typesId, offset, limit);
                            if(tracks.Any())
                            {
                                tracksDTOs = MapToListTrackDTO(tracks, loggedUserId);
                                await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTOs), GetCacheOptions());
                            }
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
                if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    bool isTrackExists = await _trackRepository.TrackExists(trackId);
                    if (isTrackExists)
                    {
                        var track = await _trackRepository.GetAsync(trackId); 
                        if(track is not null)
                        {
                            var userLT = new UserListennedTrack
                            {
                                TrackId = trackId,
                                UsersId = loggedUserId,
                            };
                            _context.UserListennedTrack.Add(userLT);
                            await _context.SaveChangesAsync();

                            var cacheKey = CacheKeyForUserListeningHistory(loggedUserId);
                            if (_memoryCache.TryGetValue(cacheKey, out _))
                            {
                                // La clé existe dans le cache, alors nous pouvons la supprimer
                                _memoryCache.Remove(cacheKey);
                            }
                            
                        }
                        return NoContent();
                    }
                }
                return NotFound();


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
        [HttpGet("get-tracks-by-playlist/{playlistId}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetTracksByPlaylist(int playlistId, int offset = 0, int limit = 20)
        {
            try
            {
                var playlistTracks = Enumerable.Empty<TrackDTO>();
                var loggedUserId = await GetLoggedUserIdAsync();
                if(!string.IsNullOrEmpty(loggedUserId) ) 
                {
                    var playlist = await _context.Playlist.FindAsync(playlistId);
                    if(playlist is not null) 
                    {
                        if ( LoggedUserNotAuthorizedToSeeThisPlaylist(playlist, loggedUserId))
                        {
                            return Unauthorized(); //StatusCode 401;
                        }
                       var  tracks = await _trackRepository.GetTracksByPlaylistAsync(playlist.Id, offset, limit);
                        if(tracks.Any()) 
                        {
                            playlistTracks = MapToListTrackDTO(tracks, loggedUserId);
                        }
                       
                    }
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
        [HttpGet("get-favorite-tracks/{userId}")]
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetFavoriteTracks(string userId, int offset = 0, int limit = 20)
        {
            _logger.LogInformation("Requete pour récupérer les musiques favoris d'un utilisateur.");
            var tracksDTOs = Enumerable.Empty<TrackDTO>();
            var cacheKey = CacheKeyForUserFavoriteTracks(userId);
            if (_memoryCache.TryGetValue(cacheKey, out tracksDTOs))
            {
                return Ok(tracksDTOs);
            }
            else
            {

                try
                {
                    var favoriteTracks = await _likeRepository.GetUserFavoriteTracks(userId, offset, limit);
                    if(favoriteTracks.Any())
                    {
                        tracksDTOs = MapToListTrackDTO(favoriteTracks, userId);
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
           
        }

        

        /// <summary>
        /// Méthode qui ajoute l'artist principal lors de la création d'un objet Track.
        /// </summary>
        private async Task AddTrackPrincipalArtist(int trackId, int artistId)
        {
            //Création de l'artiste principal 
            var principalArtist = await _artistRepository.GetAsync(artistId);
            if(principalArtist is not null)
            {
                TrackArtist principal = new()
                {
                    ArtistId = principalArtist.Id,
                    ArtistRole = ArtistRole.Principal,
                    TrackId = trackId
                };
                _context.TrackArtist.Add(principal);
                await _context.SaveChangesAsync();
            }
           
        }

        /// <summary>
        /// Méthode qui ajoute les artistes en featuring  lors de la création d'un objet Track.
        /// </summary>
        private async Task AddTrackFeaturingArtists(int trackId, IEnumerable<int> artistsIds)
        {
            List<Artist> featuringArtists = new();
            foreach (var item in artistsIds)
            {
                var artist = await _artistRepository.GetAsync(item);
                if(artist is not null)
                {
                    featuringArtists.Add(artist);
                }
            }
            if (featuringArtists.Any())
            {
                foreach (var item in featuringArtists)
                {
                    TrackArtist featuring = new()
                    {
                        ArtistId = item.Id,
                        ArtistRole = ArtistRole.Featuring,
                        TrackId = trackId };
                    _context.TrackArtist.Add(featuring);
                }
                await _context.SaveChangesAsync();
            }

        }


        /// <summary>
        /// Méthode qui ajoute les types de la musique  lors de la création d'un objet Track.
        /// </summary>
        private async Task AddTrackTypes(int trackId, IEnumerable<int> typesIds)
        {
            List<Types> types = new();  
            if(typesIds.Any())
            {
                foreach (int id in typesIds)
                {
                    var type = await _typesRepository.GetAsync(id);
                    if(type is not null)
                    {
                        types.Add(type);
                    }
                }
               
            }
            if(types.Any())
            {
                foreach (Types type in types)
                {
                    TrackType trackType = new() { TrackId = trackId, TypeId = type.Id };
                    _context.TrackType.Add(trackType);
                }
                await _context.SaveChangesAsync();
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
                if (loggedUser is not null)
                {
                    var userLastPlayingTracksCacheKey = CacheKeyForUserLastPlayingTracks(loggedUser.Id);
                    var data = await _distributedCache.GetAsync(userLastPlayingTracksCacheKey);
                    if (data != null)
                    {
                        lastPlayingTracks = DeserializeData<PlaylingTracks>(data);

                    }
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
