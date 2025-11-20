using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.CreateItem
{
    public class CreateItem
    {
        private readonly IBookRepository _repository;

        public CreateItem(IBookRepository repository)
        {
            _repository = repository;
        }

        [Function("CreateItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateItem");

            if (!AuthHelper.IsValidApiKey(req))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid API key");
                return unauthorizedResponse;
            }

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                logger.LogInformation("Raw JSON received: {Body}", requestBody);

                using var document = JsonDocument.Parse(requestBody);
                var root = document.RootElement;

                var book = new Book
                {
                    Title = root.GetProperty("title").GetString() ?? "",
                    Author = root.GetProperty("author").GetString() ?? "",
                    Isbn = root.GetProperty("isbn").GetString() ?? "",
                    Publisher = root.GetProperty("publisher").GetString() ?? "",
                    Year = root.GetProperty("year").GetInt32(),
                    Description = root.GetProperty("description").GetString() ?? ""
                };

                var createdBook = await _repository.CreateAsync(book);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteStringAsync(JsonSerializer.Serialize(createdBook));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating book");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Internal server error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}
