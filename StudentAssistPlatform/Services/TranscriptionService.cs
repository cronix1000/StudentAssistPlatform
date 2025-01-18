using System.Net.Http.Headers;
using System.Text.Json;

public interface ITranscriptionService
{
    Task<string> TranscribeAudioAsync(byte[] audioBytes, string fileName, string contentType);
}

public class TranscriptionService : ITranscriptionService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public TranscriptionService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        // Or create named clients, etc.
    }

    public async Task<string> TranscribeAudioAsync(byte[] audioBytes, string fileName, string contentType)
    {
        // 1. Get API key
        var openAIApiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(openAIApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        // 2. Prepare request
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", openAIApiKey);

        using var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(audioBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        // e.g., "audio/mpeg", "audio/wav", etc.

        // The field name must be "file" per OpenAI's API requirement
        formData.Add(fileContent, "file", fileName);

        // Required model
        formData.Add(new StringContent("whisper-1"), "model");

        // (Optional) add language, prompt, etc. as needed
        // formData.Add(new StringContent("en"), "language");

        // 3. Send to OpenAI
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", formData);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"OpenAI Whisper request failed: {response.StatusCode}, {responseString}");
        }

        // 4. Parse JSON
        using var jsonDoc = JsonDocument.Parse(responseString);
        var transcription = jsonDoc.RootElement.GetProperty("text").GetString();

        return transcription ?? string.Empty;
    }
}
