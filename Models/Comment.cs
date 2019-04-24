using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models 
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
        public string ApplicationUserId { get; set; }
        public AppUser ApplicationUser { get; set; }
    }
}