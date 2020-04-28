using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Profiles
{
    public class BookProfile:Profile
    {
        public BookProfile()
        {
            CreateMap<Entities.Book, Models.BookDto>();

            CreateMap<Models.BookForCreationDto, Entities.Book>();
        }
    }
}
