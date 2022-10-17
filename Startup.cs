using Microsoft.OpenApi.Models;
using elefanti.video.backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using elefanti.video.backend.Services;
using FluentValidation;
using elefanti.video.backend.Models;
using Microsoft.Extensions.FileProviders;

namespace elefanti.video.backend {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
            MyAllowedOrigins = "AllowedOrigins";
        }

        public IConfiguration Configuration { get; }
        public String MyAllowedOrigins { get; }

        public void ConfigureServices(IServiceCollection services) {

            services.AddControllers();

            services.AddSwaggerGen(setup => {
                var jwtSecurityScheme = new OpenApiSecurityScheme {
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });

            });


            services.AddDbContext<DbConnection>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            services.AddCors((options) => {
                options.AddPolicy(name: MyAllowedOrigins,
                                  policy => {
                                      policy
                                      .WithOrigins(
                                         Configuration.GetValue<String>("OriginsAllowed"))
                                       .AllowAnyHeader()
                                       .AllowAnyMethod();
                                  });
                options.AddPolicy(name: "DevOrigins",
                                    policy => {
                                        policy.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader();
                                    });
            });
            services.AddScoped<TokenService>();
            services.AddScoped<PasswordService>();

            services.AddScoped<IValidator<UserPost>, UserValidator>();
            services.AddScoped<IValidator<VideoPost>, VideoValidator>();
            services.AddScoped<IValidator<CommentPost>, CommentValidator>();
            services.AddScoped<IValidator<CategoryDto>, CategoryValidator>();

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = "Elefanti-Video",
                        ValidAudience = "Elefanti-Video",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWTKey"))) // The same key as the one that generate the token
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elefanti Video"));
                app.UseCors("DevOrigins");
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles(new StaticFileOptions {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "assets")),
                        RequestPath = "/api/assets"
            });


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(MyAllowedOrigins);

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
