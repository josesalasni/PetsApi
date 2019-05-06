using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace petsapi.Models 
{
    public class Category
    {
        public Category ()
        {
            this.Publications = new HashSet<Publication>();
        }

        [Key]
        public int CategoryId {get; set;}

        public string CategoryName {get; set;}
    
        public virtual ICollection <Publication> Publications {get; set;}
    }
}