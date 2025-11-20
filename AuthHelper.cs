using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker.Http;

public static class AuthHelper
{
    private static SecretClient? _secretClient;
    private static string? _cachedApiKey;
    private static DateTimeOffset _cacheExpiration;

    public static void Initialize(string? keyVaultName, string? apiKeySecretName)
    {
        if (string.IsNullOrWhiteSpace(keyVaultName) || string.IsNullOrWhiteSpace(apiKeySecretName))
        {
            _secretClient = null;
            return;
        }

        var vaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        _secretClient = new SecretClient(vaultUri, new DefaultAzureCredential());
        _cachedApiKey = null;
        _cacheExpiration = DateTimeOffset.MinValue;
        ApiKeySecretName = apiKeySecretName;
    }

    private static string? ApiKeySecretName { get; set; }

    public static bool IsValidApiKey(HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("X-API-Key", out var apiKeys))
            return false;

        var apiKey = apiKeys.FirstOrDefault();
        var validKey = GetApiKey();

        return !string.IsNullOrWhiteSpace(apiKey) && apiKey == validKey;
    }

    private static string? GetApiKey()
    {
        // Prefer Key Vault if configured
        if (_secretClient != null && !string.IsNullOrEmpty(ApiKeySecretName))
        {
            if (_cachedApiKey == null || DateTimeOffset.UtcNow > _cacheExpiration)
            {
                var secret = _secretClient.GetSecret(ApiKeySecretName);
                _cachedApiKey = secret.Value.Value;
                _cacheExpiration = DateTimeOffset.UtcNow.AddMinutes(15);
            }

            return _cachedApiKey;
        }

        // Fallback for local dev
        return Environment.GetEnvironmentVariable("API_KEY");
    }

    public static string? GetSecret(string? secretName)
    {
        if (_secretClient != null && !string.IsNullOrWhiteSpace(secretName))
        {
            var secret = _secretClient.GetSecret(secretName);
            return secret.Value.Value;
        }

        return Environment.GetEnvironmentVariable(secretName ?? string.Empty);
    }
}
