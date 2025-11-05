using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Db;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class AppAuthenticator(ProtectedLocalStorage protectedLocalStorage, DatabaseContext context) : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage protectedLocalStorage = protectedLocalStorage;
    private readonly DatabaseContext context = context;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal();

        try
        {
            var storedPrincipal = await protectedLocalStorage.GetAsync<string>("identity");

            if (storedPrincipal.Success && storedPrincipal.Value != null)
            {
                var user = JsonSerializer.Deserialize<User>(storedPrincipal.Value);
                if (user != null)
                {
                    var (_, isLookUpSuccess) = LookUpUserId(user.Id, user.PasswordHash);

                    if (isLookUpSuccess)
                    {
                        var identity = CreateIdentityFromUser(user);
                        principal = new(identity);
                    }
                }
            }
        }
        catch
        {

        }

        return new AuthenticationState(principal);
    }

    public async Task<bool> LoginAsync(string username, string passwordHash)
    {
        var (userInDatabase, isSuccess) = LookUpUsername(username, passwordHash);
        var principal = new ClaimsPrincipal();

        if (isSuccess && userInDatabase != null)
        {
            var identity = CreateIdentityFromUser(userInDatabase);
            principal = new ClaimsPrincipal(identity);
            await protectedLocalStorage.SetAsync("identity", JsonSerializer.Serialize(userInDatabase));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

            return true;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        return false;
    }

    public async Task LogoutAsync()
    {
        await protectedLocalStorage.DeleteAsync("identity");
        var principal = new ClaimsPrincipal();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private static ClaimsIdentity CreateIdentityFromUser(User user)
    {
        return new ClaimsIdentity(
        [
            new (ClaimTypes.Name, user.Id.ToString()),
        ], "CS4090");
    }

    public static string GetPasswordHash(string password)
    {
        byte[] salt = [122, 24, 184, 72, 168, 115, 221, 128, 92, 87, 185, 94, 165, 110, 240, 60, 152, 220, 51, 77, 222, 50, 115, 10, 247, 136, 89, 88, 59, 105, 28, 232];
        return Convert.ToHexString(Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),
        salt,
        350000,
        HashAlgorithmName.SHA512,
        64));
    }

    private (User?, bool) LookUpUsername(string username, string passwordHash)
    {
        var result = context.Users.FirstOrDefault(u => username == u.Username && passwordHash == u.PasswordHash);

        return (result, result is not null);
    }

    private (User?, bool) LookUpUserId(Guid id, string passwordHash)
    {
        var result = context.Users.FirstOrDefault(u => id == u.Id && passwordHash == u.PasswordHash);

        return (result, result is not null);
    }
}