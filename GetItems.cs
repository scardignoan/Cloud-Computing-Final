using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.GetItems
{
    public class GetItems
    {
        private readonly IBookRepository _repository;

        public GetItems(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("GetItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetItems");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorizedResponse;
            }

            try
            {
                var books = await _repository.GetAllAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(JsonSerializer.Serialize(books, options));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving books");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error");
                return errorResponse;
            }
        }
    }
}
