using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Exceptions;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;


namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des utilisateurs.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<UserController> _logger;
        /// <summary>
        /// Initialise une nouvelle instance du contrôleur des utilisateurs.
        /// </summary>
        /// <param name="userRepository">Le repository des utilisateurs.</param>
        public UserController(
            IUserRepository userRepository,
            UserManager<Users> userManager,
         TurnupContext context,
          IMapper mapper,
          ILogger<UserController> logger

            ) : base(userRepository,null,null,context,mapper)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Récupère l'utilisateur connecté.
        /// </summary>
        /// <returns>Une réponse HTTP contenant les informations de l'utilisateur connecté ou une réponse NotFound si l'utilisateur n'est pas trouvé.</returns>
        [HttpGet("get-logged-user")]
        public async Task<ActionResult<UserDTO>> GetLoggedUser()
        {
            _logger.LogInformation("Requete pour récupérer l'utilisateur connecté.");
            try
            {
                var user = await GetLoggedUserAsync();
                var userDTO = _mapper.Map<UserDTO>(user);
                return Ok(userDTO);
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
        /// Récupère un utilisateur .
        /// </summary>
        /// <returns>Une réponse HTTP contenant les informations d'un utilisateur ou une réponse NotFound si l'utilisateur n'est pas trouvé.</returns>
        [HttpGet("get-user/{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(string id)
        {
            _logger.LogInformation("Requete pour récupérer un utilisateur.");
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            try
            {
                var user = await _userRepository.GetUserAsync(id);
                var userDTO = _mapper.Map<UserDTO>(user);
                return Ok(userDTO);
            }
            catch (NotFoundException)
            {
                _logger.LogWarning( "L'utilisateur n'a pas été trouvé.");
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

        [HttpGet("load-logged-user-data")]
        public async Task<ActionResult<UserDataForm>> LoadLoggedUserData()
        {
            _logger.LogInformation("Requete pour récupérer mes données d'un utilisateur.");
            try
            {
                var user = await GetLoggedUserAsync();
                var userDTO = _mapper.Map<UserDataForm>(user);
                return Ok(userDTO);
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
        [HttpPost("update-user-informations")]
        public async Task<ActionResult> UpdateUserInformations([FromBody] UserDataForm input)
        {
            _logger.LogInformation("Requete pour modifier les informations d'un utilisateur.");
            if(!ModelState.IsValid) 
            {
               return BadRequest(ModelState);
            }
           
                try
                {

                    var loggedUser = await GetLoggedUserAsync();
                    if (loggedUser.Country != input.Country)
                    {
                        loggedUser.Country = input.Country;
                    }
                    if (loggedUser.FirstName != input.FirstName)
                    {
                        loggedUser.FirstName = input.FirstName;
                    }
                    if (loggedUser.LastName != input.LastName)
                    {
                        loggedUser.LastName = input.LastName;
                    }
                    if (loggedUser.Birthdate != input.Birthdate)
                    {
                        loggedUser.Birthdate = input.Birthdate;
                    }
                    
                        loggedUser.IsDarkTheme = input.IsDarkTheme;
                    
                     _context.Users.Update(loggedUser);
                    await _context.SaveChangesAsync();
                    return NoContent();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue.");
                    return StatusCode(501);
                }
            
            
        }
        [HttpPost("update-user-password")]
        public async Task<ActionResult> UpdateUserPassword([FromBody] ChangePasswordForm input)
        {
            _logger.LogInformation("Requete pour modifier le mot de passe d'un utilisateur.");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var loggedUser = await GetLoggedUserAsync();
                var changePasswordResult = await _userManager.ChangePasswordAsync(loggedUser, input.OldPassword, input.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return StatusCode(501);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(501);
            }


        }
        [HttpPost("update-user-picture")]
        public async Task<ActionResult> UpdateUserPicture([FromForm] ChangePictureForm input)
        {
            _logger.LogInformation("Requete pour modifier la photo de profil d'un utilisateur.");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var loggedUser = await GetLoggedUserAsync();
                if (input.Picture != null && input.Picture.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await input.Picture.CopyToAsync(memoryStream);
                        loggedUser.Picture = memoryStream.ToArray();
                    }

                }
                _context.Users.Update(loggedUser);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue.");
                return StatusCode(501);
            }


        }
       
    }
}
