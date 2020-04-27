using AutoMapper;
using Library.Api.Helpers;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Profiles
{
    public class AuthorProfile : Profile
    {
        public AuthorProfile()
        {
            CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                     $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                     src.DateOfBirth.GetCurrentAge()));
            CreateMap<Entities.Book, Models.BookDto>();
            //CreateMap<Models.AuthorForCreationDto, Entities.Author>();

            //CreateMap<Models.BookForCreationDto, Entities.Book>();
        }
    }
}
