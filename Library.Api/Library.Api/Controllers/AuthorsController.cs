using AutoMapper;
using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Http;
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

        //[HttpGet("{id}")]
        [HttpGet("{id}", Name = "GetAuthor")]
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

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failedon save.");
                //return StatusCode(500, "A problem happend with handling your request");
            }
            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor",
                new { id = authorToReturn.Id },
                authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteAuthor(authorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting author {id}  failed on save.");
            }

            return NoContent();
        }
    }
}
