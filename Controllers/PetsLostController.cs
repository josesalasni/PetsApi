using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TodoApi.Models;

namespace TodoApi.Controllers 
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class PetsLostController : Controller
    {
        private readonly TodoContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PetsLostController (TodoContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _userManager = UserManager;
        }

        [HttpGet]
        public async Task<OkObjectResult> Get([FromQuery]PagingParameterModel pagingparametermodel)
        {
            //Query 
            var PetsList = await _context.Publications.Select( p => new {
                p.Pictures,
                p.ApplicationUser.FirstName,
                p.ApplicationUser.LastName,
                p.ApplicationUser.PictureUrl,
                p.PublicationId,
                p.TypePublication,
                p.DatePublish,
                p.Description,
                p.Status,
                p.Category.CategoryName
            })
            .Where(t => t.TypePublication.Equals("Desaparecido"))
            .OrderByDescending(t => t.DatePublish).ToListAsync();

            // Get's No of Rows Count and do the paging 
            int count = PetsList.Count();  
        
            // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
            int CurrentPage = pagingparametermodel.pageNumber;  
        
            // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
            int PageSize = pagingparametermodel.pageSize;  
        
            // Display TotalCount to Records to User  
            int TotalCount = count;  
        
            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);  
        
            // Returns List of Customer after applying Paging   
            var items = PetsList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();  
        
            // if CurrentPage is greater than 1 means it has previousPage  
            var previousPage = CurrentPage > 1 ? "Yes" : "No";  
        
            // if TotalPages is greater than CurrentPage means it has nextPage  
            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";  
        
            // Object which we are going to send in header   
            var paginationMetadata = new PaginationHeaders
            {  
                totalCount = TotalCount,  
                pageSize = PageSize,  
                currentPage = CurrentPage,  
                totalPages = TotalPages,  
                previousPage = previousPage,  
                nextPage = nextPage  
            };  
            
            Response.Headers.Add("PagingHeader", JsonConvert.SerializeObject(paginationMetadata) );

            return new OkObjectResult(items) { StatusCode = (int)HttpStatusCode.OK };
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Publication publication )
        {
            //Viewmodel validations
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Get user from token
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            //Creating the entity
            var _publication = new Publication()
            {
                Description = publication.Description,
                DatePublish = DateTime.Now,
                ApplicationUser = user,  
                TypePublication = "Desaparecido",
                Status = false,
            };

            //Finally add
            await _context.Publications.AddAsync(_publication);

            await _context.SaveChangesAsync();

            return new OkObjectResult("The publication has beed Added") { StatusCode = (int)HttpStatusCode.OK };

        }

        [Authorize(Policy= "ApiUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail (int id)
        {
            //Unique public data disponible for the client
            var publication = await _context.Publications.Select( p => new {
                p.Comments,
                p.Pictures,
                p.ApplicationUser.FirstName,
                p.ApplicationUser.LastName,
                p.ApplicationUser.PictureUrl,
                p.PublicationId,
                p.TypePublication,
                p.DatePublish,
                p.Description,
                p.Latitude,
                p.Longitude,
                p.Status,
                p.Category.CategoryName
            }).FirstOrDefaultAsync( t => t.PublicationId == id );

            if (publication == null)
            {
                return new OkObjectResult("Publication doesnt exist or already deleted") { StatusCode = (int)HttpStatusCode.NotFound };
            } 

            else 
            {
                return new OkObjectResult(publication) {StatusCode = (int) HttpStatusCode.OK }  ;
            } 

        }

    }
}