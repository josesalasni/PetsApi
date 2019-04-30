using System.ComponentModel.DataAnnotations;

namespace TodoApi.Helpers 
{
    public class PublicationHelper 
    {
        [Required]
        public string Description {get; set;}

        public string Category {get; set;}

        [Required]
        public string TypePublication {get; set;}

    }
}