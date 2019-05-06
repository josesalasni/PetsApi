using System.ComponentModel.DataAnnotations;

namespace petsapi.Models 
{
    public class Picture 
    {
        [Key]
        public int PictureId {get; set;}

        public string Path {get; set;}

    	public virtual Publication Publication {get; set;}

    }
}