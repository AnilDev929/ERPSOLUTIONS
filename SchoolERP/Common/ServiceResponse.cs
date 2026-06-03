namespace SchoolERP.Common
{
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static ServiceResponse Ok(string msg)
            => new() { Success = true, Message = msg };

        public static ServiceResponse Fail(string msg)
            => new() { Success = false, Message = msg };
    }


    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ServiceResponse<T> Ok(T data, string msg = "")
            => new() { Success = true, Data = data, Message = msg };

        public static ServiceResponse<T> Fail(string msg)
            => new() { Success = false, Message = msg };
    }
}
