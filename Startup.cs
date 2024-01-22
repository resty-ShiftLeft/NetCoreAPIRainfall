using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace NetCoreAPIRainfall
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Caching 
            services.AddDistributedMemoryCache();

            services.AddMvcCore(options =>
            {
                options.EnableEndpointRouting = false;

            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAny",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            services.AddAuthorization();
            services.AddControllers();
            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Rainfall Api",
                    Version = "1.0",
                    Contact = new OpenApiContact
                    {
                        Name = "Sorted",
                        Url = new Uri("https://www.sorted.com")
                    },
                    Description = "An API which provides rainfall reading data",
                });

                // Define servers
                c.AddServer(new OpenApiServer
                {
                    Url = "http://localhost:3000",
                    Description = "Rainfall Api"
                });

                // Define tags
                c.TagActionsBy(api => new[] { "Rainfall"});
            });


            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromHours(12);
            });
        }

        public void ConfigureAppConfiguration(WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration
                    .AddJsonFile(@"appsettings.Development.json", false, true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>()
                    .Build();
            }
            if (builder.Environment.IsProduction())
            {
                builder.Configuration
                    .AddJsonFile(@"appsettings.json", false, true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>()
                    .Build();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rainfall Api"));
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();


            if (env.IsDevelopment())
            {
                app.UseCors(x => x
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            }
            else
            {
                app.UseCors("AllowAny");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        public void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        }
    }
}
