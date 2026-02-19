using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;

const string CredentialsPath = "oauth-client.json";
const string TokenStorePath = "token-store";

if (!File.Exists(CredentialsPath))
{
    Console.WriteLine("Missing oauth-client.json");
    return;
}

string[] scopes = { CalendarService.Scope.CalendarEvents };

using var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read);

var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    GoogleClientSecrets.FromStream(stream).Secrets,
    scopes,
    "user",
    CancellationToken.None,
    new FileDataStore(TokenStorePath, true)
);

Console.WriteLine("Authorized successfully.");

if (!string.IsNullOrEmpty(credential.Token.RefreshToken))
{
    Console.WriteLine("\n===== REFRESH TOKEN =====");
    Console.WriteLine(credential.Token.RefreshToken);
    Console.WriteLine("=========================\n");
}
else
{
    Console.WriteLine("No refresh token returned.");
}