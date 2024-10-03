using System.Net.Http.Json;

namespace LinkedinPostCarousel;

public enum PostCreationStatus
{
    DRAFT,
    PUBLISHED,
}

public class LinkedinApi
{
    private string _token;
    private HttpClient _client;

    public LinkedinApi(string token)
    {
        _token = token;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://api.linkedin.com/");
        _client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");
        _client.DefaultRequestHeaders.Add("LinkedIn-Version", "202409");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
    }

    public async Task<UserInfo> GetUserInfo(CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync("v2/userinfo", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
    }
    
    public async Task<string> UploadDocument(string userUrn, Stream document, CancellationToken cancellationToken)
    {
        // Get url to upload document
        var startUploadRequest = new
        {
            initializeUploadRequest = new
            {
                owner = userUrn
            }
        };
        
        var startUploadResponse = await _client.PostAsJsonAsync("rest/documents?action=initializeUpload", startUploadRequest, cancellationToken);
        startUploadResponse.EnsureSuccessStatusCode();
        var startUploadResponseContent = await startUploadResponse.Content.ReadFromJsonAsync<UploadDocumentResponse>(cancellationToken);
        
        // Upload file
        var uploadResponse = await _client.PostAsync(startUploadResponseContent.value.UploadUrl,
            new StreamContent(document), cancellationToken);
        uploadResponse.EnsureSuccessStatusCode();
        
        return startUploadResponseContent.value.Document;
    }

    public async Task CreateCarousel(string userUrn, string title, string documentTitle, string documentId, PostCreationStatus status, CancellationToken cancellationToken)
    {
        var requestJson = new
        {
            author = userUrn,
            commentary = title,
            visibility = "PUBLIC",
            distribution = new
            {
                feedDistribution = "MAIN_FEED",
                targetEntities = new string[] { },
                thirdPartyDistributionChannels = new string[] { }
            },
            content = new
            {
                media = new
                {
                    title = documentTitle,
                    id = documentId
                }
            },
            lifecycleState = status.ToString(),
            isReshareDisabledByAuthor = false
        };


        var response = await _client.PostAsJsonAsync("rest/posts", requestJson, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    record UploadDocumentResponse(UploadDocumentResponseValue value);

    record UploadDocumentResponseValue(string UploadUrl, string Document);
    
    public record UserInfo(string sub, string email, string name, bool email_verified, string given_name, string family_name);
}