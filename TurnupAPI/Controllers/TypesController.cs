using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using TurnupAPI.DTO;
using TurnupAPI.Forms;
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
                if( track is not  null ) 
                {
                    return Ok(track);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Une erreur s'est produite.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ajoute un nouveau type.
        /// </summary>
        /// <param name="typeVM">Les informations du type à ajouter.</param>
        /// <returns>Une réponse HTTP indiquant le succès de l'ajout ou une réponse BadRequest si les données ne sont pas valides.</returns>
        [Authorize(Roles ="admin")]
        [HttpPost("add-types")]
        public async Task<ActionResult> AddTypes([FromBody] TypeForm typeVM)
        {
            _logger.LogInformation("Requete pour ajouter un genre.");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingTypes = (await _typesRepository.GetAllAsync())
                                    .Where(t => t.Name.Equals(typeVM.Name, StringComparison.OrdinalIgnoreCase))
                                    .FirstOrDefault();
                if(existingTypes is not  null)
                {
                    return Conflict();
                }
                Types type = new()
                {
                    Name = typeVM.Name,
                    Picture = $@"picture/{typeVM.Picture}"
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
            if (id != type.Id)
            {
                return BadRequest();
            }
           
            bool result =  await _typesRepository.UpdateAsync(type);
            string message = result ? "Le genre a été mis à jour avec succès." : "Le genre n'a pas pu etre mis à jour.";
            return Ok(message);
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

        /// <summary>
        /// Récupère tous les types sous forme de DTO.
        /// </summary>
        /// <returns>Une réponse HTTP contenant la liste de tous les types ou une réponse NoContent si la liste est vide.</returns>
        [HttpGet("get-all-types")]
        public async Task<ActionResult<IEnumerable<TypesDTO>>> GetAllTypes()
        {
            _logger.LogInformation("Requete pour récupérer tous les genres.");
            var typesDTOs = Enumerable.Empty<TypesDTO>();
            var cacheKey = CacheKeyForTypes();
            var data = await _distributedCache.GetAsync(cacheKey); 
            if (data is not  null) 
            {
                 typesDTOs = DeserializeData<IEnumerable<TypesDTO>>(data);
                return Ok(typesDTOs);
            }
            else
            {
                try
                {
                    var types = await _typesRepository.GetAllAsync();
                    if(types.Any())
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
           
        }
    }
}
