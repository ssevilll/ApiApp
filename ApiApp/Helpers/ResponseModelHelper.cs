using ApiApp.Models;

namespace ApiApp.Helpers
{
    public class ResponseModelHelper
    {
        public static ResponseModel<T> CreateSuccessResponse<T>(T data)
        {
            return new ResponseModel<T>
            {
                Success = true,
                StatusCode = 200,
                Errors = null,
                Data = data
            };
        }

        public static ResponseModel<T> CreateBadRequestResponse<T>(string error)
        {
            return new ResponseModel<T>
            {
                Success = false,
                StatusCode = 400,
                Errors = new List<string> { error },
                Data = default
            };
        }

        public static ResponseModel<T> CreateNotFoundResponse<T>(string error)
        {
            return new ResponseModel<T>
            {
                Success = false,
                StatusCode = 404,
                Errors = new List<string> { error },
                Data = default
            };
        }

        public static ResponseModel<T> CreateConflictResponse<T>(string error)
        {
            return new ResponseModel<T>
            {
                Success = false,
                StatusCode = 409,
                Errors = new List<string> { error },
                Data = default
            };
        }

        public static ResponseModel<T> CreateErrorResponse<T>(List<string> errors)
        {
            return new ResponseModel<T>
            {
                Success = false,
                StatusCode = 500,
                Errors = errors,
                Data = default
            };
        }
    }
}
