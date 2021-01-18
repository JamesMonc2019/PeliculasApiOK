using PeliculasApi.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Entidades
{
    public class Genero
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:50, ErrorMessage ="el campo {0} no debe superar los 50 caracteres")]
        [PrimeraLetraMayuscula]  /// esta validacion la hice en la clase PrimeraLetraMayusculaAtribute
        public string Nombre { get; set; }
      
    }
}
