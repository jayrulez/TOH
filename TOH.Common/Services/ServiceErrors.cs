namespace TOH.Common.Services
{
    public class ServiceErrors
    {
        public static ServiceError UnauthorizedRequestError = new ServiceError("UnauthorizedRequest", "The request is not authorized.");
        public static ServiceError UnexpectedError = new ServiceError("Unexpected", "An unexpected error has occured on the server.");
        public static ServiceError PlayerExistError = new ServiceError("PlayerExist", "A player with the username already exist.");
        public static ServiceError PlayerNotFoundError = new ServiceError("PlayerNotFound", "No player was found with the Username or Id specified.");
        public static ServiceError PlayerSessionNotFoundError = new ServiceError("PlayerSessionNotFound", "No player session was found with the Id specified.");
        public static ServiceError PlayerUnitNotFoundError = new ServiceError("PlayerUnitNotFound", "No player unit was found with the Id specified.");
    }
}