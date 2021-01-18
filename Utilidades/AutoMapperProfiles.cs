using AutoMapper;
using NetTopologySuite.Geometries;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();  ///ReverseMap me indica que es un mapeo de doble via
            CreateMap<GeneroCreacionDTO, Genero>();

            CreateMap<Actor, ActorDTO>().ReverseMap();  ///ReverseMap me indica que es un mapeo de doble via
            CreateMap<ActorCreacionDTO, Actor>().ForMember(x => x.Foto, options => options.Ignore()) ;  //AQUI ESTOY IGNORANDO EL CAMPO DE LA FOTO YA QUE ESTA TIENE UN TRATO DIFERENTE

            CreateMap<CineCreacionDTO, Cine>()
                .ForMember(x => x.Ubicacion, x => x.MapFrom(dto =>
                geometryFactory.CreatePoint(new Coordinate(dto.Longitud, dto.Latitud))));
        }
    }
}
