using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Entidades
{
    public class PeliculasGeneros
    {
        public int PeliculaId { get; set; }

        public int GeneroId { get; set; }
        //creo propiedades de navegacion
        public Pelicula Pelicula { get; set; }
        public Genero Genero { get; set; }
    }
}
