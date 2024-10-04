using LinkedinPostCarousel;

// How to generate a token:
// - Go to https://www.linkedin.com/developers/tools/oauth/token-generator
// - Select the app you want to use
// - Select all scopes
// - Click on "Request access token" and follow all the steps
// - Enjoy!

string token = "<TO-CONFIGURE>";

var api = new LinkedinApi(token);

LinkedinApi.UserInfo userInfo = await api.GetUserInfo(CancellationToken.None);

// Here we post as user. If the user is admin of an organization, we could get 
// the organization urn (using https://api.linkedin.com/v2/organizationAcls?q=roleAssignee)
// and post on behalf of the organization.

string urn = $"urn:li:person:{userInfo.sub}";

Console.WriteLine($"User URN: {urn}");

var images = new[]
{
    "./images/image1.jpg",
    "./images/image2.jpg",
    "./images/image3.jpg",
    "./images/image4.jpg"
};

using (SKDocument pdfDocument = SKDocument.CreatePdf("carousel.pdf"))
{
    foreach (var path in images)
    {
        using SKBitmap bitmap = SKBitmap.Decode(path);
        using SKCanvas pageCanvas = pdfDocument.BeginPage(bitmap.Width, bitmap.Height);
        pageCanvas.DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height));
        pdfDocument.EndPage();
    }

    pdfDocument.Close();
}


var document = File.OpenRead("carousel.pdf");
 var documentId = await api.UploadDocument(urn, document, CancellationToken.None);
 Console.WriteLine($"Document: {documentId}");
await api.CreateCarousel(urn, "Test Carousel", "Document", documentId, PostCreationStatus.PUBLISHED, CancellationToken.None);

Console.WriteLine("Post created!");