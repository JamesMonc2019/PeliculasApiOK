using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    [Route("api/rating")]
    [ApiController]
    public class RatingsController: ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext context;

        public RatingsController(UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]   //Aqui no uso el police solo que este autenticado
        public async Task<ActionResult> Post([FromBody] RatingDTO ratingDTO)
        {
            var email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email").Value;
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var retingActual = await context.Ratings
                .FirstOrDefaultAsync(x => x.PeliculaId == ratingDTO.PeliculaId
                 && x.UsuarioId == usuarioId);

            if(retingActual == null)
            {
                var rating = new Rating();
                rating.PeliculaId = ratingDTO.PeliculaId;
                rating.Puntuacion = ratingDTO.Puntuacion;
                rating.UsuarioId = usuarioId;
                context.Add(rating);
               
            }
            else
            {
                retingActual.Puntuacion = ratingDTO.Puntuacion;
            }
            await context.SaveChangesAsync();
            return NoContent();

        }
    }
}
