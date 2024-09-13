namespace Blog.Core.Models
{
    public class PagingResult<T> : PagingResultBase where T : class
    {
        public List<T> Results { get; set; }
        public PagingResult()
        {
            Results = new List<T>();
        }
    }
}
