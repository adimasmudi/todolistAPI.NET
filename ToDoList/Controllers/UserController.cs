using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ToDoList.Models;
using ToDoList.Models.Authentication;
using ToDoList.Responses;

namespace ToDoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;

        public UserController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            _configuration = configuration;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExist = await userManager.FindByNameAsync(model.UserName);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { Status = "Error", Message = "Register Failed" });
            }
            ApplicationUser user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { Status = "Error", Message = "Register Failed" });
            }

            return Ok(new AuthResponse { Status = "Success", Message = "Register User Success" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var userLogin = await userManager.FindByEmailAsync(model.Email);

                if (userLogin != null && await userManager.CheckPasswordAsync(userLogin, model.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(userLogin);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, userLogin.Email),
                        new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim("email",userLogin.Email),
                        new Claim("id", userLogin.Id)
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole.ToString()));
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:ValidIssuer"],
                        audience: _configuration["Jwt:ValidAudience"],
                        claims: authClaims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: signIn);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        user = userLogin.Email
                    });

                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { Status = "Error", Message = ex.Message });

            }
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse { Status = "Error", Message = "Login Failed" });
        }

        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> Profile()
        {
            var currentUser = HttpContext.User.Identity as ClaimsIdentity;
            if (currentUser != null)
            {
                var userClaims = currentUser.Claims;
                var users = new List<string>();

                foreach (var claim in userClaims)
                {
                    users.Add(claim.Value);
                }

                var authenticatedUser = await userManager.FindByEmailAsync(users[0]);

                return Ok(new
                {
                    id = authenticatedUser.Id,
                    email = authenticatedUser.Email,
                    username = authenticatedUser.UserName,
                });

            }
            return StatusCode(StatusCodes.Status401Unauthorized, new AuthResponse { Status = "Error", Message = "Unauthorized" });
        }


    }


}
