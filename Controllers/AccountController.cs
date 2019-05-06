using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using petsapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using petsapi.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace petsapi.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly TodoContext _context;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;

        UserManager<AppUser> _userManager;

        public AccountController(TodoContext db, UserManager<AppUser> UserManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _context = db;
            _userManager = UserManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
        }

      
        /* 
        // POST api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AppUser _user)
        {   
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = await GetClaimsIdentity(_user.Email, _user.PasswordHash);
             
            if (identity == null)
            {
                return new OkObjectResult("Account not found") { StatusCode = (int)HttpStatusCode.NotFound };
            }
            
            var jwt = await GenerateEncodedToken.GenerateJwt(identity, _jwtFactory, _user.Email, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }                                

        //POST api/account/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AppUser _user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AppUser Usuario = new AppUser();
            Usuario.Email = _user.Email;
            Usuario.TypeAccount = "Usuario";   
            Usuario.UserName = _user.Email;
  
            var result = await _userManager.CreateAsync(Usuario, _user.PasswordHash);

            if (!result.Succeeded) return new BadRequestObjectResult( "Error 500"  );

            await _context.SaveChangesAsync();
            return new OkObjectResult("Account created");
           
        }

        */
    } 
}