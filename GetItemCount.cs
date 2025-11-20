using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.GetItemCount
{
    public class GetItemCount
    {
        private readonly IBookRepository _repository;

        public GetItemCount(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("GetItemCount")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/count")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetItemCount");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorizedResponse;
            }

            try
            {
                var count = await _repository.CountAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(JsonSerializer.Serialize(new { count }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting book count");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error");
                return errorResponse;
            }
        }
    }
}
