﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using petsapi.Helpers;
using petsapi.Models;

namespace petsapi
{
    public class Startup
    {
        private string _secretKey = null;
        private SymmetricSecurityKey _signingKey = null;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IHostingEnvironment _env { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            FacebookAuthSettings fbTokens = new FacebookAuthSettings();
            //Api keys from the app-secret asp.net core provider
            if (_env.IsDevelopment() ) 
            {
                _secretKey = Configuration["ApiKey"];
                services.AddDbContext<TodoContext>(p => p
                    .UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            
                fbTokens.AppSecret = Configuration["FacebookAuthSettings:AppSecret"];
                fbTokens.AppId = Configuration["FacebookAuthSettings:AppId"];
            }
            //Api keys from env variables from hosting
            else 
            {
                _secretKey = Environment.GetEnvironmentVariable("ApiKey");

                fbTokens.AppSecret = Environment.GetEnvironmentVariable("FbSecret");
                fbTokens.AppId= Environment.GetEnvironmentVariable("FbId");
            
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                var databaseUri = new Uri(databaseUrl);
                var userInfo = databaseUri.UserInfo.Split(':');

                var conBuilder = new NpgsqlConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.LocalPath.TrimStart('/')
                };

                var constring = conBuilder.ToString();

                services.AddDbContext<TodoContext>(p => p
                    .UseNpgsql( constring ));
            }

            //Dependency Injection
            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddSingleton<FacebookAuthSettings> (fbTokens);
            
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));

            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Id));
            });

            var builder = services.AddIdentityCore<AppUser>(o => 
            {
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
                o.Password.RequireDigit = false;
            });
                        
            builder.AddEntityFrameworkStores<TodoContext>().AddDefaultTokenProviders();

            services.AddSignalR();

            services.AddCors(); //Development
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            //Development
            app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .WithExposedHeaders("PagingHeader")
            );

            app.UseStaticFiles();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
