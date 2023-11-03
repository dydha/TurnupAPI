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
            ) : base(null!,artistRepository,null!,null!, mapper)
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
                if (artist is not null)
                {
                    var artistDTO = _mapper.Map<ArtistDTO>(artist);
                    return Ok(artistDTO);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Ajoute un artiste.
        /// </summary>
        /// <param name="artistForm">Formulaire d'artiste.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [Authorize(Roles = "admin")]
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
              
                var existingArtist = await _artistRepository.GetFilteredArtistAsync(a => (!string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(artistForm.Name) && a.Name.ToLower().Equals(artistForm.Name.ToLower()))
                                                                                                 && ((!string.IsNullOrEmpty(a.Country) && !string.IsNullOrEmpty(artistForm.Country) &&  a.Country.ToLower().Equals(artistForm.Country.ToLower()))));
                if(existingArtist is not  null) 
                {
                    _logger.LogWarning("Un artiste avec ce nom existe déja.");
                    return Conflict(); // StatusCode 409
                }
                else
                {
                    _logger.LogInformation("Ajout d'un nouvel artiste.");
                    var artist = _mapper.Map<Artist>(artistForm);
                    await _artistRepository.AddAsync(artist);
                    return NoContent();
                }

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
        [Authorize(Roles = "admin")]
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
                string message  = result ? "L'artiste a été modifié avec succès" : "L'artiste n'a pas pu etre modifié.";
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
        [Authorize(Roles = "admin")]
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
        /// Retourne tous les  artistes
        /// </summary>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpGet("get-all-artists")]
        public async Task<ActionResult<IEnumerable<ArtistDTO>>> GetAllArtists()
        {
            _logger.LogInformation("Requete pour récupérer tous les artistes.");
            var cacheKey = CacheKeyForArtists();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if(data != null)
            {
                var artists = DeserializeData<List<ArtistDTO>>(data);
                return Ok(artists);
            }
            else
            {
                try
                {
                    var artists = await _artistRepository.GetAllAsync();
                    var artistsDTO = Enumerable.Empty<ArtistDTO>();
                    if (artists.Any())
                    {
                         artistsDTO = MapToListArtistsDTO(artists);
                        await _distributedCache.SetAsync(cacheKey, SerializeData(artistsDTO), GetCacheOptions());
                       
                    }
                    return Ok(artistsDTO);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
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
                var favoriteArtistsMapped = Enumerable.Empty<ArtistDTO>();
                if (favoriteArtists.Any())
                {
                     favoriteArtistsMapped = MapToListArtistsDTO(favoriteArtists);

                }
                return Ok(favoriteArtistsMapped);                       

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

       

       

       
    }
}
