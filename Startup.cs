using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aspnet_resource_manifest_sample.Cache;
using aspnet_resource_manifest_sample.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace aspnet_resource_manifest_sample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add IHttpContextAccessor and ResourceManifestCache
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IResourceManifestCache, ResourceManifestCache>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".br"] = "application/brotli";

            app.UseStaticFiles(new StaticFileOptions {
                ContentTypeProvider = contentTypeProvider,
                OnPrepareResponse = context => {
                    context.Context.Response.Headers.Add("Cache-Control", "public, max-age=31536000");

                    var filename = context.File.Name.ToLowerInvariant();
                    if (filename.EndsWith(".gz")) {
                        if (filename.EndsWith("js.gz")) {
                            context.Context.Response.ContentType = "application/javascript";
                        } else if (filename.EndsWith("css.gz")) {
                            context.Context.Response.ContentType = "text/css";
                        }

                        context.Context.Response.Headers.Add("Content-Encoding", "gzip");
                    } else if (filename.EndsWith(".br")) {
                        if (filename.EndsWith("js.br")) {
                            context.Context.Response.ContentType = "application/javascript";
                        } else if (filename.ToLowerInvariant().EndsWith("css.br")) {
                            context.Context.Response.ContentType = "text/css";
                        }

                        context.Context.Response.Headers.Add("Content-Encoding", "br");
                    }
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
