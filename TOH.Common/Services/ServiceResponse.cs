using System.Runtime.Serialization;

namespace TOH.Common.Services
{
    [DataContract]
    public class ServiceError
    {
        [DataMember(Order = 1)]
        public string ErrorCode { get; set; }

        [DataMember(Order = 2)]
        public string ErrorMessage { get; set; }

        public ServiceError()
        {
            ErrorCode = string.Empty;
            ErrorMessage = "An Undefined Error Occured";
        }

        public ServiceError(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode ?? string.Empty;
            ErrorMessage = errorMessage;
        }

        public ServiceError(string errorMessage)
        {
            ErrorCode = string.Empty;
            ErrorMessage = errorMessage;
        }
    }

    [DataContract]
    public class ServiceResponse<TData> where TData : class, new()
    {
        [DataMember(Order = 1)]
        public TData Data { get; set; }

        [DataMember(Order = 2)]
        public ServiceError Error { get; set; }

        public bool IsSuccessful { get { return Error == null; } }

        public ServiceResponse()
        {
            Data = default;
            Error = null;
        }

        public ServiceResponse(TData data)
        {
            Data = data;
            Error = null;
        }

        public ServiceResponse(ServiceError error)
        {
            Data = default;
            Error = error;
        }

        public ServiceResponse<TData> Succeed(TData data)
        {
            Data = data;
            Error = null;

            return this;
        }

        public ServiceResponse<TData> Fail(ServiceError error)
        {
            Data = default;
            Error = error;

            return this;
        }

        public ServiceResponse<TData> Fail(string errorCode, string errorMessage)
        {
            Data = default;
            Error = new ServiceError(errorCode, errorMessage);

            return this;
        }

        public ServiceResponse<TData> Fail(string errorMessage)
        {
            Data = default;
            Error = new ServiceError(errorMessage);

            return this;
        }
    }
}