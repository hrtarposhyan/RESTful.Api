using Library.Api.Entities;
using Library.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Services
{
    public interface ILibraryRepository
    {
        // IEnumerable<Author> GetAuthors();
        // IEnumerable<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters);
        PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters);

        Author GetAuthor(Guid authorId);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        IEnumerable<Book> GetBooksForAuthor(Guid authorId);
        Book GetBookForAuthor(Guid authorId, Guid bookId);
        void AddBookForAuthor(Guid authorId, Book book);
        void UpdateBookForAuthor(Book book);
        void DeleteBook(Book book);
        bool Save();

    }
}
