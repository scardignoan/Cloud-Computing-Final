using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsProject.PurgeItems
{
    public class PurgeItems
    {
        private readonly IBookRepository _repository;

        public PurgeItems(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("PurgeItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "books")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PurgeItems");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorizedResponse;
            }

            try
            {
                await _repository.PurgeAsync();
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error purging books");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error");
                return errorResponse;
            }
        }
    }
}
