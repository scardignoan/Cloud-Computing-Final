using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var keyVaultName = Environment.GetEnvironmentVariable("KeyVaultName");
var apiKeySecretName = Environment.GetEnvironmentVariable("ApiKeySecretName");
AuthHelper.Initialize(keyVaultName, apiKeySecretName);

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        services.AddDbContext<BookDbContext>(options =>
        {
            var sqlSecretName = Environment.GetEnvironmentVariable("SqlConnectionStringSecretName");
            var connectionString = AuthHelper.GetSecret(sqlSecretName)
                ?? throw new InvalidOperationException("SqlConnectionString could not be retrieved from Key Vault.");
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IBookRepository, Repository>();
    })
    .Build();

host.Run();
