namespace ApiApp.Models
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public List<string>? Errors { get; set; }
        public T Data { get; set; }
    }
}
