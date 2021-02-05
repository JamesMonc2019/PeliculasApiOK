using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Filtros;
using PeliculasApi.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    [Route("api/generos")]
    [ApiController] // este decorado hace esta validacion en todo mi metodo  // if(!ModelState.IsValid)  {  return BadRequest(ModelState);  }
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class GenerosController: ControllerBase  //metodo auxiliar ControllerBase me permite acceder a un cierto numero de metodos auxiliares uno de ellos es poder retornanr un 404
    {
        private readonly ILogger<GenerosController> loggger;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public GenerosController(
            ILogger<GenerosController> loggger,
            ApplicationDbContext context,
            IMapper mapper )
        {
            this.loggger = loggger;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        //  [ResponseCache(Duration = 60)]  ///para tener datos en cache
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<GeneroDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            loggger.LogInformation("Vamos a mostrar los generos");

            var queryable =  context.Generos.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var generos = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<GeneroDTO>>(generos);
        }
        [HttpGet("todos")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GeneroDTO>>> Todos()
        {
            var generos = await context.Generos.ToListAsync();
            return mapper.Map<List<GeneroDTO>>(generos);
        }


        [HttpGet("{Id:int}")]
        public async Task<ActionResult<GeneroDTO>> Get(int Id)
        {
            var genero = await context.Generos.FirstOrDefaultAsync(x => x.Id == Id);
            if(genero == null)
            {
                return NotFound();
            }
            return mapper.Map<GeneroDTO>(genero);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)  // FromBody  regularmete usamos paracrear o editar data
        //public void Post()
        {
            var genero = mapper.Map<Genero>(generoCreacionDTO);
            context.Add(genero);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult> Put(int Id, [FromBody] GeneroCreacionDTO generoCreacionDTO )
        {
            var genero = await context.Generos.FirstOrDefaultAsync(x => x.Id == Id);
            if (genero == null)
            {
                return NotFound();
            }
            genero = mapper.Map(generoCreacionDTO, genero);

            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Generos.AnyAsync(x => x.Id == id);
            if(!existe)
            {
                return NotFound();
            }

            context.Remove(new Genero() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
