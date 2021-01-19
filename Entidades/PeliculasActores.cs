using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Entidades
{
    public class PeliculasActores
    {
        public int PeliculaId { get; set; }
        public int ActorId { get; set; }
        //creo propiedades de navegacion
        public Pelicula Pelicula { get; set; }
        public Actor Actor { get; set; }
        //agrego el personaje que se va a relacionar con la relacion muchos a muchos en una nueva tabla
        [StringLength(maximumLength: 100)]
        public string Personaje { get; set; }
        public int Orden { get; set; }
    }
}
