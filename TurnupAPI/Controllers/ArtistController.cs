using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Data;
using TurnupAPI.DTO;
using TurnupAPI.Interfaces;


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
                if (artist is null)
                {
                    return NotFound();
                }
                var artistDTO = _mapper.Map<ArtistDTO>(artist);
                return Ok(artistDTO);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, ex.Message);
            }
        }
       
        /// <summary>
        /// Retourne tous les  artistes
        /// </summary>
        /// <returns>Le résultat de l'opération.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-all-artists")]
        public async Task<ActionResult<IEnumerable<ArtistDTO>>> GetAllArtists(int offset = 0, int limit = 20)
        {
            _logger.LogInformation("Requete pour récupérer tous les artistes.");
            var artistsDTO = Enumerable.Empty<ArtistDTO>();
            var cacheKey = CacheKeyForArtists();
            byte[]? data = await _distributedCache.GetAsync(cacheKey);
            if(data is null)
            {
                try
                {
                   var   artists = await _artistRepository.GetAllAsync(offset, limit);                
                    if (artists.Any())
                    {
                        artistsDTO = MapToEnumerableArtistsDTO(artists);
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
            artistsDTO = (DeserializeData<IEnumerable<ArtistDTO>>(data))
                                               .Skip(offset)
                                               .Take(limit)
                                               .AsEnumerable();
            return Ok(artistsDTO);
           
           
        }
        /// <summary>
        /// Retourne les artistes favoris d'un utilisateur.
        /// </summary>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-favorite-artists/{userId}")]
        public async Task<ActionResult<List<ArtistDTO>>> GetFavoriteArtists(string userId, int offset = 0, int limit = 20)
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer les artistes favoris d'un utilisateur.");
                var favoriteArtists = await _likeRepository.GetUserFavoriteArtists(userId, offset, limit);
                var favoriteArtistsMapped = Enumerable.Empty<ArtistDTO>();
                if (favoriteArtists.Any())
                {
                     favoriteArtistsMapped = MapToEnumerableArtistsDTO(favoriteArtists);
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
