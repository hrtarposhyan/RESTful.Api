﻿using AutoMapper;
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

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),
                    "The provided description should be different from the tuitle.");
            }

            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
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


            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description should be different from the tuitle.");
            }

            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
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

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                //return NotFound();

                var bookDto = new BookForUpdateDto();

                patchDoc.ApplyTo(bookDto,ModelState);

                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto),
                        "The provided description should be different from the tuitle.");
                }

                TryValidateModel(bookDto);

                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = _mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");
                }

                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id },
                    bookToReturn);
            }
                

            var bookToPatch = _mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided description should be different from the tuitle.");
            }
            TryValidateModel(bookToPatch);

            //patchDoc.ApplyTo(bookToPatch);

            patchDoc.ApplyTo(bookToPatch, ModelState);
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // add validation

            _mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} failed on save");
            }

            return NoContent();
        }
    }
}
