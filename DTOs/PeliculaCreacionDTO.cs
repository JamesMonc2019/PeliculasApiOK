using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeliculasApi.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.DTOs
{
    public class PeliculaCreacionDTO
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 300)]
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Trailer { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaLanzamiento { get; set; }
        public IFormFile Poster { get; set; }

        //TypeBinder  aqui pongo el ModelBinder que ya he creado  // con esto puedo recibir un listado atravez de un fromform
        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GenerosIds { get; set; }
        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> CinesIds { get; set; }
        [ModelBinder(BinderType = typeof(TypeBinder<List<ActorPeliculaCreacionDTO>>))]
        public List<ActorPeliculaCreacionDTO> Actores { get; set; }

    }
}
