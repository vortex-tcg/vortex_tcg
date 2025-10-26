namespace VortexTCG.Common.DTO
{
    public class ResultDTO<T> 
    {
        public bool success { get; set; }
        public int statusCode { get; set; }
        public string? message { get; set; } = null;

        public T? data { get; set; }

    }
}