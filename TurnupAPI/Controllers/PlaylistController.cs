using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using TurnupAPI.Repositories;


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
        public async Task<ActionResult<PlaylistDTO>> GetPlaylist(int id)
        {
            try
            {
                _logger.LogInformation( "Requete pour récupérer une playlist par son id.");
                var loggedUserId = await GetLoggedUserIdAsync();            
                if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    var playlist = await _playlistRepository.GetAsync(id);
                    if (playlist is not null && LoggedUserNotAuthorizedToSeeThisPlaylist(playlist, loggedUserId))
                    {
                        return Unauthorized(); //StatusCode 401;
                    }
                    var playlistDTO = playlist is not  null ? _mapper.Map<PlaylistDTO>(playlist) : new PlaylistDTO();
                    return Ok(playlistDTO);
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");
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
                if (!string.IsNullOrEmpty(loggedUserId))
                {
                    var existingPlaylist = await _playlistRepository.GetFilteredPlaylistAsync(p => ((!string.IsNullOrEmpty(p.Name) && input.Name.ToLower().Equals(p.Name.ToLower())) && p.UsersId == loggedUserId));
                    if (existingPlaylist is not null) 
                    {
                        _logger.LogWarning("L'auteur possède déjà une playlist avec ce nom.");
                        return Conflict(); //StatusCode 409;
                    }
                    else
                    {
                        var playlist = MapToPlaylist(input, loggedUserId);
                        await _playlistRepository.AddAsync(playlist);
                        return NoContent();
                    }
                   
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");
                            
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
                playlistDTO.Name= _htmlEncoder.Encode(playlistDTO.Name!);
                var loggedUserId = await GetLoggedUserIdAsync();
                if (!string.IsNullOrEmpty(loggedUserId))
                {
                    if (loggedUserId.Equals(playlistDTO.OwnerId))
                    {
                        string message = string.Empty;
                        var playlist = await _playlistRepository.GetAsync(playlistDTO.Id);
                        if(playlist is not null)
                        {
                            playlist.Name = playlistDTO.Name;
                            playlist.IsPrivate = playlistDTO.IsPrivate;
                            bool result = await _playlistRepository.UpdateAsync(playlist);
                            message = result ? "Playlist mis à jour avec succès" : "La playlist n'a pas pu etre mis à jour.";
                                
                        }
                        return Ok(message);
                    }
                    else
                    {
                        _logger.LogWarning("Suppression impossible l'utilisateur connecté n'est pas l'auteur de la playlist.");
                        return Unauthorized(); //StatusCode 501
                    }
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");    
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
               if(!string.IsNullOrEmpty(loggedUserId)) 
                {
                    var playlist = await _playlistRepository.GetAsync(id);
                    if(playlist is not null)
                    {
                        if (playlist.UsersId == loggedUserId)
                        {
                            await _playlistRepository.DeleteAsync(id);
                            return NoContent();
                        }
                        else
                        {
                            return StatusCode(401);
                        }
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");

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
        [HttpGet("get-user-playlists/{userId}")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetUserPlaylists(string userId)
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
                if(user is not null)
                {
                    var playlists = (await _playlistRepository.GetAllAsync()).Where(p => p.UsersId == user.Id).ToList();
                    if(playlists is not null && playlists.Any())
                    {
                        playlistsMapped = MapToListPlaylistDTO(playlists);
                    }

                    return Ok(playlistsMapped);
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");

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

        [HttpGet("get-all-playlists")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetAllPlaylists()
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer toutes les playlists.");
                var playlistDTOs = Enumerable.Empty<PlaylistDTO>();
                var playlists = await _playlistRepository.GetAllAsync();
                if(playlists.Any())
                {
                    playlistDTOs = MapToListPlaylistDTO(playlists);
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
                if(!string.IsNullOrEmpty(loggedUserId))
                {
                    var playlist = await _playlistRepository.GetAsync(input.PlaylistId);
                    var track = await _trackRepository.GetAsync(input.TrackId);

                    if(playlist is not null && track is not null) 
                    {
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
                        else
                        {
                            return Conflict(); // StatusCode 409
                        }
                    }
                    else
                    {
                        return NotFound();  
                    }
                }
                throw new Exception("L'utilisateur n'a pas été trouvé.");
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
        [HttpGet("get-favorite-playlists/{userId}")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetFavoritePlaylists(string userId)
        {
            try
            {
                _logger.LogInformation("Requete pour récupérer les playlists favoris d'un utilisateur.");
                var user = await _userRepository.GetUserAsync(userId);
                if(user is not null)
                {
                    var faoritePlaylistsMapped = Enumerable.Empty<PlaylistDTO>();
                    var favoritePlaylists = await _likeRepository.GetUserFavoritePlaylists(userId);
                    if(favoritePlaylists.Any())
                    {
                        faoritePlaylistsMapped = MapToListPlaylistDTO(favoritePlaylists);
                    }
                    return Ok(faoritePlaylistsMapped);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
       
    }
}
