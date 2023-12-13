
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace IcotakuScrapperWebApi
{
    public class Program
    {
        private static readonly string[] configureOptions = ["en-US", "fr-GP", "fr", "fr-FR"];

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
                options.SetDefaultCulture(supportedCultures[3])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });

            //IcotakuScrapper.Main.LoadDatabaseAt(@"C:\Datas\icotaku.db");
            
            //Initialise la connexion à la base de données SQLite
            IcotakuScrapper.Main.InitializeDbConnectionString(null);
            
            //Initialise le dossier de travail
            //IcotakuScrapper.Main.LoadWorkingDirectoryAt(@"C:\Datas\icotaku");

            //Interdit l'accès au contenu adulte au sein de l'application
            IcotakuScrapper.Main.IsAccessingToAdultContent = false;
            
            //Autorise l'accès au contenu explicite au sein de l'application
            IcotakuScrapper.Main.IsAccessingToExplicitContent = true;
            
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
