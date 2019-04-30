using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TodoApi.Helpers;
using TodoApi.Models;

namespace TodoApi.Controllers 
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationController : Controller 
    {
        private readonly TodoContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PublicationController(TodoContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _userManager = UserManager;
        }

        [HttpGet]
        public async Task<JsonResult> Get([FromQuery]PagingParameterModel pagingparametermodel)
        {
            //Get type publication and status
            var typeP = pagingparametermodel.TypePublication;
            bool status = pagingparametermodel.Status;
            var typePet = pagingparametermodel.CategoryName;

            //Query with optional parameters in the url
            var PetsList = await _context.Publications.Select( p => new {
                p.Pictures.FirstOrDefault().Path,
                p.ApplicationUser.FirstName,
                p.ApplicationUser.LastName,
                p.ApplicationUser.PictureUrl,
                p.PublicationId,
                p.TypePublication,
                p.DatePublish,
                p.Description,
                p.Status,
                p.Category.CategoryName
            }).Where( t=>  
                (t.TypePublication == typeP || typeP == null)
                && (t.CategoryName.Equals(typePet) || typePet == null)
                && (t.Status == status)
            ).OrderByDescending(t => t.DatePublish)
            .ToListAsync();

            //Return if error
            if (PetsList == null)
            {
                return new JsonResult( "Not Found" ) { StatusCode = (int)HttpStatusCode.NotFound };
            }

            //Do the paging unility
            else 
            {
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
            
                // Returns List of Pets after applying Paging   
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
                
                //Put paging parameters to the header
                Response.Headers.Add("PagingHeader", JsonConvert.SerializeObject(paginationMetadata) );

                //Finally send petlist to the client 
                return new JsonResult (items) {StatusCode = (int)HttpStatusCode.OK}; 
            }

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PublicationHelper publication )
        {
            //Viewmodel validations
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ( publication.TypePublication.Equals("Donación") == false && (publication.TypePublication.Equals("Desaparecido") == false ) )
            {
                return BadRequest("Please choose one of the two options");
            }

            var Category = await _context.Categories.Where ( t=> t.CategoryName.Equals (publication.Category)).FirstOrDefaultAsync();

            if (Category == null)
            {
                return BadRequest("Please select a valid Category");
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
                TypePublication = publication.TypePublication,
                Category = Category,
                Status = false,
            };

            //Finally add
            await _context.Publications.AddAsync(_publication);

            await _context.SaveChangesAsync();

            return new OkObjectResult(_publication.PublicationId) { StatusCode = (int)HttpStatusCode.OK };
        }

        [HttpGet("{id}")]
        public async Task<OkObjectResult> Detail(int id)
        {
            //Query with optional parameters in the url
            var Pet = await _context.Publications.Select( p => new {
                p.Pictures,
                p.ApplicationUser.FirstName,
                p.ApplicationUser.LastName,
                p.ApplicationUser.PictureUrl,
                p.PublicationId,
                p.TypePublication,
                p.Longitude,
                p.Latitude,
                p.DatePublish,
                p.Description,
                p.Status,
                p.Category.CategoryName
            }).FirstOrDefaultAsync (t => t.PublicationId == id);

            //Return if error
            if (Pet == null)
            {
                return new OkObjectResult( "Publication doesn't exist or has been deleted" ) { StatusCode = (int)HttpStatusCode.NotFound };
            }

            return new OkObjectResult (Pet) { StatusCode = (int)HttpStatusCode.OK };
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete (int id) 
        {
            //Get user from token
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            //Check if user is valid, and get publication
            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            var publication = await _context.Publications.FirstOrDefaultAsync(x => x.PublicationId == id);
            
            //Validating if the publication exists and belongs to the user
            if (publication == null)
            {
                return new OkObjectResult("Publication doesnt exist or already deleted") { StatusCode = (int)HttpStatusCode.NotFound };
            }

            if (publication.ApplicationUser != user )
            {
                return new OkObjectResult("Your account doesn't have this rights") { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            //Finally remove from the database
            _context.Publications.Remove(publication);

            await _context.SaveChangesAsync();

            return new OkObjectResult("Publication Deleted Succesfully") { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}