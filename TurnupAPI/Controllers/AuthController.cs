using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Imaging;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Forms;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Cors;

namespace TurnupAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [EnableCors("MyCorsPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IUserEmailStore<Users> _emailStore;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IUserStore<Users> _userStore;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur d'authentification.
        /// </summary>
        public AuthController(UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            IConfiguration configuration,
            HtmlEncoder htmlEncoder,
            IUserStore<Users> userStore,
            IMemoryCache memoryCache

            )

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _htmlEncoder = htmlEncoder;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _memoryCache = memoryCache;

        }
        /// <summary>
        /// Inscrit un nouvel utilisateur.
        /// </summary>
        /// <param name="Input">Les informations d'inscription de l'utilisateur.</param>
        /// <returns>Un résultat HTTP indiquant le succès de l'inscription.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterForm Input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Vérifiez si l'e-mail existe déjà
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);

            if (existingUser != null)
            {
                return Conflict();
            }
            var user = CreateUser();
            user.FirstName = _htmlEncoder.Encode(Input.FirstName!);
            user.LastName = _htmlEncoder.Encode(Input.LastName!);
            user.Gender = _htmlEncoder.Encode(Input.Gender!);
            user.Country = _htmlEncoder.Encode(Input.Country!);
            user.Birthdate = Input.Birthdate;
            if (Input.Picture != null && Input.Picture.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Input.Picture.CopyToAsync(memoryStream);
                    user.Picture = memoryStream.ToArray();
                }
            }
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            if (!string.IsNullOrEmpty(Input.Password))
            {
                await _userManager.CreateAsync(user, Input.Password);

            }
            return Ok(new { Message = "Utilisateur enregistré avec succès." });
        }
        [HttpGet("generate-captcha")]
        public ActionResult<Captcha> GenerateCaptcha()
        {
            string captchaText = GenerateRandomCaptcha();
            // Stockez le texte du captcha dans MemoryCache avec une durée de vie limitée
            _memoryCache.Set("CaptchaText", captchaText, TimeSpan.FromMinutes(5));
            byte[] captchaImageBytes = GenerateCaptchaImageBase64(captchaText);
            Captcha captcha = new()
            {
                Image = captchaImageBytes,
            };
            // Convertissez les données binaires de l'image en chaîne base64
            // string captchaImageBase64 = Convert.ToBase64String(captchaImageBytes);

            // Vous pouvez également renvoyer captchaText si vous devez valider côté client
            return Ok(captcha);
        }

        [HttpPost("validate-captcha")]
        public ActionResult<bool> ValidateCaptcha([FromBody] string input)
        {
            string storedCaptchaText = _memoryCache.Get<string>("CaptchaText");
            bool captchaVerified = string.Equals(input, storedCaptchaText);
            return Ok(captchaVerified);

        }

        /// <summary>
        /// Connecte un utilisateur existant.
        /// </summary>
        /// <param name="model">Les informations de connexion de l'utilisateur.</param>
        /// <returns>Un résultat HTTP indiquant le succès de la connexion.</returns>
    
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginForm model)
        {
            if (!ModelState.IsValid || (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.Password)))
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Email!);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {

                return Ok(new { token = await GenerateToken(user) });
            }
            else
            {
                return Unauthorized();
            }

        }
        /// <summary>
        /// Crée un utilisateur.
        /// </summary>

        private Users CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Users>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Users)}'. " +
                    $"Ensure that '{nameof(Users)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        /// <summary>
        /// Connecte un utilisateur existant.
        /// </summary>

        private IUserEmailStore<Users> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Users>)_userStore;
        }
        /// <summary>
        /// Génère un token jwt.
        /// </summary>
        private async  Task<string> GenerateToken(Users user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email !),
               
            };
            var roles = await _userManager.GetRolesAsync(user);

           
             
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Turnup_JWT_Key")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken
            (
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRandomCaptcha()
        {
            // Générez une chaîne de caractères aléatoires pour le captcha
            // Vous pouvez utiliser des lettres majuscules, minuscules et des chiffres
            // Vous pouvez personnaliser la longueur et les caractères autorisés
            // Par exemple, pour une longueur de 6 caractères :
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var captchaText = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return captchaText;
        }

        private static byte[] GenerateCaptchaImageBase64(string captchaText)
        {
            // Utilisez une bibliothèque de génération d'images (par exemple, System.Drawing.Common)
            // pour créer une image captcha à partir du texte
            using (var image = new Bitmap(200, 50))
            using (var graphics = Graphics.FromImage(image))
            {
                // Personnalisez la génération de l'image, par exemple, définissez la couleur de fond, la police, etc.
                graphics.Clear(Color.DarkOliveGreen);
                using (var font = new Font("Arial", 20))
                using (var brush = new SolidBrush(Color.White))
                {
                    graphics.DrawString(captchaText, font, brush, 10, 10);
                }

                // Convertissez l'image en format base64
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Png);
                    var imageBytes = stream.ToArray();
                    return imageBytes;
                }
            }
        }
    }

    public class Captcha
    {
        public byte[]? Image { get; set; }
    }
}