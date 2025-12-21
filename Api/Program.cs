using ASP.NET_Core_Template.Ioc;
using ASP.NET_Core_Template.Setups;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowedCorsOrigins",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

if (builder.Configuration.GetValue<bool>("Swagger:Enabled", false))
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSwagger();
}

builder.Services.RegisterServices(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);


builder.Services.AddIdentityAuthentication(builder.Configuration);

builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ASP.NET_Core_Template.Middleware.ApiKeyAuthenticationHandler>(
    "ApiKey", options => { }
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedCorsOrigins",
    builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
});

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Swagger:Enabled", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowedCorsOrigins");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();