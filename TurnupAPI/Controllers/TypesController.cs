using AutoMapper;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using TurnupBlazor.Models;

namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des types.
    /// </summary>
    [Authorize]
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
                return Ok(track);
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("Aucun genre n'a été trouvé.");
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
        [HttpPost("add-types")]
        public async Task<ActionResult> AddTypes([FromForm] TypeForm typeVM)
        {
            _logger.LogInformation("Requete pour ajouter un genre.");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
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
        [HttpPut("update-types/{id}")]
        public async Task<ActionResult> UpdateTypes(int id, Types type)
        {
            _logger.LogInformation("Requete pour modifier un genre.");
            if (id != type.Id)
            {
                return BadRequest();
            }

            await _typesRepository.UpdateAsync(type);
            return NoContent();
        }

        /// <summary>
        /// Supprime un type par son ID.
        /// </summary>
        /// <param name="id">L'ID du type à supprimer.</param>
        /// <returns>Une réponse HTTP NoContent en cas de succès, une réponse NotFound si le type n'est pas trouvé, ou une réponse StatusCode 500 en cas d'erreur interne.</returns>
        [HttpDelete("delete-types/{id}")]
        public async Task<ActionResult> DeleteTypes(int id)
        {
            _logger.LogInformation("Requete pour supprimer un genre.");
            try
            {
                await _typesRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentNullException)
            {
                _logger.LogWarning("Le genre n'existe pas.");
                return NotFound();
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
        public async Task<ActionResult<List<TypesDTO>>> GetAllTypes()
        {
            _logger.LogInformation("Requete pour récupérer tous les genres.");
            var cacheKey = CacheKeyForTypes();
            var data = await _distributedCache.GetAsync(cacheKey); // Je récupère les données.
            if (data != null) //Si les données nes sont pas null je désérialise les données avant de les retourner.
            {
                var types = DeserializeData<List<TypesDTO>>(data);
                Console.WriteLine("These types came from cache memory.");
                return Ok(types);
            }
            else
            {
                try
                {
                    var types = await _typesRepository.GetAllAsync();
                    var typesDTOs = types.Select(t => _mapper.Map<TypesDTO>(t)).ToList();

                    await _distributedCache.SetAsync(cacheKey, SerializeData(typesDTOs), GetCacheOptions());
                    Console.WriteLine("These types do not came from cache memory.");
                    return Ok(typesDTOs);
                }
                catch (EmptyListException)
                {
                    _logger.LogWarning("Aucun genre n'a été trouvé.");
                    return NoContent();
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
