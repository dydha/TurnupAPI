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
        /// <summary>
        /// Initialise une nouvelle instance du contrôleur des utilisateurs.
        /// </summary>
        /// <param name="userRepository">Le repository des utilisateurs.</param>
        public UserController(
            IUserRepository userRepository,
            UserManager<Users> userManager,
         TurnupContext context,
          IMapper mapper
            ) : base(userRepository,null,null,context,mapper)
        {
            _userManager = userManager;
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
                var user = await GetLoggedUserAsync();
                try
                {
                   
                    var userDTO = _mapper.Map<UserDTO>(user);
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
                    var userDTO = _mapper.Map<UserDTO>(user);
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
                return Ok(_mapper.Map<UserDataForm>(user));
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
                    
                        loggedUser.IsDarkTheme = input.IsDarkTheme;
                    
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
       
        

       
           
        
       
       
    }
}
