using CMSAPI.Services;
using CMSAPIPublica.Controllers;
using CMSXData.Models;
using CMSXDAO;
using CMSXRepo;
using ICMSX;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var connPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CmsxDbContext>(o => o.UseNpgsql(connPostgres));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CMSX API", Version = "v1" });
    c.AddServer(new OpenApiServer { Url = "http://localhost:5124" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Informe o token JWT obtido em /auth/login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCMSXDAL();
builder.Services.AddCMSXRepo();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IAgentIAFactory, AgentIAFactory>();
// builder.Services.AddHostedService<PedidosServiceBusConsumer>(); // ServiceBus desabilitado — namespace excluído
builder.Services.AddScoped<PedidoServiceBusPublisher>(ps =>
{
    var config = ps.GetRequiredService<IConfiguration>();
    var logger = ps.GetRequiredService<ILogger<PedidoServiceBusPublisher>>();
    return new PedidoServiceBusPublisher(config, logger);
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    })
    .AddJwtBearer("Salematic", options =>
    {
        var salematicKey = builder.Configuration["SalematicJwt:Key"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["SalematicJwt:Issuer"],
            ValidAudience            = builder.Configuration["SalematicJwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(salematicKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHttpClient<SalematicHttpService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Salematic:BaseUrl"]!);
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api_publica", o =>
    {
        o.PermitLimit = 30;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

builder.Services.AddHealthChecks();

builder.Services.AddControllers()
     .AddApplicationPart(typeof(OrcamentoPublicoController).Assembly)
     .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CmsxDbContext>().Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("Angular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMSX API v1");
    c.RoutePrefix = "swagger";
});

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
