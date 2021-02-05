using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    [ApiController]
    [Route("api/peliculas")]

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class PeliculasController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly UserManager<IdentityUser> userManager;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context,
            IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos,
            UserManager<IdentityUser> userManager )
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<LandingPageDTO>> Get()
        {
            var top = 6;
            var hoy = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                .Where(x => x.FechaLanzamiento > hoy)
                .OrderBy(x => x.FechaLanzamiento)
                .Take(top)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(x => x.EnCines)
                .OrderBy(x => x.FechaLanzamiento)
                .Take(top)
                .ToListAsync();

            var resultado = new LandingPageDTO();
            resultado.ProximosEstrenos = mapper.Map<List<PeliculaDTO>>(proximosEstrenos);
            resultado.EnCines = mapper.Map<List<PeliculaDTO>>(enCines);
            return resultado;

        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PeliculaDTO>> Get(int id)
        {
            var pelicula = await context.Peliculas
                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero)
                .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                .Include(x => x.PeliculasCines).ThenInclude(x => x.Cine)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(pelicula == null) { return NotFound(); }

            var promedioVoto = 0.0;
            var usuarioVoto = 0;

            if(await context.Ratings.AnyAsync(x => x.PeliculaId == id))
            {
                promedioVoto = await context.Ratings.Where(x => x.PeliculaId == id).AverageAsync(x => x.Puntuacion);

                if(HttpContext.User.Identity.IsAuthenticated) // garantizo que si esta el usuario y evito un error
                {
                    var email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email").Value;  //SOLO FUNCIONA SI ESTAMOS EN UN Empoint [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] LO SOLUCIONO ARRIBA Y CON EL [AllowAnonymous]  en su metodo
                    var usuario = await userManager.FindByEmailAsync(email);
                    var usuarioId = usuario.Id;
                    var ratingDB = await context.Ratings.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PeliculaId == id);
                    if(ratingDB != null)
                    {
                        usuarioVoto = ratingDB.Puntuacion;
                    }
                }
            }

            var dto = mapper.Map<PeliculaDTO>(pelicula);
            dto.VotoUsuario = usuarioVoto;
            dto.PromedioVoto = promedioVoto;
            dto.Actores = dto.Actores.OrderBy(x => x.Orden).ToList();
            return dto;
        }

        [HttpGet("filtrar")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] PeliculasFiltrarDTO peliculasFiltrarDTO)
        {
            var peliculaQueryable = context.Peliculas.AsQueryable();

            if(!string.IsNullOrEmpty(peliculasFiltrarDTO.Titulo))
            {
                peliculaQueryable = peliculaQueryable.Where(x => x.Titulo.Contains(peliculasFiltrarDTO.Titulo));
            }
            if(peliculasFiltrarDTO.EnCines)
            {
                peliculaQueryable = peliculaQueryable.Where(x => x.EnCines);
            }

            if(peliculasFiltrarDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Today;
                peliculaQueryable = peliculaQueryable.Where(x => x.FechaLanzamiento > hoy);
            }

            if(peliculasFiltrarDTO.GeneroId != 0)
            {
                peliculaQueryable = peliculaQueryable
                    .Where(x => x.PeliculasGeneros.Select(y => y.GeneroId)
                    .Contains(peliculasFiltrarDTO.GeneroId));
            }

            await HttpContext.InsertarParametrosPaginacionEnCabecera(peliculaQueryable);
            var peliculas = await peliculaQueryable.Paginar(peliculasFiltrarDTO.PaginacionDTO).ToListAsync();
            return mapper.Map<List<PeliculaDTO>>(peliculas);
        }


        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);

            if(peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenedor, peliculaCreacionDTO.Poster);
            }
            EscribirOrdenActores(pelicula);

            context.Add(pelicula);
            await context.SaveChangesAsync();
            return pelicula.Id;
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<PeliculasPostGetDTO>> PostGet()
        {
            var cines = await context.Cines.ToListAsync();
            var generos = await context.Generos.ToListAsync();

            var cinesDTO = mapper.Map<List<CineDTO>>(cines);
            var generosDTO = mapper.Map<List<GeneroDTO>>(generos);
            return new PeliculasPostGetDTO() { Cines = cinesDTO, Generos = generosDTO };
        }

        [HttpGet("PutGet/{id:int}")]
        public async Task<ActionResult<PeliculasPutGetDTO>> PutGet(int id)
        {
            var peliculaActionResult = await Get(id); // mando llamar este metodo
            if(peliculaActionResult.Result is NotFoundResult) { return NotFound(); }

            var pelicula = peliculaActionResult.Value;

            var generosSeleccionadosIds = pelicula.Generos.Select(x => x.Id).ToList();
            var generosNoSeleccionadosIds = await context.Generos
                .Where(x => !generosSeleccionadosIds.Contains(x.Id))
                .ToListAsync();

            var cinesSeleccionadosIds = pelicula.Cines.Select(x => x.Id).ToList();
            var cinesNoSeleccionadosIds = await context.Cines
                .Where(x => !cinesSeleccionadosIds.Contains(x.Id))
                .ToListAsync();

            var generosNoSeleccionadosDTO = mapper.Map<List<GeneroDTO>>(generosNoSeleccionadosIds);
            var cinesNoSeleccionadosDTO = mapper.Map<List<CineDTO>>(cinesNoSeleccionadosIds);

            var respuesta = new PeliculasPutGetDTO();
            respuesta.Pelicula = pelicula;

            respuesta.GenerosSeleccionados = pelicula.Generos;
            respuesta.GenerosNoSeleccionados = generosNoSeleccionadosDTO;

            respuesta.CinesSeleccionados = pelicula.Cines;
            respuesta.CinesNoSeleccionados = cinesNoSeleccionadosDTO;

            respuesta.Actores = pelicula.Actores;
            return respuesta;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = await context.Peliculas
                .Include(x => x.PeliculasActores)
                .Include(x => x.PeliculasGeneros)
                .Include(x => x.PeliculasCines)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(pelicula == null)
            {
                return NotFound();
            }

            pelicula = mapper.Map(peliculaCreacionDTO, pelicula);

            if(peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await almacenadorArchivos.EditarArchivo(contenedor, peliculaCreacionDTO.Poster, pelicula.Poster);
            }
            EscribirOrdenActores(pelicula);

            await context.SaveChangesAsync();
            return NoContent();
        }



        private void EscribirOrdenActores(Pelicula pelicula)
        {
            if(pelicula.PeliculasActores != null)
            {
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var pelicula = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);
            if (pelicula == null)
            {
                return NotFound();
            }

            context.Remove(pelicula);
            await context.SaveChangesAsync();
            await almacenadorArchivos.BorrarArchivo(pelicula.Poster, contenedor);
            return NoContent();
        }
    }
}
