using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using ImageDemo.IRepositories;
using ImageDemo.Repositories;
using DbContext = ImageDemo.Db.DbContext;

namespace ImageDemo
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
            #region 日志

            Log.Logger=new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft",LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs",@"log.txt"),rollingInterval:RollingInterval.Day)
                .CreateLogger();

            #endregion

            #region 链接数据库

            services.AddDbContext<DbContext>(s =>
            {
                s.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")); 

            });

            #endregion

            services.AddScoped<IImagesResource, ImagesResource>();

            #region Swagger

            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new Info
                {
                    Title = "图片上传",
                    Description = "图片上传测试",
                    Version = "v1"
                });

                #region XML备注

                var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
                var imagePath = Path.Combine(basePath, "ImageDemo.xml");
                s.IncludeXmlComments(imagePath,true);

                #endregion
            });

            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(s => s.SwaggerEndpoint("/swagger/v1/swagger.json", "v1版本"));
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
