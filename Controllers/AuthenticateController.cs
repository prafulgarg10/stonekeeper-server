using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyFirstServer.Models;

namespace MyFirstServer.Controllers
{
    [Route("/")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthenticateController(IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            var user = await _authService.FindByNameAsync(model.Username);
            if (user != null)
            {
                bool isPasswordMatches = _authService.CheckPassword(user, model.Password);
                if(!isPasswordMatches){
                    return Unauthorized(new Response { Status = "Unauthorized", Message = "Incorrect Password: Please provide correct password and try again." });
                }

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                if(user.Role?.Name!=null){
                        authClaims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized(new Response { Status = "Unauthorized", Message = "User not found. Please signup to login." });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            var userExists = await _authService.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            AppUser user = new AppUser()
            {
                Email = model.Email,
                Username = model.Username,
                IsActive = true,
                RoleId = 2, //staff
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };
            var result = await _authService.CreateAsync(user);
            if (result==null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] Register model)
        {
            if(string.IsNullOrEmpty(model.SecretPassword)){
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Kindly provide secret key" });
            }
            else{
                var secretKeyHash = _configuration["AdminSecretKey:SecretKeyHashed"];
                //Stonekeeper@3103
                if(secretKeyHash!=null){
                    bool isValid = BCrypt.Net.BCrypt.Verify(model.SecretPassword, secretKeyHash);
                    if(!isValid){
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Kindly provide valid secret key" });
                    }
                }
                else{
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Problem adding user. Please try again later." });
                }
            }
            var userExists = await _authService.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
            AppUser user = new AppUser()
            {
                Email = model.Email,
                Username = model.Username,
                IsActive = true,
                RoleId = 1, //admin
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };
            var result = await _authService.CreateAsync(user);
            if (result==null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }
    }
}
