using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Enums;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Controllers
{
    [Authorize(Roles ="admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : BaseController
    {
        private readonly ITypesRepository _typesRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<TrackController> _logger;
        private readonly IPlaylistRepository _playlistRepository;
        /// <summary>
        /// Constructeur du contrôleur de l'artiste.
        /// </summary>
        public AdminController(
            ITrackRepository trackRepository,
            ITypesRepository typesRepository,
            IArtistRepository artistRepository,
            TurnupContext context,
            IUserRepository userRepository,
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            ILogger<TrackController> logger,
            IMapper mapper,
            IPlaylistRepository playlistRepository

            ) : base(userRepository, artistRepository, trackRepository, context, mapper)
        {
            _typesRepository = typesRepository;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _logger = logger;
            _playlistRepository = playlistRepository;
        }

        /// <summary>
        /// Ajoute un artiste.
        /// </summary>
        /// <param name="artistForm">Formulaire d'artiste.</param>
        /// <returns>Le résultat de l'opération.</returns>

        [HttpPost("add-artist")]
        public async Task<IActionResult> AddArtist([FromBody] ArtistForm artistForm)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Formulaire non valide.");
                return BadRequest(ModelState);
            }
            try
            {

                var existingArtist = await _artistRepository.ArtistExistsAsync(artistForm);
                if (existingArtist is null)
                {
                    _logger.LogInformation("Ajout d'un nouvel artiste.");
                    var artist = _mapper.Map<Artist>(artistForm);
                    await _artistRepository.AddAsync(artist);
                    return NoContent();
                }
                _logger.LogWarning("Un artiste avec ce nom existe déja.");
                return Conflict(); // StatusCode 409
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500);
            }

        }
        /// <summary>
        /// Met à jour  un artiste.
        /// </summary>
        /// <param name="artist">artiste.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpPut("update-artist/{id}")]
        public async Task<ActionResult> UpdateArtist(int id, Artist artist)
        {
            if (id != artist.Id)
            {
                return BadRequest();
            }
            try
            {
                _logger.LogInformation("Mise à jour d'un artiste.");
                bool result = await _artistRepository.UpdateAsync(artist);
                string message = result ? "L'artiste a été modifié avec succès" : "L'artiste n'a pas pu etre modifié.";
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Supprime  un artiste.
        /// </summary>
        /// <param name="artist">artiste.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpDelete("delete-artist/{id}")]
        public async Task<ActionResult> DeleteArtist(int id)
        {
            try
            {
                _logger.LogInformation("Suppression d'un artiste.");
                bool result = await _artistRepository.DeleteAsync(id);
                string message = result ? "L'artiste a été supprimé avec succès" : "L'artiste n'a pas pu etre supprimé.";
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne toutes les playlists.
        /// </summary>
        /// <returns>Toutes les playlists.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-all-playlists")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetAllPlaylists(int offset = 0, int limit = 20)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer toutes les playlists.");
                var playlistDTOs = Enumerable.Empty<PlaylistDTO>();
                var playlists = await _playlistRepository.GetAllAsync(offset, limit);
                if (playlists.Any())
                {
                    playlistDTOs = MapToEnumerablePlaylistDTO(playlists);
                }
                return Ok(playlistDTOs);

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
                if (input.FeaturingArtists is not null && input.FeaturingArtists.Any())
                {
                    await AddTrackFeaturingArtists(track.Id, input.FeaturingArtists);
                }
                if (input.TrackTypes is not null && input.TrackTypes.Any())
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
        [HttpPut("update-track/{id}")]
        public async Task<ActionResult> UpdateTrack(int id, Track track)
        {
            if (id == track.Id)
            {
                try
                {
                    _logger.LogInformation("Requete pour modifier une musique.");
                    bool result = await _trackRepository.UpdateAsync(track);
                    string message = result ? "La musique a été mise à jour avec succès." : "La musique n'a pas pu etre mise à jour.";
                    return Ok(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
            }
            return BadRequest();

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
                _logger.LogInformation("Requete pour supprimer une musique.");
                bool result = await _trackRepository.DeleteAsync(id);
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
 
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-all-tracks")] //Méthode qui retourne toutes les musiques
        public async Task<ActionResult<IEnumerable<TrackDTO>>> GetAllTracks(int offset = 0, int limit = 20)
        {

            try
            {
                _logger.LogInformation("Requete pour récupérer toutes les musiques.");
                var tracksMapped = Enumerable.Empty<TrackDTO>();
                var tracks = await _trackRepository.GetAllAsync(offset, limit);
                if (tracks.Any())
                {
                    tracksMapped = MapToEnumerableTrackDTO(tracks);
                }
                return Ok(tracksMapped);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Méthode qui ajoute l'artist principal lors de la création d'un objet Track.
        /// </summary>
        private async Task AddTrackPrincipalArtist(int trackId, int artistId)
        {
            //Création de l'artiste principal 
            var principalArtist = await _artistRepository.GetAsync(artistId);
            if (principalArtist is not null)
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
                if (artist is not null)
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
                        TrackId = trackId
                    };
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
            if (typesIds.Any())
            {
                foreach (int id in typesIds)
                {
                    var type = await _typesRepository.GetAsync(id);
                    if (type is not null)
                    {
                        types.Add(type);
                    }
                }

            }
            if (types.Any())
            {
                foreach (Types type in types)
                {
                    TrackType trackType = new() { TrackId = trackId, TypeId = type.Id };
                    _context.TrackType.Add(trackType);
                }
                await _context.SaveChangesAsync();
            }

        }

        /// <summary>
        /// Ajoute un nouveau type.
        /// </summary>
        /// <param name="typeVM">Les informations du type à ajouter.</param>
        /// <returns>Une réponse HTTP indiquant le succès de l'ajout ou une réponse BadRequest si les données ne sont pas valides.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost("add-types")]
        public async Task<ActionResult> AddTypes([FromBody] TypeForm input)
        {
            _logger.LogInformation("Requete pour ajouter un genre.");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var exist = await _typesRepository.TypesExistsAsync(input);
                if (exist)
                {
                    return Conflict();
                }
                Types type = new()
                {
                    Name = input.Name,
                    Picture = $@"picture/{input.Picture}"
                };
                await _typesRepository.AddAsync(type);
                return Ok(new { Message = "Type enregistré avec succès." });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Met à jour un type par son ID.
        /// </summary>
        /// <param name="id">L'ID du type à mettre à jour.</param>
        /// <param name="type">Les nouvelles données du type.</param>
        /// <returns>Une réponse HTTP NoContent en cas de succès ou une réponse BadRequest si l'ID ne correspond pas.</returns>
        [Authorize(Roles = "admin")]
        [HttpPut("update-types/{id}")]
        public async Task<ActionResult> UpdateTypes(int id, Types type)
        {
            _logger.LogInformation("Requete pour modifier un genre.");
            if (id == type.Id)
            {
                bool result = await _typesRepository.UpdateAsync(type);
                string message = result ? "Le genre a été mis à jour avec succès." : "Le genre n'a pas pu etre mis à jour.";
                return Ok(message);
            }
            return BadRequest();

        }

        /// <summary>
        /// Supprime un type par son ID.
        /// </summary>
        /// <param name="id">L'ID du type à supprimer.</param>
        /// <returns>Une réponse HTTP NoContent en cas de succès, une réponse NotFound si le type n'est pas trouvé, ou une réponse StatusCode 500 en cas d'erreur interne.</returns>
        [Authorize(Roles = "admin")]
        [HttpDelete("delete-types/{id}")]
        public async Task<ActionResult> DeleteTypes(int id)
        {
            _logger.LogInformation("Requete pour supprimer un genre.");
            try
            {
                bool result = await _typesRepository.DeleteAsync(id);
                string message = result ? "Le genre a été supprimé avec succès." : "Le genre n'a pas pu etre supprimé.";
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
