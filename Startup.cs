using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace source_code_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            #region SWAGER CODE FOR TESTING API & DOCUMENTATION (OpenAPI)

            // ++++++++++++++++++++++++++ SWAGGER CODE  STARTS ++++++++++++++++++++++++++++++++++ //

            // Install-Package Swashbuckle.AspNetCore -Version 5.3.3 

            services.AddSwaggerGen(config =>
            {
                // Swagger Documentation Code
                // 'version1' -- name should match with above config settings
                config.SwaggerDoc("version1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Swagger Usage Example",
                    Description = "Swagger Usage Example"
                });

                // Basic Swagger Config For authentication of API , now you do not require any client to test API 
                // ALso it it helps you to document API automatically
                // SWAGGER FOR Jwt Token
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Format : Bearer {token}"
                });

                // Global Authentication scheme For Swagger
                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                   {
                       {
                   new OpenApiSecurityScheme
                     {
                         Reference = new OpenApiReference
                         {
                             Type = ReferenceType.SecurityScheme,
                             Id = "Bearer"
                         }
                     },
                     new string[] {}

             }
                          });
            });

            // ++++++++++++++++++++++++++ SWAGGER CODE  ENDS ++++++++++++++++++++++++++++++++++ //

            #endregion

            #region  Authentication Code For JWT Token

            // install-package Microsoft.AspNetCore.Authentication.JwtBearer 
            // for JWT Token Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = "example.com",
                         ValidAudience = "example.com",
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UseAnySecreateKeyHereForToken"))
                     };
                 });

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            #region USAGE OF AUTHENTICATION CODE FOR JWT TOKEN

            //BELOW CALLING ORDER OF UseAuthentication & UseAuthorization IS IMPORTANT//

            // for authentication of JWT Token
            app.UseAuthentication();
            // 

            app.UseAuthorization();

            #endregion


            #region USAGE OF SWAGER CODE FOR TESTING API & DOCUMENTATION (OpenAPI)

            // ++++++++++++++++++++++++++ SWAGGER CODE  STARTS ++++++++++++++++++++++++++++++++++ //

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)

            // Specifying the Swagger JSON endpoint as below.
            // 'version1' -- name should match with above config settings
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/version1/swagger.json", "Swagger Example");
            });
            // TO SEE SWAGGER JSON FILE : http://localhost/swagger/version1/swagger.json  
            // TO SEE UI : http://localhost/swagger/index.html
            //Use localhost:port  like url if required //
            // ++++++++++++++++++++++++++ SWAGGER CODE  ENDS ++++++++++++++++++++++++++++++++++ //

            #endregion


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
