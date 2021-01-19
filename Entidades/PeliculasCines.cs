using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Entidades
{
    public class PeliculasCines
    {
        public int PeliculaId { get; set; }
        public int CineId { get; set; }
        //creo propiedades de navegacion
        public Pelicula Pelicula { get; set; }
        public Cine Cine { get; set; }
    }
}
