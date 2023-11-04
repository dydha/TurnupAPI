using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using TurnupAPI.DTO;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des types.
    /// </summary>

    [Route("api/[Controller]")]
    [ApiController]
    public class TypesController : BaseController
    {
        private readonly ITypesRepository _typesRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<TypesController> _logger;
        /// <summary>
        /// Initialise une nouvelle instance du contrôleur des types.
        /// </summary>
        /// <param name="typesRepository">Le repository des types.</param>
        public TypesController(
            ITypesRepository typesRepository,
            IDistributedCache distributedCache,
             IMapper mapper,
             ILogger<TypesController> logger
            ) : base(null,null,null,null,mapper)
        {
            _typesRepository = typesRepository;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <summary>
        /// Récupère un type par son ID.
        /// </summary>
        /// <param name="id">L'ID du type à récupérer.</param>
        /// <returns>Une réponse HTTP contenant le type trouvé ou une réponse NotFound si le type n'existe pas.</returns>
        [HttpGet("get-types/{id}")]
        public async Task<ActionResult<Types>> GetTypes(int id)
        {
            _logger.LogInformation("Requete pour récupérer un genre par son id.");
            try
            {
                var track = await _typesRepository.GetAsync(id);
                if( track is   null ) 
                {
                    return NotFound();
                }
                return Ok(track);
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        

        /// <summary>
        /// Récupère tous les types sous forme de DTO.
        /// </summary>
        /// <returns>Une réponse HTTP contenant la liste de tous les types ou une réponse NoContent si la liste est vide.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-all-types")]
        public async Task<ActionResult<IEnumerable<TypesDTO>>> GetAllTypes(int offset = 0, int limit = 20)
        {
            _logger.LogInformation("Requete pour récupérer tous les genres.");
            var typesDTOs = Enumerable.Empty<TypesDTO>();
            var cacheKey = CacheKeyForTypes();
            var data = await _distributedCache.GetAsync(cacheKey); 
            if (data is  null) 
            {
                try
                {
                    var types = await _typesRepository.GetAllAsync(offset, limit);
                    if (types.Any())
                    {
                        typesDTOs = types.Select(t => _mapper.Map<TypesDTO>(t));
                        await _distributedCache.SetAsync(cacheKey, SerializeData(typesDTOs), GetCacheOptions());
                    }
                    return Ok(typesDTOs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur s'est produite.");
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
               
            }
            typesDTOs = (DeserializeData<IEnumerable<TypesDTO>>(data)).Skip(offset).Take(limit).AsEnumerable();
            return Ok(typesDTOs);

        }
    }
}
