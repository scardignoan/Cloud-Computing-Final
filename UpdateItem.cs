using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.UpdateItem
{
    public class UpdateItem
    {
        private readonly IBookRepository _repository;

        public UpdateItem(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("UpdateItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "books/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateItem");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorizedResponse;
            }

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updatedBook = JsonSerializer.Deserialize<Book>(requestBody);

                if (updatedBook == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid JSON");
                    return badRequestResponse;
                }

                var success = await _repository.UpdateAsync(id, updatedBook);
                if (success)
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteStringAsync(JsonSerializer.Serialize(updatedBook));
                    return response;
                }

                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Book not found");
                return notFoundResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating book");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error");
                return errorResponse;
            }
        }
    }
}
