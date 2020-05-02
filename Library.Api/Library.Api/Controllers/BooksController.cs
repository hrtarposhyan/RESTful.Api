using AutoMapper;
using Library.Api.Entities;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;
        public BooksController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var booksForAuthorRepo = _libraryRepository.GetBooksForAuthor(authorId);

            var booksForAuthor = _mapper.Map<IEnumerable<BookDto>>(booksForAuthorRepo);
            return Ok(booksForAuthor);
        }

        [HttpGet("{id}",Name ="GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId,Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorRepo == null)
            {
                return NotFound();
            }
            var bookForAuthor = _mapper.Map<BookDto>(bookForAuthorRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId,
            [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookEntity = _mapper.Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = _mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor",
                new { authorId = authorId, id = bookToReturn.Id },
                bookToReturn);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteBook(bookForAuthorFromRepo);

            if (_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId,Guid id,
            [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                //return NotFound();
                var bookToAdd = _mapper.Map<Book>(book);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");
                }

                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor",
                    new { authorId = authorId, id = bookToReturn.Id },
                    bookToReturn);
            }

            //map

            //apply update

            //map back to entity
            _mapper.Map(book, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on save.");
            }
            return NoContent();
        }


        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId,Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            if (_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorRepo == null)
                return NotFound();

            var bookToPatch = _mapper.Map<BookForUpdateDto>(bookForAuthorRepo);
            patchDoc.ApplyTo(bookToPatch);

            // add validation

            _mapper.Map(bookToPatch, bookForAuthorRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} failed on save");
            }

            return NoContent();
        }
    }
}
