using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HybridCacheFunction;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
    [Authorize]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, HttpRequestData data)
    {
        var dataHttpReq = await req.ReadFromJsonAsync<object>();

        var authHeader = req.Headers["Authorization"].FirstOrDefault();

        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return new UnauthorizedResult();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var principal = ValidateToken(token);

        if (principal == null)
        {
            return new UnauthorizedResult();
        }

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(principal, options);

        _logger.LogInformation("C# HTTP trigger function processed a request.");
        _logger.LogInformation(json);

        var res = new
        {
            FromHttpRequest = dataHttpReq,
            userData = JsonSerializer.Deserialize<object>(json),
            Nessage = "Welcome to Azure Functions!"
        };

        return new OkObjectResult(res);
    }

    [Function("GetJWTToken")]
    public IActionResult GetJWTToken([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        return new OkObjectResult(GenerateJwt());
    }

    private static ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("dsfh537dfnbdfndverysgsgssecretbdfbhfdh4624624!##");

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "http://localhost:7071",
                ValidAudience = "your-audience",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }


    private static string GenerateJwt()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dsfh537dfnbdfndverysgsgssecretbdfbhfdh4624624!##"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
            new Claim(JwtRegisteredClaimNames.Email, "user@example.com"),
            new Claim("role", "admin")
        };

        var token = new JwtSecurityToken(
            issuer: "http://localhost:7071",
            audience: "your-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
