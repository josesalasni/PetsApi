using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petsapi.Models 
{
    public class Comment
    {
        [Key]
        public int CommentId {get; set;}

        [Required]
        public string Message {get; set;}

        public DateTime? DateComment {get; set;}


        //The Comments from
        public virtual Publication Publication {get; set;}

        //Identity relation
        public virtual string ApplicationUserId { get; set; }
        public virtual AppUser ApplicationUser { get; set; }
    }
}