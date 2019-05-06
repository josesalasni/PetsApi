using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace petsapi.Models 
{
    public class AppUser : IdentityUser
    {
        //Data Provided by Facebook Login
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? FacebookId { get; set; }
        public string PictureUrl { get; set; }

        //Normal user or organization
        public string TypeAccount {get ; set;}

        //Relationhips
        public virtual ICollection<Comment> Comments {get; set;}
        public virtual ICollection<Publication> PetLosts {get; set;}
    }
}
