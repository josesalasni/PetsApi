using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models 
{
    public class Publication
    {
        public Publication ()
        {
            this.Comments = new HashSet<Comment>();
            this.Pictures = new HashSet<Picture>();
        }

        [Key]
        public int PublicationId {get; set;}

        [Required]
        public string Description {get; set;}

        [Column(TypeName = "decimal(9, 6)") ]
        public decimal Latitude {get; set;}

        [Column(TypeName = "decimal(9, 6)") ]
        public decimal Longitude {get; set;}

        //Desaparecido o Donacion
        public string TypePublication {get; set;}

        //still Searching, or already found (finished)
        [Required]
        public bool Status {get; set;}

        public DateTime? DatePublish {get; set;}

        //Relationships
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Picture> Pictures { get; set; }

        //Animal Category
        public virtual Category Category {get; set;}

        //Identity relation
        public string ApplicationUserId { get; set; }
        public AppUser ApplicationUser { get; set; }
        
    }
}