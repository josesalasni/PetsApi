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
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationController : Controller 
    {
        private readonly TodoContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHostingEnvironment _environment;

        public PublicationController(TodoContext context, UserManager<AppUser> UserManager, IHostingEnvironment environment)
        {
            _context = context;
            _userManager = UserManager;
            _environment = environment;
        }

        [Authorize(Policy = "ApiUser")]
        [HttpGet]
        public async Task<JsonResult> Get()
        {
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
            }).ToListAsync();

            if (PetsList == null)
            {
                return new JsonResult( "Not Found" ) { StatusCode = (int)HttpStatusCode.NotFound };
            }

            else 
            {
                return new JsonResult (PetsList) {StatusCode = (int)HttpStatusCode.OK}; 
            }

        }

        [Authorize(Policy = "ApiUser")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Publication publication, List<IFormFile> files )
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

            if (files.Count > 3 )
            {
                return BadRequest("Please upload max 3 photos");
            }

            //Get user from token
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            //First check the photos before upload to verify its not a script or corrupted file
            foreach (var file in files)
            {
                if (CheckPhoto.IsImage(file) == false )
                {
                    return BadRequest("Please upload a valid photo");
                }
            }
 
            //Creating the entity
            var _publication = new Publication()
            {
                Description = publication.Description,
                DatePublish = DateTime.Now,
                ApplicationUser = user,  
                TypePublication = publication.TypePublication,
                Status = false,
            };

            //Finally add
            await _context.Publications.AddAsync(_publication);

            //Uploading if all photos are valid
            foreach (var file in files)
            {
                var Picture = new Models.Picture () 
                {
                    Path = UploadPhoto (file),
                    Publication = _publication
                };

                await _context.Pictures.AddAsync(Picture);
            }            
            
            await _context.SaveChangesAsync();

            return new OkObjectResult("The publication has beed Added") { StatusCode = (int)HttpStatusCode.OK };
        }

        //Method for upload photos 
        private String UploadPhoto(IFormFile file) 
        {
            if (file.Length > 0 )
            {
                try
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\uploads\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\uploads\\");
                    }
                    
                    using (FileStream filestream =  System.IO.File.Create(_environment.WebRootPath + "\\uploads\\" + file.FileName))
                    {
                        file.CopyTo(filestream);
                        filestream.Flush();

                        return file.FileName ;
                    }
                
                
                }

                catch (Exception ex)
                {
                    return ex.ToString();
                }
                
            }

            else
            {
                return "File is corrupted";
            }

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

        [Authorize(Policy = "ApiUser")]
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
                return new OkObjectResult("Your account doesn't have this rights") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            //Finally remove from the database
            _context.Publications.Remove(publication);

            await _context.SaveChangesAsync();

            return new OkObjectResult("Publication Deleted Succesfully") { StatusCode = (int)HttpStatusCode.Unauthorized };
        }
    }
}