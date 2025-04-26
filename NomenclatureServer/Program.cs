using System.Text;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NomenclatureServer.Controllers;
using NomenclatureServer.Hubs;
using NomenclatureServer.Services;

namespace NomenclatureServer;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = new Configuration();

        // Configuration Authentication and Authorization
        ConfigureJwtAuthentication(builder.Services, configuration);

        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions =
                    MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            });
        builder.Services.AddSingleton(configuration);

        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<NomenclatureService>();
        builder.Services.AddSingleton<RegistrationService>();
        builder.Services.AddSingleton<RegistrationController>();
        builder.Services.AddSingleton<LodestoneService>();
        builder.Services.AddHostedService<LodestoneService>();

#if DEBUG
        builder.WebHost.UseUrls("https://localhost:5006");
#else
        builder.WebHost.ConfigureKestrel(options => 
        {
            options.Listen(Configuration.Ip, Configuration.Port, listenOptions =>
            {
                listenOptions.UseHttps($"{configuration.CertificatePath}", $"{configuration.CertificatePasswordPath}");
            });
        });
#endif
        
        var app = builder.Build();
        
        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseRouting();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<NomenclatureHub>("/nomenclature");
        app.MapControllers();
        app.Run();
        
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, Configuration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.SigningKey))
            };
        });
    }
}