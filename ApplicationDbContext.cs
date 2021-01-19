using Microsoft.EntityFrameworkCore;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        //para configurar una llave compuesta se realiza con el API FLUENTE  esa llave representa la relacion muchos a muchos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeliculasActores>().HasKey(x => new { x.ActorId, x.PeliculaId });
            modelBuilder.Entity<PeliculasGeneros>().HasKey(x => new { x.PeliculaId, x.GeneroId });
            modelBuilder.Entity<PeliculasCines>().HasKey(x => new { x.PeliculaId, x.CineId });

            base.OnModelCreating(modelBuilder);  //ojo es importate no borrar esta seccion para evitar errores
        }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<Cine> Cines { get; set; }

        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<PeliculasActores> PeliculasActores { get; set; }
        public DbSet<PeliculasGeneros> PeliculasGeneros { get; set; }
        public DbSet<PeliculasCines> PeliculasCines { get; set; }
    }
}
