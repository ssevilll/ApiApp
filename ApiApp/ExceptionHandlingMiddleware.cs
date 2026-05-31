using ApiApp.Helpers;

namespace ApiApp
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ResponseModelHelper.CreateErrorResponse<string>(new List<string> { "An unexpected error occurred.", exception.Message });
            return context.Response.WriteAsJsonAsync(
                ResponseModelHelper.CreateErrorResponse<string>(new List<string> { "An unexpected error occurred.", exception.Message })
            );
        }
    }
}
