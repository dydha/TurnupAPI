﻿using AutoMapper;
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
         IMapper mapper
            ) : base(userRepository, null, trackRepository,context, mapper)
        { 
            _playlistRepository = playlistRepository;
            _likeRepository = likeRepository;
            _htmlEncoder = htmlEncoder;
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
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    var playlist = await _playlistRepository.GetAsync(id);
                    if (playlist != null && playlist.IsPrivate && playlist.UsersId != loggedUser.Id)
                    {
                        return Unauthorized(); //StatusCode 401;
                    }
                    var playlistDTO = playlist != null ? _mapper.Map<PlaylistDTO>(playlist) :  throw new NotFoundException();
                    return Ok(playlistDTO); 
                }
                catch (NotFoundException)
                {
                    return StatusCode(500, $"Internal Server Error");
                }

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
                input.Name =_htmlEncoder.Encode(input.Name!);
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    var existingPlaylist = await _playlistRepository.GetFilteredPlaylistAsync(p => ((!string.IsNullOrEmpty(p.Name) && input.Name.ToLower().Equals(p.Name.ToLower())) && p.UsersId == loggedUser.Id));
                  
                     throw new DuplicateException();
                }
                catch (NotFoundException)
                {
                    try
                    {
                        var playlist = MapToPlaylist(input, loggedUser.Id);
                        Console.WriteLine(playlist.Name);
                        await _playlistRepository.AddAsync(playlist);
                        return NoContent();
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal Server Error: {ex.Message}");
                    }
                }
                catch(DuplicateException)
                {
                    return Conflict(); //StatusCode 409;
                }
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
                playlistDTO.Name= _htmlEncoder.Encode(playlistDTO.Name!);
                var loggedUser = await GetLoggedUserAsync();
                if(loggedUser.Id == playlistDTO.OwnerId)
                {
                    try
                    {
                        var playlist = await _playlistRepository.GetAsync(playlistDTO.Id);
                        playlist.Name = playlistDTO.Name;
                        playlist.IsPrivate = playlistDTO.IsPrivate;
                        await _playlistRepository.UpdateAsync(playlist);
                        return NoContent();
                    }
                    catch (NotFoundException)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return Unauthorized(); //StatusCode 501
                }

                
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
        /// Supprime  une playlist.
        /// </summary>
        /// <param name="id">l'id de la playlist.</param>
        /// <returns>Le résultat de l'opération.</returns>
        [HttpDelete("delete-playlist/{id:int}")]
        public async Task<ActionResult> DeletePlaylist(int id)
        {
            try
            {
                var loggedUser = await GetLoggedUserAsync();
                try
                {
                    var playlist = await _playlistRepository.GetAsync(id);
                    if(playlist.UsersId == loggedUser.Id)
                    {
                        await _playlistRepository.DeleteAsync(id);
                        return NoContent();
                    }
                    else
                    {
                        return StatusCode(401);
                    }
                    
                }
                catch (NotFoundException)
                {
                    return NotFound(); // Return a 404 if the form is not found
                }

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
        /// Retourne les playlists de l'utilisateur connecté.
        /// </summary>
        /// <returns>Une liste de playlist.</returns>
        [HttpGet("get-user-playlists/{userId}")]
        public async Task<ActionResult<List<PlaylistDTO>>> GetUserPlaylists(string userId)
        {
            if(string.IsNullOrEmpty(userId))
                    return BadRequest();
            try
            {
                var user = await _userRepository.GetUserAsync(userId);
                try
                {
                    var playlists = (await _playlistRepository.GetAllAsync()).Where(p => p.UsersId == user.Id).ToList();
                    return Ok(MapToListPlaylistDTO(playlists));
                }
                catch (EmptyListException)
                {
                    return NoContent(); // Return a 204
                }
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
        /// Retourne toutes les playlists.
        /// </summary>
        /// <returns>Toutes les playlists.</returns>

        [HttpGet("get-all-playlists")]
        public async Task<ActionResult<List<PlaylistDTO>>> GetAllPlaylists()
        {
            try
            {
                var playlists = await _playlistRepository.GetAllAsync();
                var playlistDTOs = MapToListPlaylistDTO(playlists);
                return Ok(playlistDTOs);

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
                var loggedUser = await GetLoggedUserAsync(); // Je récupère l'utilisateur connecté
                try
                {
                    //Je récpère la playlist
                    var playlist = await _playlistRepository.GetAsync(input.PlaylistId);

                    try
                    {
                        var track = await _trackRepository.GetAsync(input.TrackId);
                        try
                        {
                            var existingTrackInPlaylist = await _context.PlaylistTrack.Where(pt => pt.PlaylistId == playlist.Id && pt.TrackId == track.Id).FirstOrDefaultAsync();
                            if (existingTrackInPlaylist == null)
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
                                throw new DuplicateException();
                            }
                           
                        }
                        catch (DuplicateException)
                        {
                            return Conflict(); // StatusCode 409
                        }

                    }
                    catch (NotFoundException)
                    {
                        return NotFound();
                    }
                }
                catch (NotFoundException)
                {
                    return NotFound();
                }
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
        /// Retourne les playlist favoris d'un utilisateur.
        /// </summary>
        [HttpGet("get-favorite-playlists/{userId}")]
        public async Task<ActionResult<List<PlaylistDTO>>> GetFavoritePlaylists(string userId)
        {
            try
            {
                var favoritePlaylists = await _likeRepository.GetUserFavoritePlaylists(userId);
               
                    return Ok(MapToListPlaylistDTO(favoritePlaylists));
                

            }
            catch (EmptyListException)
            {
                return NoContent() ; //StatusCode 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
