namespace Api.Models
{
    public class GenericApiResponse<T>
    {

        public bool IsSuccess { get; private set; }
        public int HttpCode { get; private set; }
        public string error { get; private set; }

        public T  Data { get; private set; }
        
    
     public static GenericApiResponse<T> Success(T data, int httpcode)
        {
            return new GenericApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                HttpCode = httpcode
            };
        }

        public static GenericApiResponse<T> Failure(string errormessage,int httpcode)
        {
            return new GenericApiResponse<T>
            {
                IsSuccess = false,
                error=errormessage,
                HttpCode = httpcode
            };
        }
    
}
}