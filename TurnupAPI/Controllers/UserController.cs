using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Data;
using TurnupAPI.Exceptions;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des utilisateurs.
    /// </summary>
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly TurnupContext _context;
        /// <summary>
        /// Initialise une nouvelle instance du contrôleur des utilisateurs.
        /// </summary>
        /// <param name="userRepository">Le repository des utilisateurs.</param>
        public UserController(
            IUserRepository userRepository,
            UserManager<Users> userManager,
         SignInManager<Users> signInManager,
         TurnupContext context
            )
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        /// Récupère l'utilisateur connecté.
        /// </summary>
        /// <returns>Une réponse HTTP contenant les informations de l'utilisateur connecté ou une réponse NotFound si l'utilisateur n'est pas trouvé.</returns>
        [HttpGet("get-logged-user")]
        public async Task<ActionResult<UserDTO>> GetLoggedUser()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    return NotFound();
                }
                try
                {
                    var user = await _userRepository.GetLoggedUserAsync(email);
                    var userDTO = new UserDTO()
                    {
                        Id = user.Id,
                        FullName = string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) ? string.Empty : GetFormattedFullName(user.FirstName, user.LastName),
                        Picture = user.Picture,
                        Country = user.Country,
                        Gender = user.Gender,
                        BirthDate = user.Birthdate,
                        IsDarkTheme = user.IsDarkTheme,
                    };
                    return Ok(userDTO);
                }
                catch (NotFoundException)
                {
                    return NotFound();
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
        /// Récupère un utilisateur .
        /// </summary>
        /// <returns>Une réponse HTTP contenant les informations d'un utilisateur ou une réponse NotFound si l'utilisateur n'est pas trouvé.</returns>
        [HttpGet("get-user/{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(string id)
        {
            try
            {
               
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound() ;
                }
                try
                {
                    var user = await _userRepository.GetUserAsync(id);
                    var userDTO = MapToUserDTO(user);
                    return Ok(userDTO);
                }
                catch (NotFoundException)
                {
                    return NotFound();
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

        [HttpGet("load-logged-user-data")]
        public async Task<ActionResult<UserDataForm>> LoadLoggedUserData()
        {
            var user = await GetLoggedUserAsync();
            if(user != null)
            {
                return Ok(MapToUserDataForm(user));
            }
            return NotFound();
        }
        [HttpPost("update-user-informations")]
        public async Task<ActionResult> UpdateUserInformations([FromBody] UserDataForm input)
        {
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
                    
                        loggedUser.IsDarkTheme = input.Theme;
                    
                _context.Users.Update(loggedUser);
                    await _context.SaveChangesAsync();
                    return NoContent();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return StatusCode(501);
                }
            
            
        }
        [HttpPost("update-user-password")]
        public async Task<ActionResult> UpdateUserPassword([FromBody] ChangePasswordForm input)
        {
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
                Console.WriteLine(ex.Message);
                return StatusCode(501);
            }


        }
        [HttpPost("update-user-picture")]
        public async Task<ActionResult> UpdateUserPicture([FromForm] ChangePictureForm input)
        {
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
                Console.WriteLine(ex.Message);
                return StatusCode(501);
            }


        }
       
        private async Task<Users> GetLoggedUserAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                throw new DataAccessException();
            }
            else
            {
                var user = await _userRepository.GetLoggedUserAsync(email);
                return user;
            }
        }

        private static UserDataForm MapToUserDataForm(Users user)
        {
            var userFORM = new UserDataForm()
            {
 
                LastName = user.LastName,
                FirstName = user.FirstName,
                Country = user.Country,
                Gender = user.Gender,
                Birthdate = user.Birthdate,
                Email = user.Email,
                Theme = user.IsDarkTheme
            };
            return userFORM;
           
        }
        /// <summary>
        /// Formate le nom complet en mettant en majuscule la première lettre du prénom et du nom.
        /// </summary>
        /// <param name="firstname">Le prénom de l'utilisateur.</param>
        /// <param name="lastname">Le nom de l'utilisateur.</param>
        /// <returns>Le nom complet formaté.</returns>
        private static string GetFormattedFullName(string firstname, string lastname)
        {
            string fullname = firstname.Substring(0, 1).ToUpper() + firstname.ToLower().Substring(1, firstname.Length - 1) + " " + lastname.Substring(0, 1).ToUpper() + lastname.ToLower().Substring(1, lastname.Length - 1);
            return fullname;
        }
        /// <summary>
        /// Convertit un Users en UserDTO.
        /// </summary>
        /// <param name="user">L'utilisateur.</param>
        /// <returns>Retourne un UserDTO.</returns>
        private static UserDTO MapToUserDTO(Users user) 
        {
            var userDTO = new UserDTO()
            {
                Id = user.Id,
                FullName = string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) ? string.Empty : GetFormattedFullName(user.FirstName, user.LastName),
                Picture = user.Picture,             
                Country = user.Country,
                Gender = user.Gender,
                BirthDate = user.Birthdate,
                IsDarkTheme = user.IsDarkTheme,
            };

            return userDTO;
        }
    }
}
