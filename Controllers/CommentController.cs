using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petsapi.Models;

namespace petsapi.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [ApiController]
    public class CommentController : Controller 
    {
        private readonly TodoContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CommentController( TodoContext context, UserManager<AppUser> UserManager )
        {
            _context = context;
            _userManager = UserManager;
        }

        [HttpGet("api/publication/{id}/comment") ]
        public async Task<IActionResult> Get (int id )
        {
            var publication = await _context.Publications.FindAsync(id);

            if (publication == null)
            {
                return new OkObjectResult("The publication doesn't exist or has been deleted") {StatusCode = (int)HttpStatusCode.NotFound };
            }

            var comments = await _context.Comments
            .Where (t => t.Publication == publication)
            .Select(c => new {
                c.ApplicationUser.FirstName,
                c.ApplicationUser.LastName,
                c.ApplicationUser.PictureUrl,
                c.DateComment,
                c.CommentId,
                c.Message
            }).ToListAsync();

            return new OkObjectResult (comments) {StatusCode = (int)HttpStatusCode.OK };
        }
        
        
        [HttpPost("api/publication/{id}/comment")]
        public async Task<IActionResult> Create (int id, [FromBody] Comment comment )
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            var publication = await _context.Publications.FindAsync(id);

            if (publication == null)
            {
                return new OkObjectResult("The publication doesn't exist or has been deleted") {StatusCode = (int)HttpStatusCode.NotFound };
            }
            
            var _comment = new Comment()
            {
                DateComment = DateTime.Now,
                Message = comment.Message,
                Publication = publication,
                ApplicationUser = user
            };

            publication.Comments.Add(_comment);

            await _context.SaveChangesAsync();

            return new OkObjectResult("The comment has been added") {StatusCode = (int)HttpStatusCode.OK };
        }

        [HttpDelete("api/publication/{idpub}/comment/{idcom}")]
        public async Task<OkObjectResult> Delete (int idpub, int idcom)
        {
            //Check users
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            //Get required data
            var publication = await _context.Publications.Select ( p => new {
                p.PublicationId,
                p.ApplicationUserId
            }).FirstOrDefaultAsync( t => t.PublicationId == idpub);

            var comment = await _context.Comments.Select (p => new {
                p.CommentId,
                p.Publication.PublicationId,
                p.Publication.ApplicationUserId
            })
            .FirstOrDefaultAsync(t => t.CommentId == idcom);

            //Validations
            if (comment == null )
            {
                return new OkObjectResult("Comment Doesn't Exist") { StatusCode = (int)HttpStatusCode.NotFound };
            }

            if (publication == null)
            {
                return new OkObjectResult ("Publication doesnt exist or have been deleted") { StatusCode = (int)HttpStatusCode.NotFound };
            }

            if (comment.PublicationId != publication.PublicationId)
            {
                return new OkObjectResult ("This comentary doesn't belong to the post sent") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            
            //Delete if the comment is from user, or the comment is from the creator of the publication
            if (comment.ApplicationUserId != user.Id || publication.ApplicationUserId != user.Id )
            {
                return new OkObjectResult("Doesn't have rights for this task") { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            //Finally remove
            _context.Comments.Remove( await _context.Comments.FirstOrDefaultAsync( t => t.CommentId == idcom)  );

            await _context.SaveChangesAsync();

            return new OkObjectResult("The comment has been deleted") { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}