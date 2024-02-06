using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;



var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/callback", async (HttpContext context) =>
{
    Console.WriteLine("hello world");
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
        Encoding.UTF8.GetBytes("6b71231841e2457480410c0c5a90d3b6" + ":" + "87b912e9ac5c4188952195924408fb60")));

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



app.Run();
