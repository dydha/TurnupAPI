
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnupAPI.DTO;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using TurnupAPI.Enums;
using TurnupAPI.Forms;
using TurnupAPI.Exceptions;
using TurnupAPI.Data;
using Microsoft.Extensions.Caching.Distributed;
using TurnupBlazor.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using AutoMapper;

namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des artistes.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
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
        public async Task<ActionResult<Track>> GetTrack(int id)
        {
            
            try    
            {
                _logger.LogInformation("Requete pour récupérer une Track par son id.");
                var track = await _trackRepository.GetAsync(id);
                return Ok(track);
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("La Track n'a pas été trouvée.");
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
        public async Task<ActionResult> AddTrack([FromForm] TrackForm input)
        {
            _logger.LogInformation("Requete pour ajouter une Track dans la base de données.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Le formulaire n'est pas valide.");
                return BadRequest(ModelState);
            }
            try
            {
                var track = MapToTrack(input);
                await _trackRepository.AddAsync(track); //J'enregistre la mausique dans la base de données 

                await AddTrackPrincipalArtist(track.Id, input.PrincipalArtistId); //Ajout de l'artiste principal
                if (input.FeaturingArtists != null && input.FeaturingArtists.Any()) // Si le Track contient des artistes en featuring, je récupère tous les artistes et je les ajoutes Dans la featuringArtists
                {
                    await AddTrackFeaturingArtists(track.Id, input.FeaturingArtists);
                }


                //Création de la table TrackType
                if (input.TrackTypes != null && input.TrackTypes.Any())
                {
                    await AddTrackTypes(track.Id, input.TrackTypes);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Mise çà jour d'un objet Track.
        /// </summary>
        /// <param name="track"> l'objet trackDTO.</param>
        /// <param name="id"> l'id trackDTO.</param>
        /// <returns>Met à jour l'objet Track.</returns>
        [HttpPut("update-track/{id}")]
        public async Task<ActionResult> UpdateTrack(int id, Track track)
        {
            if (id != track.Id)
            {
                return BadRequest();
            }
            try
            {
                await _trackRepository.UpdateAsync(track);
                return NoContent();
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Suppression d'un objet Track.
        /// </summary>
        /// <param name="id"> l'id de l'objet Track</param>
        /// <returns>Supprime un objet Track.</returns>
        [HttpDelete("delete-track/{id}")]
        public async Task<ActionResult> DeleteTrack(int id)
        {
            try
            {
                await _trackRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère tous les tracks.
        /// </summary>
        /// <returns>Retourne tous les Track</returns>
        [HttpGet("get-all-tracks")] //Méthode qui retourne toutes les musiques
        public async Task<ActionResult<List<TrackDTO>>> GetAllTracks()
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    //Je récupère toutesles musiques
                    var tracks = await _trackRepository.GetAllAsync();
                    return Ok(MapToListTrackDTO(tracks, loggedUser.Id)); // Je convertis la List<Track> en List<TrackDTO> avant d'etre retournée.
                }
                catch (EmptyListException)
                {
                    return NoContent(); //StatusCode 204;
                }
            }
           
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère l'historique d'écoute de l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne l'historique d'écoute de l'utilisateur connecté</returns>
        [HttpGet("listening-history/{userId}")] //Méthode qui retourne l'historique d'écoute d'un utilisateur
        public async Task<ActionResult<List<TrackDTO>>> ListeningHistory(string userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }
           
            var cacheKey = CacheKeyForUserListeningHistory(userId);
            if(_memoryCache.TryGetValue(cacheKey, out  List<TrackDTO>? cachedData))
            {
                return Ok(cachedData);
            }
            else
            {
                try
                {
                    var user = await _userRepository.GetUserAsync(userId); // Jerécupère l'utilisateur connecté                                                               
                    try
                    {
                        var historicTracks = await _trackRepository.GetUserListeningHistory(user.Id);
                        var historicTracksDTO = MapToListTrackDTO(historicTracks, user.Id);

                        var cacheEntryOptions = GetMemoryCacheOptions();

                        _memoryCache.Set(cacheKey, historicTracksDTO, cacheEntryOptions);
                        return Ok(historicTracksDTO);
                    }
                    catch (EmptyListException)
                    {
                        return NoContent(); //StatusCode 204
                    }
                }
                catch (NotFoundException)
                {
                    return NotFound(); // Return a 404 if the form is not found
                }
                catch (Exception ex)
                {
                    //  Utiliser ex.Message pour obtenir le message d'erreur
                    // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                    // Utiliser ex.StackTrace pour obtenir la pile d'appels
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            
        }

        /// <summary>
        /// Récupère les musique non écouté par  l'utilisateur connecté.
        /// </summary>
        /// <returns>Retourne les musique non écouté par  l'utilisateur connecté</returns>
        [HttpGet("discovery")]
        public async Task<ActionResult<List<TrackDTO>>> Discovery()
        {
            try
            {
                //Meme fonctionnement que la méthode qui retourne l'hisorique d'écoute de l'utilisateur connecté
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    var tracksFiltered = await _trackRepository.GetDiscoveryAsync(loggedUser.Id);
                    var discorveryTracks = MapToListTrackDTO(tracksFiltered, loggedUser.Id);
                    return Ok(discorveryTracks);
                }
                catch (EmptyListException)
                {
                    return NoContent(); //StatusCode 204
                }
            }          
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Récupère les musiques populaires avec le plus d'écoute.
        /// </summary>
        /// <returns>Retourne les musiques populaires</returns>
        [HttpGet("popular")]
        public async Task<ActionResult<List<TrackDTO>>> Popular()
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();

                try
                {
                    var tracks = (await _trackRepository.GetPopularTracksAsync()).ToList();
                    var tracksDTO = MapToListTrackDTO(tracks, loggedUser.Id);
                    return Ok(tracksDTO);
                }
                catch (EmptyListException)
                {
                    return NoContent(); //StatusCode 204
                }
            }          
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère les musiques récemment ajoutées.
        /// </summary>
        /// <returns>Retourne les musiques récemment ajoutées. </returns>
        [HttpGet("new-tracks-playlist")]
        public async Task<ActionResult<List<TrackDTO>>> NewTracksPlaylist()
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();
                var tracks = (await _trackRepository.GetAllAsync()).OrderByDescending(t => t.AddedAt).Take(50).ToList();
                var tracksDTO = MapToListTrackDTO(tracks, loggedUser.Id);
                return Ok(tracksDTO);
            }           
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère le top 20 des musiques les plus écoutées.
        /// </summary>
        /// <returns>Retourne le top 20 des musiques les plus écoutées. </returns>
        [HttpGet("top-20")]
        public async Task<ActionResult<List<TrackDTO>>> Top20()
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();
                var tracks = (await _trackRepository.GetPopularTracksAsync()).Take(20).ToList();
                var tracksDTO = MapToListTrackDTO(tracks, loggedUser.Id);
                return Ok(tracksDTO);
            }
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
                var loggedUser = await GetLoggedUserAsync(); // Je récupère l'utilisateur connecté
                                                             //Je récpère la playlist
                await _trackRepository.DeleteTrackFromPlaylistAsync(input, loggedUser.Id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        ///  récupère et retourne les musique d'un artiste.
        /// </summary>
        [HttpGet("get-tracks-by-artist/{id}")]
        public async Task<ActionResult<List<TrackDTO>>> GetTracksByArtist(int id) //Id de l'artiste
        {
            var cacheKey = CacheKeyForArtistTracks(id);
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if(data != null)
            {
                var tracks = DeserializeData<List<TrackDTO>>(data);
                Console.WriteLine("These tracks came from cache memory.");
                return Ok(tracks);
            }
            else
            {
                try
                {
                    var loggedUser = await GetLoggedUserAsync();
                    try
                    {
                        var artist = await _artistRepository.GetAsync(id);
                        var ids = await _context.TrackArtist.Where(tt => tt.ArtistId == artist.Id).Select(tt => tt.TrackId).ToListAsync();
                        var tracks = (await _trackRepository.GetAllAsync()).Where(t => ids.Contains(t.Id)).ToList();
                        var tracksDTO = MapToListTrackDTO(tracks, loggedUser.Id);
                        await _distributedCache.SetAsync(cacheKey, SerializeData(tracksDTO), GetCacheOptions());
                        Console.WriteLine("These tracks do not came from cache memory.");
                        return Ok(tracksDTO);
                    }
                    catch (NotFoundException)
                    {
                        return NotFound(); // Return a 404 if the form is not found
                    }
                }
                catch (NotFoundException)
                {
                    return NotFound(); // Return a 404 if the form is not found
                }
                catch (Exception ex)
                {
                    //  Utiliser ex.Message pour obtenir le message d'erreur
                    // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                    // Utiliser ex.StackTrace pour obtenir la pile d'appels
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            
        }



        /// <summary>
        ///  récupère et retourne les musiques d'un Type.
        /// </summary>
        [HttpGet("get-tracks-by-types/{typesId}")]
        public async Task<ActionResult<List<TrackDTO>>> GetTracksByTypes(int typesId)
        {
            var cacheKey = CacheKeyForTypesTracks(typesId);
            byte[]? data = _distributedCache.Get(cacheKey);
            if(data != null)
            {
                var tracksDTOs = DeserializeData<List<TrackDTO>>(data);
                return Ok(tracksDTOs);
            }
            else
            {
                try
                {
                    var loggedUser = await GetLoggedUserAsync();
                    try
                    {
                        var type = await _typesRepository.GetAsync(typesId);
                        try
                        {
                            var tracks = await _trackRepository.GetTracksByTypesAsync(typesId);
                            var tracksDTO = MapToListTrackDTO(tracks, loggedUser.Id);
                            await _distributedCache.SetAsync(cacheKey,SerializeData(tracksDTO), GetCacheOptions());
                            return Ok(tracksDTO);
                        }
                        catch (EmptyListException)
                        {
                            return NoContent(); //StatusCode 204
                        }
                    }
                    catch (NotFoundException)
                    {
                        return NotFound();
                    }
                }
                catch (NotFoundException)
                {
                    return NotFound(); // Return a 404 if the form is not found
                }
                catch (Exception ex)
                {
                    //  Utiliser ex.Message pour obtenir le message d'erreur
                    // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                    // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    var track = await _trackRepository.GetAsync(trackId);
                    var userLT = new UserListennedTrack
                    {
                        TrackId = track.Id,
                        UsersId = loggedUser.Id,
                    };
                    _context.UserListennedTrack.Add(userLT);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                catch (NotFoundException)
                {
                    return NotFound();
                }
                finally
                {
                    var cacheKey = CacheKeyForUserListeningHistory(loggedUser.Id);
                    if (_memoryCache.TryGetValue(cacheKey, out _))
                    {
                        // La clé existe dans le cache, alors nous pouvons la supprimer
                        _memoryCache.Remove(cacheKey);
                    }
                }
            }
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }

        }

        /// <summary>
        /// Retourne toutes les tracks d'une playlist.
        /// </summary>
        /// <returns>Toutes les musiques (Track) d'une  playlists.</returns>
        [HttpGet("get-tracks-by-playlist/{playlistId}")]
        public async Task<ActionResult<List<TrackDTO>>> GetTracksByPlaylist(int playlistId)
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    var playlist = await _context.Playlist.FindAsync(playlistId);
                    if (playlist != null && playlist.IsPrivate && playlist.UsersId != loggedUser.Id)
                    {
                        return Unauthorized(); //StatusCode 401;
                    }
                    try
                    {
                        var playlistTracks = await _trackRepository.GetTracksByPlaylistAsync(playlistId);
                        return Ok(MapToListTrackDTO(playlistTracks, loggedUser.Id));
                    }
                    catch (EmptyListException)
                    {
                        return NoContent();
                    }
                }
                catch (NotFoundException)
                {
                    return NotFound();
                }
            }
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Récupère les tracks favoris d'un utilisateur.
        /// </summary>
        /// <param name="userId">ID de l'utilisateur.</param>
        /// <returns>Retourne  les tracks favoris d'un utilisateur.</returns>
        [HttpGet("get-favorite-tracks/{userId}")]
        public async Task<ActionResult<List<TrackDTO>>> GetFavoriteTracks(string userId)
        {
            // Utilisez userId comme clé de mise en cache
            var cacheKey = CacheKeyForUserFavoriteTracks(userId);

            // Vérifiez d'abord si la réponse est déjà mise en cache
            if (_memoryCache.TryGetValue(cacheKey, out List<TrackDTO>? cachedValue))
            {
                // Utilisez la réponse mise en cache
                return Ok(cachedValue);
            }
            else
            {
                try
                {
                    var user = await _userRepository.GetUserAsync(userId);
                    try
                    {
                        var favoriteTracks = await _likeRepository.GetUserFavoriteTracks(user.Id);

                        var trackDTOs = MapToListTrackDTO(favoriteTracks, user.Id);
                        // Mettre la réponse en cache avec une durée d'expiration
                        var cacheEntryOptions = GetMemoryCacheOptions();

                        _memoryCache.Set(cacheKey, trackDTOs, cacheEntryOptions);
                        return Ok(trackDTOs);
                    }
                    catch (EmptyListException)
                    {
                        return NoContent(); //StatusCode 204
                    }

                }
                catch (NotFoundException)
                {
                    return NotFound(); // Return a 404 if the form is not found
                }
                catch (Exception ex)
                {
                    //  Utiliser ex.Message pour obtenir le message d'erreur
                    // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                    // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
            //Je crée  l'artist principal et l'enregistre dans la base de données 
            TrackArtist principal = new() {
                ArtistId = principalArtist.Id,
                ArtistRole = ArtistRole.Principal,
                TrackId = trackId };
            _context.TrackArtist.Add(principal);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Méthode qui ajoute les artistes en featuring  lors de la création d'un objet Track.
        /// </summary>
        private async Task AddTrackFeaturingArtists(int trackId, List<int> artistsIds)
        {
            List<Artist> featuringArtists = new();
            foreach (var item in artistsIds)
            {
                var artist = await _artistRepository.GetAsync(item);
                featuringArtists.Add(artist);
            }
            //Je parcours la liste des featuring artist et je les enregistre dans la base de données en leur attribuant le role Featuring
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
        private async Task AddTrackTypes(int trackId, List<int> typesIds)
        {
            List<Types> types = new();  //Idem que pour les featuringArtist
            foreach (int id in typesIds)
            {
                var type = await _typesRepository.GetAsync(id);
                types.Add(type);
            }
            foreach (Types type in types)
            {
                TrackType trackType = new() { TrackId = trackId, TypeId = type.Id };
                _context.TrackType.Add(trackType);
            }
            await _context.SaveChangesAsync();

        }
        

        //------------------------------------GESTION CACHE-----------------------------------------------------

        /// <summary>
        /// Méthode qui récupère la derniere liste de musique en lecture depuis le cache.
        /// </summary>
        /// <returns>retourne la dernière liste de musique en lecture de l'utilisateur connecté.</returns>
        [HttpGet("get-last-playing-tracks")]
        public async Task<ActionResult<PlaylingTracks>> GetLastPlayingTracks()
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();


                var userLastPlayingTracksCacheKey = CacheKeyForUserLastPlayingTracks(loggedUser.Id!); //Je récupère la clé des données des données 
            
                var data = await _distributedCache.GetAsync(userLastPlayingTracksCacheKey); // Je récupère les données.
                if (data != null) //Si les données nes sont pas null je désérialise les données avant de les retourner.
                {
                    var lastPlayingTracks = DeserializeData<PlaylingTracks>(data);
                    Console.WriteLine(lastPlayingTracks.Name + " ou null");
                    return lastPlayingTracks != null && lastPlayingTracks.Tracks != null ? lastPlayingTracks : throw new EmptyListException();
                }
                else
                {
                    Console.WriteLine(" nullissime");
                    throw new EmptyListException();
                }

            }
            catch (EmptyListException)
            {
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound(); // Return a 404 if the form is not found
            }
            catch (Exception ex)
            {
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Méthode qui met à jour la derniere liste de musique en lecture dansle cache.
        /// </summary>
        [HttpPost("set-last-playing-tracks")]
        public async Task<ActionResult> SetLastPlayingTracks([FromBody] PlaylingTracks playingTracks)
        {
           
            var loggedUser = await GetLoggedUserAsync();
            var userLastPlayingTracksCacheKey = CacheKeyForUserLastPlayingTracks(loggedUser.Id);
            await _distributedCache.SetAsync(userLastPlayingTracksCacheKey, SerializeData(playingTracks), GetCacheOptions());

            
            return NoContent();
        }

       
       

       
    }
}
