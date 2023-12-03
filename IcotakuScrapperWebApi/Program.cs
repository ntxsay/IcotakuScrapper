
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace IcotakuScrapperWebApi
{
    public class Program
    {
        private static readonly string[] configureOptions = ["en-US", "fr-GP", "fr"];

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = configureOptions;
                options.SetDefaultCulture(supportedCultures[2])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });

            //IcotakuScrapper.Main.SetCultureInfo(configureOptions[2]);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            

            app.MapControllers();

            app.Run();
        }
    }
}
