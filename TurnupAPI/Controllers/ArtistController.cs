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
      
        /// <summary>
        /// Constructeur du contrôleur de l'artiste.
        /// </summary>

        public ArtistController( 
            IArtistRepository artistRepository,
            ILikeRepository likeRepostory,
            IDistributedCache distributedCache,
             IMapper mapper
            ) : base(null,artistRepository,null,null,mapper)
        { 
            _likeRepository = likeRepostory;
            _distributedCache = distributedCache;
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
                var artist = await _artistRepository.GetAsync(id);
                var artistDTO = _mapper.Map<ArtistDTO>(artist);
                return Ok(artistDTO);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
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
                return BadRequest(ModelState);
            }
            try
            {
              
                var existingArtist = await _artistRepository.GetFilteredArtistAsync(a => (!string.IsNullOrEmpty(a.Name) && !string.IsNullOrEmpty(artistForm.Name) && a.Name.ToLower().Equals(artistForm.Name.ToLower()))
                                                                                                 && ((!string.IsNullOrEmpty(a.Country) && !string.IsNullOrEmpty(artistForm.Country) &&  a.Country.ToLower().Equals(artistForm.Country.ToLower()))));  
                
                throw new DuplicateException(); // Si l'artiste existe une exception est levée.

            }
            catch (NotFoundException) // Si l'artiste n'existe pas il est crée.
            {
                try
                {
                    var artist = _mapper.Map<Artist>(artistForm);
                    await _artistRepository.AddAsync(artist);
                    return Ok(new { Message = "Artist enregistré avec succès." });
                }
                catch 
                {
                    throw;
                }
               
            }
            catch (DuplicateException)
            {
                return Conflict(); // StatusCode 409
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
                await _artistRepository.UpdateAsync(artist);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
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
        /// Supprime  un artiste.
        /// </summary>
        /// <param name="artist">artiste.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpDelete("delete-artist/{id}")]
        public async Task<ActionResult> DeleteArtist(int id)
        {
            try
            {
                await _artistRepository.DeleteAsync(id);
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
        /// Retourne tous les  artistes
        /// </summary>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpGet("get-all-artists")]
        public async Task<ActionResult<List<ArtistDTO>>> GetAllArtists()
        {
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
                    return NoContent(); // StatusCode 204
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
        /// Retourne les artistes favoris d'un utilisateur.
        /// </summary>
        [HttpGet("get-favorite-artists/{userId}")]
        public async Task<ActionResult<List<ArtistDTO>>> GetFavoriteArtists(string userId)
        {
            try
            {
                var favoriteArtists = await _likeRepository.GetUserFavoriteArtists(userId);
                 return Ok(MapToListArtistsDTO(favoriteArtists));           

            }
            catch (EmptyListException)
            {
                return NoContent(); //StatusCode 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

       

        /// <summary>
        /// Convertit un en List.
        /// </summary>
        private List<ArtistDTO> MapToListArtistsDTO(List<Artist> artists)
        {
            var artistsDTO = artists.Select(a => _mapper.Map<ArtistDTO>(a)).ToList();
            return artistsDTO;
        }

       
    }
}
