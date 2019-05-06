using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using petsapi.Helpers;
using petsapi.Models;

namespace petsapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuthController : Controller
    {
        private readonly TodoContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly FacebookAuthSettings _fbAuthSettings;

        private static readonly HttpClient Client = new HttpClient();

        public ExternalAuthController(FacebookAuthSettings fbAuthSettingsAccessor,TodoContext context, UserManager<AppUser> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _context = context;
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _fbAuthSettings = fbAuthSettingsAccessor;
        }

        [HttpPost("facebook") ]
        public async Task<IActionResult> Facebook([FromBody]FacebookAuthModel model)
        {
            if (model.AccessToken == null)
            {
                return BadRequest("Please login using the facebook button");
            }

            // 1.generate an app access token
            var appAccessTokenResponse = await Client.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_fbAuthSettings.AppId}&client_secret={_fbAuthSettings.AppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await Client.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                return new BadRequestObjectResult( "Error 404 - Invalid User Token");
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await Client.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,name,gender,locale,birthday,picture&access_token={model.AccessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                var appUser = new AppUser
                {
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    FacebookId = userInfo.Id,
                    Email = userInfo.Email,
                    UserName = userInfo.Email,
                    PictureUrl = userInfo.Picture.Data.Url,
                    TypeAccount = "User"
                };

                var result = await _userManager.CreateAsync(appUser, "Giratina97" );

                if (!result.Succeeded) return new  BadRequestObjectResult( "Error 500");

                await _context.SaveChangesAsync();
            }

            // finally generate the jwt for the user client
            var localUser = await _userManager.FindByEmailAsync(userInfo.Email);

            if (localUser==null)
            {
                return new  BadRequestObjectResult( "Error 404");
            }

            var jwt = await GenerateEncodedToken.GenerateJwt(_jwtFactory.GenerateClaimsIdentity(localUser.UserName, localUser.Id), _jwtFactory, localUser.UserName, _jwtOptions, new JsonSerializerSettings {Formatting = Formatting.Indented});

            return new OkObjectResult(jwt);
        }
    }
}


