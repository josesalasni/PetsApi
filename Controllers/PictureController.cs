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
using TodoApi.Helpers;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [ApiController]
    public class PictureController : Controller
    {
        private readonly TodoContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHostingEnvironment _environment;

        public PictureController(TodoContext context, UserManager<AppUser> UserManager , IHostingEnvironment environment) 
        {
            _context = context;
            _userManager = UserManager;
            _environment = environment;
        }

        [HttpPost("api/publication/{id}/picture")]
        public async Task<IActionResult> Post ( int id, List<IFormFile> files )
        {  

            if (files.Count < 1 || files.Count > 3 )
            {
                return BadRequest("Please upload max 3 photos");
            }

            //First check the photos before upload to verify its not a script or corrupted file
            foreach (var file in files)
            {
                if (CheckPhoto.IsImage(file) == false )
                {
                    return BadRequest("Please upload a valid photo");
                }
            }

            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            //Get the publication father
            var publication = await _context.Publications
            .Select ( t => new {
                t.Pictures,
                t.PublicationId,
                t.ApplicationUser
            })
            .FirstOrDefaultAsync(t => t.PublicationId == id);

            if (publication == null)
            {
                return new OkObjectResult("The publication doesn't exist or has been deleted") {StatusCode = (int)HttpStatusCode.NotFound };
            }

            if (publication.ApplicationUser != user)
            {
                return new OkObjectResult("you not have this rights") {StatusCode = (int)HttpStatusCode.Forbidden };
            }

            if (publication.Pictures.Count >= 3 )
            {
                return BadRequest("The limit of pictures of a publication is 3");
            }

            //Finally Uploading if all photos are valid
            foreach (var file in files)
            {
                var picture = new Picture (); 
                picture.Publication = await _context.Publications.FirstOrDefaultAsync( t => t.PublicationId == id);
   
                string path = await UploadPhoto(file);

                if (path.Equals("Error"))
                {
                    return new OkObjectResult("Error uploading the photo") {StatusCode = (int)HttpStatusCode.InternalServerError };
                } 

                picture.Path = path;

                await _context.Pictures.AddAsync(picture);
                await _context.SaveChangesAsync();
            }            
            
            return new OkObjectResult("The pictures has beed Added") { StatusCode = (int)HttpStatusCode.OK };
        }

        //Method for upload photos 
        private async Task<String> UploadPhoto(IFormFile file) 
        {
            if (file.Length > 0 )
            {
                string fileName = "";

                try
                {
                    var url = _environment.ContentRootPath + "\\wwwroot\\images\\";

                    if (!Directory.Exists(url))
                    {
                        Directory.CreateDirectory(url);
                    }
                    var extension =  "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                    
                    fileName = Guid.NewGuid().ToString() + extension; 

                    //Asegurate that is the unique image
                    while (System.IO.File.Exists (url + fileName) )
                    {
                        fileName = Guid.NewGuid().ToString() + extension; 
                    }
                    
                    var path = Path.Combine(url , fileName);

                    using (FileStream filestream =  System.IO.File.Create(url + fileName) )
                    {
                        await file.CopyToAsync(filestream);
                        filestream.Flush();

                        return fileName ;
                    }
                    
                }

                catch 
                {
                    return "Error";
                }
                
            }

            else
            {
                return "Error";
            }

        }

        
        [HttpDelete("api/picture/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //Query for get picture
            var picture = await _context.Pictures.Select (
                t => new {
                    t.PictureId, 
                    t.Publication,
                    t.Path
                }
            ).FirstOrDefaultAsync( t=> t.PictureId == id);

            if (picture == null)
            {
                return new OkObjectResult("Picture doesn't exist or has been deleted") { StatusCode = (int)HttpStatusCode.NotFound };
            }

            //Get user
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            //Check if the publication belongs to the user
            if (picture.Publication.ApplicationUser != user)
            {
                return new OkObjectResult("you not have this rights") {StatusCode = (int)HttpStatusCode.Forbidden};
            }

            //Finally remove
            try 
            {
                _context.Pictures.Remove( await _context.Pictures.FirstOrDefaultAsync(t => t.PictureId == id)  );

                await _context.SaveChangesAsync();

                var url = _environment.ContentRootPath + "\\wwwroot\\images\\";
                if (System.IO.File.Exists (url + picture.Path) )
                {
                    System.IO.File.Delete(url + picture.Path); 
                }
            }
            catch 
            {
                return new OkObjectResult("Error deleting photo") {StatusCode = (int)HttpStatusCode.InternalServerError};
            }
           

            return new OkObjectResult("The picture has been deleted") { StatusCode = (int)HttpStatusCode.OK };
        }

    }

}