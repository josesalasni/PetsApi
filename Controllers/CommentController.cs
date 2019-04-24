using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
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
        
        /*
        [Authorize(Policy = "ApiUser")]
        [HttpGet("api/publication/{id}/comment")]
        public async Task<IActionResult> Get (int id )
        {
            var publication = await _context.Publications.Include(t => t.Comments).FirstOrDefaultAsync(t => t.PublicationId == id);

            if (publication == null)
            {
                return new OkObjectResult("The publication doesn't exist or has been deleted") {StatusCode = (int)HttpStatusCode.NotFound };
            }

            var comments = publication.Comments.Select ( p => new {
                p.CommentId,
                p.DateComment,
                p.Message,
                p.ApplicationUserId
            }).ToList();

            return new OkObjectResult( comments ) {StatusCode = (int)HttpStatusCode.OK };
        }

        */

        [Authorize(Policy = "ApiUser")]
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

        [Authorize(Policy = "ApiUser")]
        [HttpDelete("{id}")]
        public async Task<JsonResult> Delete (int id)
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
            {
                return new JsonResult("Error") { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            var comment = await _context.Comments.FindAsync(id);

            if (comment == null )
            {
                return new JsonResult("Comment Doesn't Exist") { StatusCode = (int)HttpStatusCode.NotFound };
            }
            
            //Delete if the comment is from user, or the comment is from the creator of the publication
            if (comment.ApplicationUser != user || comment.Publication.ApplicationUser != user )
            {
                return new JsonResult("Doesn't have rights for this task") { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return new JsonResult("The comment has been deleted") { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}