using CurrencyConverter.Application.Common.Abstractions;
using CurrencyConverter.Auth.Models;
using CurrencyConverter.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Auth
{
    public static class DependencyInjectionRegister
    {
        public static IServiceCollection AddLocalIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddJwt(configuration);
            services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
            services.AddScoped<IUserRepository, UserService>();
            return services;
        }

        private static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings(); 

            configuration.Bind(JwtSettings.SectionName, jwtSettings); 

            services.AddSingleton(Options.Create(jwtSettings));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {

                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("jwt"))
                        {
                            context.Token = context.Request.Cookies["jwt"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            return services;
        }
    }
}
