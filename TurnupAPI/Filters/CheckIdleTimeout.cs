using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Interfaces;

namespace TurnupAPI.Filters
{
    public class CheckIdleTimeout : IActionFilter
    {

        private readonly IUserRepository _userRepository;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CheckIdleTimeout(
            IUserRepository userRepository,
            IDistributedCache cache, 
            IHttpContextAccessor httpContextAccessor

            )
        {  
            _userRepository = userRepository;
            _cache = cache;  
            _httpContextAccessor = httpContextAccessor;
        }

        private  async Task<Users?> GetLoggedUser()
        {
              var principal = _httpContextAccessor.HttpContext?.User;
            Users? loggedUser = null;
               var email = principal?.Identity?.Name;
                if (!string.IsNullOrEmpty(email))
                {
                   Console.WriteLine(email);
                   loggedUser = await _userRepository.GetLoggedUserAsync(email);
                }
            
            return loggedUser;
        }
        public   void OnActionExecuting(ActionExecutingContext context)
        {
           /*
            var user = await GetLoggedUser();
            if(user is not null)
            {
                Console.WriteLine($"UTILISATEUR A DECONNECTER {user.LastName}");
            }
           */
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Code à exécuter après l'action du contrôleur
            // Vous pouvez accéder à la demande, la réponse, le résultat, etc.
        }

    }
}
