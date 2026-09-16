using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.OpenApi; // Framework mapping extensions
using Microsoft.OpenApi;             // Concrete .NET 10 OpenAPI spec models
using System.Text;
using Scalar.AspNetCore;             // For the modern API Reference UI

var builder = WebApplication.CreateBuilder(args);

// Read configurations
var adminCfg = builder.Configuration.GetSection("Jwt:Admin");
var tenantCfg = builder.Configuration.GetSection("Jwt:Tenant");

var adminKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminCfg["Key"] ?? ""));
var tenantKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tenantCfg["Key"] ?? ""));

builder.Services.AddAuthentication() // No default scheme
    .AddJwtBearer("AdminJwt", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = adminCfg["Issuer"],
            ValidateAudience = true,
            ValidAudience = adminCfg["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = adminKey,
            ValidateLifetime = true
        };
    })
    .AddJwtBearer("TenantJwt", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tenantCfg["Issuer"],
            ValidateAudience = true,
            ValidAudience = tenantCfg["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = tenantKey,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✓ FIX: Add Native .NET 10 OpenAPI Services using matching IOpenApi types
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        // 1. Fixed Interface Mismatch: Must explicitly match IDictionary<string, IOpenApiSecurityScheme>
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // Define Admin Security Scheme
        document.Components.SecuritySchemes.Add("AdminJwt", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter Admin JWT"
        });

        // Define Tenant Security Scheme
        document.Components.SecuritySchemes.Add("TenantJwt", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter Tenant JWT"
        });

        // 2. Fixed Dictionary Mapping: Applying requirements globally using OpenApiSecuritySchemeReference
        document.Security ??= new List<OpenApiSecurityRequirement>();

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("AdminJwt", document)] = [],
            [new OpenApiSecuritySchemeReference("TenantJwt", document)] = []
        };
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Generates the native JSON metadata at /openapi/v1.json
    app.MapOpenApi();

    // Generates the new high-performance UI tool at /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Preserve existing attribute-routed API controllers
app.MapControllers();

// Add conventional route for MVC controllers and areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();