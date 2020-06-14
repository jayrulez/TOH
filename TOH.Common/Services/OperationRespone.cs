using System.Runtime.Serialization;

namespace TOH.Common.Services
{
    [DataContract]
    public class OperationError
    {
        [DataMember(Order = 1)]
        public string ErrorCode { get; set; }

        [DataMember(Order = 2)]
        public string ErrorMessage { get; set; }

        public OperationError()
        {
            ErrorCode = string.Empty;
            ErrorMessage = "An Undefined Error Occured";
        }

        public OperationError(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode ?? string.Empty;
            ErrorMessage = errorMessage;
        }

        public OperationError(string errorMessage)
        {
            ErrorCode = string.Empty;
            ErrorMessage = errorMessage;
        }
    }

    [DataContract]
    public class OperationResponse<TOperationResponse> where TOperationResponse : class, new()
    {
        [DataMember(Order = 1)]
        public TOperationResponse Data { get; set; }

        [DataMember(Order = 2)]
        public OperationError Error { get; set; }

        public bool IsSuccessful { get { return Error == null; } }

        public OperationResponse()
        {
            Data = default;
            Error = null;
        }

        public OperationResponse(TOperationResponse data)
        {
            Data = data;
            Error = null;
        }

        public OperationResponse(OperationError error)
        {
            Data = default;
            Error = error;
        }

        public OperationResponse<TOperationResponse> Succeed(TOperationResponse data)
        {
            Data = data;
            Error = null;

            return this;
        }

        public OperationResponse<TOperationResponse> Fail(OperationError error)
        {
            Data = default;
            Error = error;

            return this;
        }

        public OperationResponse<TOperationResponse> Fail(string errorCode, string errorMessage)
        {
            Data = default;
            Error = new OperationError(errorCode, errorMessage);

            return this;
        }

        public OperationResponse<TOperationResponse> Fail(string errorMessage)
        {
            Data = default;
            Error = new OperationError(errorMessage);

            return this;
        }
    }
}
