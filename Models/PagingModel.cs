using Newtonsoft.Json;

namespace TodoApi.Models
{
    public class PagingParameterModel  
    {  
        public string TypePublication {get; set;} 

        public bool Status {get; set;} = false;

        public string CategoryName {get; set;} 

        const int maxPageSize = 20;  
  
        public int pageNumber { get; set; } = 1;  
  
        public int _pageSize { get; set; } = 10;  
  
        public int pageSize  
        {  
  
            get { return _pageSize; }  
            set  
            {  
                _pageSize = (value > maxPageSize) ? maxPageSize : value;  
            }  
        }  
    }  

    public class PaginationHeaders
    {
        [JsonProperty("totalCount")]
        public int totalCount {get; set;}

        [JsonProperty("pageSize")]
        public int pageSize {get; set;}  

        [JsonProperty("currentPage")]
        public int currentPage {get; set;}  

        [JsonProperty("totalPages")]
        public int totalPages {get; set;}  

        [JsonProperty("previousPage")]
        public string previousPage {get; set;}  

        [JsonProperty("nextPage")]
        public string nextPage {get; set;}
    }
}