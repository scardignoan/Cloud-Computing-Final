using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.ValidateBooks
{
    public class ValidateBooks
    {
        private readonly IBookRepository _repository;

        public ValidateBooks(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("ValidateBooks")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "books/validate")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ValidateBooks");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorized;
            }

            try
            {
                var updatedCount = await _repository.ValidateAndArchiveOldBooksAsync();
                var payload = new
                {
                    updatedCount,
                    timestamp = DateTime.UtcNow
                };

                logger.LogInformation("ValidationCompleted {@Payload}", payload);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(JsonSerializer.Serialize(payload));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running validation");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error");
                return errorResponse;
            }
        }
    }
}
