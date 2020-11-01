using System;
using System.Security.Cryptography.X509Certificates;
using JwtAuthDemo.IdentityServer.Authentication;
using JwtAuthDemo.IdentityServer.Constants;
using JwtAuthDemo.IdentityServer.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDemo.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddSingleton<IJwtProvider, DefaultJwtProvider>();
            services.AddSingleton<IRefreshTokenProvider, InMemoryRefreshTokenProvider>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            
            var issuer = Configuration.GetValue<string>("AccessTokenIssuer");
            var audience = Configuration.GetValue<string>("AccessTokenAudience");
            var thumbprint = Configuration.GetValue<string>("AccessTokenCertificateThumbprint");
            var securityKey = GetSecurityKey(thumbprint);
            
            services.Configure<JwtOptions>(options =>
            {
                options.Issuer = issuer;
                options.Audience = audience;
                options.SecurityKey = securityKey;
                options.ExpirationTime = TimeSpan.FromMinutes(5);
            });
            
            services.AddAuthentication().AddJwtBearer(AuthSchemes.RefreshToken, options =>
            {
                options.SaveToken = false;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidateIssuer = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        private static X509SecurityKey GetSecurityKey(string thumbprint)
        {
            using var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(
                X509FindType.FindByThumbprint, thumbprint, false);

            if (certificates.Count == 0)
                throw new ApplicationException(
                    $"Certificate with thumbprint '{thumbprint}' not found"); 
            
            return new X509SecurityKey(certificates[0]);
        }
    }
}