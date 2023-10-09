using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Data;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;


namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur de l'artiste.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    public class ArtistController : BaseController
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILikeRepository _likeRepository;
        private readonly ILogger<ArtistController> _logger;
      
        /// <summary>
        /// Constructeur du contrôleur de l'artiste.
        /// </summary>

        public ArtistController( 
            IArtistRepository artistRepository,
            ILikeRepository likeRepostory,
            IDistributedCache distributedCache,
            ILogger<ArtistController> logger,
             IMapper mapper
            ) : base(null,artistRepository,null,null,mapper)
        { 
            _likeRepository = likeRepostory;
            _distributedCache = distributedCache;
            _logger = logger;
        }


        /// <summary>
        /// Récupère un artiste par son ID.
        /// </summary>
        /// <param name="id">ID de l'artiste à récupérer.</param>
        /// <returns>Le résultat de l'artiste.</returns>
        
        [HttpGet("get-artist/{id:int}")]
        public async Task<ActionResult<ArtistDTO>> GetArtist(int id)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer un artiste par son id.");
                var artist = await _artistRepository.GetAsync(id);
                var artistDTO = _mapper.Map<ArtistDTO>(artist);
                return Ok(artistDTO);
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("L'artiste n'a pas été trouvé.");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500,ex.Message);
            }
        }
        /// <summary>
        /// Ajoute un artiste.
        /// </summary>
        /// <param name="artistForm">Formulaire d'artiste.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpPost("add-artist")]
        public async Task<IActionResult> AddArtist([FromForm] ArtistForm artistForm)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Formulaire non valide.");
                return BadRequest(ModelState);
            }
            try
            {
              
                var existingArtist = await _artistRepository.GetFilteredArtistAsync(a => (!string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(artistForm.Name) && a.Name.ToLower().Equals(artistForm.Name.ToLower()))
                                                                                                 && ((!string.IsNullOrEmpty(a.Country) && !string.IsNullOrEmpty(artistForm.Country) &&  a.Country.ToLower().Equals(artistForm.Country.ToLower()))));
                _logger.LogWarning("Un artiste avec ce nom existe déja.");
                return Conflict(); // StatusCode 409

            }
            catch (NotFoundException) // Si l'artiste n'existe pas il est crée.
            {
                try
                {
                    _logger.LogInformation("Ajout d'un nouvel artiste.");
                    var artist = _mapper.Map<Artist>(artistForm);
                    await _artistRepository.AddAsync(artist);
                    return NoContent();
                }
              
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
                }
               
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                //  Utiliser ex.Message pour obtenir le message d'erreur
                // Utiliser ex.GetType().Name pour obtenir le nom de la classe de l'exception
                // Utiliser ex.StackTrace pour obtenir la pile d'appels
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
                await _artistRepository.UpdateAsync(artist);
                return NoContent();
            }
            catch (NotFoundException)
            {
                _logger.LogWarning( "L'artiste n'a pas été trouvé.");
                return NotFound();
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
                await _artistRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("L'artiste n'a pas été trouvé.");
                return NotFound(); // Return a 404 if the form is not found
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
        /// Retourne tous les  artistes
        /// </summary>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpGet("get-all-artists")]
        public async Task<ActionResult<List<ArtistDTO>>> GetAllArtists()
        {
            _logger.LogInformation("Requete pour récupérer tous les artistes.");
            var cacheKey = CacheKeyForArtists();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if(data != null)
            {
                var artists = DeserializeData<List<ArtistDTO>>(data);
                Console.WriteLine("These artists came from cache memory.");
                return Ok(artists);
            }
            else
            {
                try
                {
                    var artists = await _artistRepository.GetAllAsync();
                    var artistsDTO = MapToListArtistsDTO(artists);
                    await _distributedCache.SetAsync(cacheKey, SerializeData(artistsDTO), GetCacheOptions());
                    Console.WriteLine("These artists do not came from cache memory.");
                    return Ok(artistsDTO);
                }
                catch (EmptyListException)
                {
                    _logger.LogWarning("Aucun artiste n'a été trouvé.");
                    return NoContent(); // StatusCode 204
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
           
        }
        /// <summary>
        /// Retourne les artistes favoris d'un utilisateur.
        /// </summary>
        [HttpGet("get-favorite-artists/{userId}")]
        public async Task<ActionResult<List<ArtistDTO>>> GetFavoriteArtists(string userId)
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les artistes favoris d'un utilisateur.");
                var favoriteArtists = await _likeRepository.GetUserFavoriteArtists(userId);
                 return Ok(MapToListArtistsDTO(favoriteArtists));           

            }
            catch (EmptyListException)
            {
                _logger.LogWarning("Aucun artiste n'a été trouvé.");
                return NoContent(); //StatusCode 204
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

       

       

       
    }
}
