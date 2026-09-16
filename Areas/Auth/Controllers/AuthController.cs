using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config) => _config = config;

    [HttpPost("admin/token")]
    public IActionResult IssueAdminToken([FromBody] LoginDto dto)
    {
        // validate admin credentials...
        var cfg = _config.GetSection("Jwt:Admin");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, dto.UserId),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: cfg["Issuer"],
            audience: cfg["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("tenant/token")]
    public IActionResult IssueTenantToken([FromBody] LoginDto dto)
    {
        // validate tenant credentials...
        var cfg = _config.GetSection("Jwt:Tenant");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, dto.UserId),
            new Claim("tenantId", dto.TenantId)
        };

        var token = new JwtSecurityToken(
            issuer: cfg["Issuer"],
            audience: cfg["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

public record LoginDto(string UserId, string TenantId);
