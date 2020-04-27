using AutoMapper;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public AuthorsController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }
        //[HttpGet("api/authors")]

        //public IActionResult GetAuthors()
        //{
        //    var authorsFromRepo = _libraryRepository.GetAuthors();
        //    return new JsonResult(authorsFromRepo);
        //}
        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors();
            //var authors = new List<AuthorDto>();
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            //foreach (var author in authorsfromrepo)
            //{
            //    authors.add(new authordto()
            //    {
            //        id = author.id,
            //        name = $"{author.firstname} {author.lastname}",
            //        genre = author.genre,
            //        age = author.dateofbirth.getcurrentage()
            //    });
            //}

            //return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
            //return new JsonResult(authors);
            return Ok(authors);
        }

        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorsFromRepo = _libraryRepository.GetAuthor(id);
            if (authorsFromRepo == null)
            {
                return NotFound();
            }
            var author = _mapper.Map<AuthorDto>(authorsFromRepo);
            return new JsonResult(author);
        }
    }
}
