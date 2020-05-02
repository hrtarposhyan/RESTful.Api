using AutoMapper;
using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Api.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private IMapper _mapper;
        public AuthorCollectionsController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult CreateAuthorcollection(
            [FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection ==null)
            {
                return BadRequest();
            }
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author collection Failed on save.");
            }

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",",
                authorCollectionToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorCollection",
                new { ids = idsAsString },
                authorCollectionToReturn);

            //return Ok();
        }

        [HttpGet("{ids}",Name ="GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType =typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _libraryRepository.GetAuthors(ids);
            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }
    }
}
