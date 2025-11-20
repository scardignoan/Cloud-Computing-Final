using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsProject.DeleteItem
{
    public class DeleteItem
    {
        private readonly IBookRepository _repository;

        public DeleteItem(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("DeleteItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "books/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("DeleteItem");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorizedResponse;
            }

            try
            {
                var deleted = await _repository.DeleteAsync(id);
                if (deleted)
                {
                    return req.CreateResponse(HttpStatusCode.NoContent);
                }

                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Book not found");
                return notFoundResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting book");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error");
                return errorResponse;
            }
        }
    }
}
