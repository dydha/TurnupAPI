using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;


namespace TurnupAPI.Controllers
{
    /// <summary>
    ///contrôleur de la playlist.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    public class PlaylistController : BaseController
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ILogger<PlaylistController> _logger;
        /// <summary>
        /// Constructeur du contrôleur de l'artiste.
        /// </summary>

        public PlaylistController
            (
                IPlaylistRepository playlistRepository,
                ITrackRepository trackRepository,
                TurnupContext context,
                IUserRepository userRepository,
                ILikeRepository likeRepository,
                HtmlEncoder htmlEncoder,
                 IMapper mapper,
                 ILogger<PlaylistController> logger
            ) : base(userRepository, null, trackRepository,context, mapper)
        { 
            _playlistRepository = playlistRepository;
            _likeRepository = likeRepository;
            _htmlEncoder = htmlEncoder;
            _logger = logger;
        }
       
        /// <summary>
        /// Récupère une playlist par son ID.
        /// </summary>
        /// <param name="id">ID de l playlist à récupérer.</param>
        /// <returns>Retourne la playlist.</returns>

        [HttpGet("get-playlist/{id}")]
        public async Task<ActionResult<PlaylistDTO>> GetPlaylist(int playlistId)
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer une playlist par son id.");
                var playlist = await _playlistRepository.GetAsync(playlistId);
                if (playlist is null)
                {
                    return NotFound();
                }
                var loggedUserId = await GetLoggedUserIdAsync();            
                if(string.IsNullOrEmpty(loggedUserId)) 
                {
                    return StatusCode(500);
                }
              
                if (LoggedUserNotAuthorizedToSeeThisPlaylist(playlist, loggedUserId))
                {
                    return Unauthorized(); //StatusCode 401;
                }
                var playlistDTO = playlist is  null ? new PlaylistDTO() : _mapper.Map<PlaylistDTO>(playlist) ;
                return Ok(playlistDTO);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Ajoute une playlist.
        /// </summary>
        /// <param name="input">Formulaire de la playlist.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpPost("add-playlist")]
        public async Task<ActionResult> AddPlaylist([FromBody] PlaylistForm input)
        {
            if(!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            try
            {
                _logger.LogInformation("Création d'une playlist.");
                input.Name =_htmlEncoder.Encode(input.Name!);
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
                var existingPlaylist = await _playlistRepository.PlaylistExistsAsync(input, loggedUserId);
                if (existingPlaylist is  null) 
                {
                    var playlist = MapToPlaylist(input, loggedUserId);
                    await _playlistRepository.AddAsync(playlist);
                    return NoContent();                   
                }
                _logger.LogWarning("L'auteur possède déjà une playlist avec ce nom.");
                return Conflict(); //StatusCode 409;                                                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Met à jour  une playlist.
        /// </summary>
        /// <param name="playlist">playlist.</param>
        /// <returns>Le résultat de l'opération.</returns>

        [HttpPut("update-playlist/{id}")]
        public async Task<ActionResult> UpdatePlaylist(int id, PlaylistDTO playlistDTO)
        {
            
            if (playlistDTO.Id != id)
            {
                return BadRequest();
            }
            try
            {
                _logger.LogInformation("Requete pour modifier une playlist.");
                var playlist = await _playlistRepository.GetAsync(playlistDTO.Id);
                if (playlist is  null)
                {
                    return NotFound();
                }
                    playlistDTO.Name= _htmlEncoder.Encode(playlistDTO.Name!);
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
                if (loggedUserId.Equals(playlistDTO.OwnerId))
                {                
                    playlist.Name = playlistDTO.Name;
                    playlist.IsPrivate = playlistDTO.IsPrivate;
                    bool result = await _playlistRepository.UpdateAsync(playlist);
                    var message = result ? "Playlist mis à jour avec succès" : "La playlist n'a pas pu etre mis à jour.";                                                
                    return Ok(message);
                }              
                _logger.LogWarning("Suppression impossible l'utilisateur connecté n'est pas l'auteur de la playlist.");
                return Unauthorized(); //StatusCode 501                              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime  une playlist.
        /// </summary>
        /// <param name="id">l'id de la playlist.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpDelete("delete-playlist/{id:int}")]
        public async Task<ActionResult> DeletePlaylist(int id)
        {
            try
            {
                _logger.LogInformation("Requete pour supprimer une playlist.");
                var loggedUserId = await GetLoggedUserIdAsync();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }
                var playlist = await _playlistRepository.GetAsync(id);
                if(playlist is null)
                {
                    return NotFound();               
                }
                if (playlist.UsersId == loggedUserId)
                {
                    bool result =  await _playlistRepository.DeleteAsync(id);
                    var message = result ? "Playlist supprimée avec succès" : "La playlist n'a pas pu etre supprimée.";
                    return Ok(message);
                }
                return StatusCode(401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne les playlists de l'utilisateur connecté.
        /// </summary>
        /// <returns>Une liste de playlist.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-user-playlists/{userId}")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetUserPlaylists(string userId, int offset =0, int limit = 20)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }
            try
            {
                _logger.LogInformation("Requete pour récupérer les playlists d'un utilisateurs.");
                var playlistsMapped = Enumerable.Empty<PlaylistDTO>();
                var user = await _userRepository.GetUserAsync(userId);
                if (user is null)
                {
                    return StatusCode(500);
                }
                var playlists = await _playlistRepository.GetPlaylistByUserIdAsync(userId,offset,limit);
                if(playlists is not null && playlists.Any())
                {
                    playlistsMapped = MapToEnumerablePlaylistDTO(playlists);
                }
                return Ok(playlistsMapped);
            }          
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }

        }

        

        

        /// <summary>
        /// Ajoute une musique à une playlist
        /// </summary>
        
        [HttpPost("add-track-to-playlist")] // Méthode pour liker et dislike une musique
        public async Task<ActionResult> AddTrackToPlaylist([FromBody] AddTrackToPlaylistForm input)
        {
            if(!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            try
            {
                _logger.LogInformation("Requete pour ajouter une musique à une playlist.");
                var loggedUserId = await GetLoggedUserIdAsync(); // Je récupère l'utilisateur connecté
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return StatusCode(500);
                }

                var playlist = await _playlistRepository.GetAsync(input.PlaylistId);
                if(playlist is null)
                {
                    return NotFound();
                }

                var track = await _trackRepository.GetAsync(input.TrackId);

                if( track is null)
                {
                    return NotFound();
                }
                var existingTrackInPlaylist = await _context.PlaylistTrack.Where(pt => pt.PlaylistId == playlist.Id && pt.TrackId == track.Id).FirstOrDefaultAsync();
                if (existingTrackInPlaylist is null)
                {
                    var newPlaylistTrack = new PlaylistTrack
                    {
                        TrackId = track.Id,
                        PlaylistId = playlist.Id,
                    };
                    _context.PlaylistTrack.Add(newPlaylistTrack);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                return Conflict(); // StatusCode 409
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        /// <summary>
        /// Retourne les playlist favoris d'un utilisateur.
        /// </summary>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [HttpGet("get-favorite-playlists/{userId}")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetFavoritePlaylists(string userId, int offset =0, int limit = 20)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer les playlists favoris d'un utilisateur.");
                var user = await _userRepository.GetUserAsync(userId);
                if (user is null)
                {
                    return StatusCode(500);
                }
                var favoritePlaylistsMapped = Enumerable.Empty<PlaylistDTO>();
                var favoritePlaylists = await _likeRepository.GetUserFavoritePlaylists(userId, offset, limit);
                if(favoritePlaylists.Any())
                {
                    favoritePlaylistsMapped = MapToEnumerablePlaylistDTO(favoritePlaylists);
                }
                return Ok(favoritePlaylistsMapped);               
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
       
    }
}
