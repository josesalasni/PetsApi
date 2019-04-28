using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers 
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : Controller
    {
        private readonly TodoContext _context;

        public MapController (TodoContext context)
        {
            _context = context;
        }

        public async Task<OkObjectResult> Get()
        {
            //Query for get only the points to show in the map
            var PetsList = await _context.Publications.Select (p => new {
                p.PublicationId,
                p.Latitude,
                p.Status,
                p.Longitude,
            }).Where( t=> 
                t.Latitude != 0 &&
                t.Longitude!= 0 &&
                t.Status == false 
            ).ToListAsync();

            return new OkObjectResult (PetsList) {StatusCode = (int)HttpStatusCode.OK}; 
        }
    }
}