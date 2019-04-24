using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TodoApi.Models;

namespace TodoApi.Helpers
{
    public class GenerateEncodedToken 
    {
        public static async Task<JObject> GenerateJwt(ClaimsIdentity identity, IJwtFactory jwtFactory,string userName, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings)
        {
            var response = new FacebookResponse()
            {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = await jwtFactory.GenerateEncodedToken(userName, identity),
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds
            };

            
            var res = JObject.FromObject(response);

            //return JsonConvert.SerializeObject(response, serializerSettings);
            return res;
        }
    }
}

