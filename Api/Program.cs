using Application.Workers;
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
app.UseHttpsRedirection();

// IMPORTANTE: UseWebSockets DEVE vir ANTES de UseAuthentication
// para que IsWebSocketRequest funcione corretamente
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        if (context.Request.Cookies.Contains(new KeyValuePair<string, string>("type", "drive")))
        {
            var authService = context.RequestServices.GetRequiredService<IAuthenticationService>();

            var result = await authService.AuthenticateAsync(context, "ApiKey");

            if (!result.Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            context.User = result.Principal;

        }
        var serviceProvider = context.RequestServices.GetRequiredService<AppFileWorker>();

        await serviceProvider.AcceptWebSocketAsync(
            context,
            await context.WebSockets.AcceptWebSocketAsync(),
            context.RequestAborted
        );

        return;
    }

    await next();
});

app.MapControllers();
app.Run();