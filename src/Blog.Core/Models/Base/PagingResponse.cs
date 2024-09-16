namespace Blog.Core.Models.Base
{
    public class PagingResponse<T> : PagingBaseResponse where T : class
    {
        public List<T> Results { get; set; }
        public PagingResponse()
        {
            Results = new List<T>();
        }
    }
}
