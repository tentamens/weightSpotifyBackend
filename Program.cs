using System.Text;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod();
});





var clientId = "6b71231841e2457480410c0c5a90d3b6";
var clientSecret = "87b912e9ac5c4188952195924408fb60";


app.MapGet("/", () => "Hello World!");



app.MapGet("/callback", async (HttpContext context) =>
{
    var request = context.Request;
    var response = context.Response;

    var code = request.Query["code"].ToString();
    var state = request.Query["state"].ToString();


    if (state == null)
    {
        await response.WriteAsync("somthing when wrong ");
        return;
    }
    var authOption = new
    {
        url = "https://accounts.spotify.com/api/token",
        form = new
        {
            code = code,
            redirct_uri = "http://localhost:5107/callback",
            grant_type = "authorization_code",
        },
    };

    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(
        Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));

    var formData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", authOption.form.code },
                { "redirect_uri", "http://localhost:5107/callback"},
                { "grant_type", authOption.form.grant_type }
            });

    // await the response.WriteAsync method call here

    var spotifyResponse = await httpClient.PostAsync(authOption.url, formData);

    if (spotifyResponse.IsSuccessStatusCode)
    {
        var content = await spotifyResponse.Content.ReadAsStringAsync();
        response.StatusCode = (int)spotifyResponse.StatusCode;
        await response.WriteAsync("Spotify API response: " + content);

    }
    else
    {

        response.StatusCode = (int)spotifyResponse.StatusCode;
        await response.WriteAsync("Error: " + spotifyResponse.StatusCode);
    }


});

app.MapPost("/api/refreshtoken", async (HttpContext context) =>
{
    var request = context.Request;
    var response = context.Response;
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();

    // Deserialize the JSON payload into an object
    var requestBodyObject = JsonSerializer.Deserialize<YourRequestModel>(requestBody);

    // Access the "Token" field from the deserialized object
    var token = requestBodyObject.refreshToken;

    var clientIdAndSecret = $"{clientId}:{clientSecret}";
    var base64ClientIdAndSecret = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(clientIdAndSecret));


    using var client = new RestClient();

    var requestCall = new RestRequest("https://accounts.spotify.com/api/token")
     .AddParameter("grant_type", "refresh_token")
     .AddParameter("refresh_token", token)
     .AddParameter("client_id", clientId)
     .AddHeader("Authorization", $"Basic {base64ClientIdAndSecret}")
     .AddHeader("Content-Type", "application/x-www-form-urlencoded");
    var result = await client.PostAsync(requestCall);

    await response.WriteAsync(result.Content);

});


app.Run();
public class YourRequestModel
{
    public string refreshToken { get; set; }
}