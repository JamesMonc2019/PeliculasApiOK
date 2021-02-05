
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PeliculasApi.Filtros;
using PeliculasApi.Utilidades;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();  //Apaga el mapeo automatico de Identity
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));

            //GeometryFactory  para trabajar con las ubicaciones geoespaciales
            services.AddSingleton(provider =>
                new MapperConfiguration(config =>
                {
                    var geometryFactory = provider.GetRequiredService<GeometryFactory>();
                    config.AddProfile(new AutoMapperProfiles(geometryFactory));
                }).CreateMapper());



            services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326)); //Este servicio me permite trabajar con distancias geoespaciales en c#
            ///este es el valor utilizado para hacer mediciones en el planeta tierra  srid: 4326

            // services.AddTransient<IAlmacenadorArchivos, AlmacenadorAzureStorage>();  // con estas lineas almacenamos archivos en AZURE
            services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>(); // con esta almacena el archivo de forma local

            services.AddHttpContextAccessor(); //ojo para cargar archivos locales es necesario para evitar errores

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"),
            sqlServer => sqlServer.UseNetTopologySuite())); //solo si uso UseNetTopologySuite esto se puede omitir  que es para la datos geoespaciales

           
            

            services.AddCors(options =>
            {
                var frontEndURL = Configuration.GetValue<string>("frontend_url");  ///AQUI SE ENCUENTRA CONFIGURADA MI RUTA http://localhost:4200"
                options.AddDefaultPolicy(buider =>
                {
                    buider.WithOrigins(frontEndURL).AllowAnyMethod().AllowAnyHeader()
                    .WithExposedHeaders(new string[] { "cantidadTotalRegistros" }) ;
                });
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => 
                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", policy => policy.RequireClaim("role", "admin"));
            });

            services.AddControllers(options => {
                options.Filters.Add(typeof(FiltroDeExcepcion));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PeliculasApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PeliculasApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(); //para cargar los archivos en mi carpeta local wwwroot

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
