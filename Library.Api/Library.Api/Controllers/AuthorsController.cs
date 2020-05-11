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
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperService;

        public AuthorsController(
            ILibraryRepository libraryRepository,
            IMapper mapper,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters,
            [FromHeader(Name ="Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>
                (authorsResourceParameters.OrderBy))
                return BadRequest();

            if (!_typeHelperService.TypeHasProperties<AuthorDto>
                (authorsResourceParameters.Fields))
                return BadRequest();

            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

            //var previousPageLink = authorsFromRepo.HasPrevious ?
            //    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;
            //var nextPageLink = authorsFromRepo.HasNext ?
            //    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = authorsFromRepo.TotalCount,
                    pageSize = authorsFromRepo.PageSize,
                    currentPage = authorsFromRepo.CurrentPage,
                    totalPages = authorsFromRepo.TotalPages,
                    //previousPageLink = previousPageLink,
                    //nextPageLink = nextPageLink
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForAuthors(authorsResourceParameters,
                authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

                var shapedAuthors = authors.ShapeData(authorsResourceParameters.Fields);

                var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
                 {
                     var authorAsDictionary = author as IDictionary<string, object>;
                     var authorLinks = CreateLinksForAuthor(
                         (Guid)authorAsDictionary["Id"], authorsResourceParameters.Fields);

                     authorAsDictionary.Add("links", authorLinks);

                     return authorAsDictionary;
                 });

                var linkedCollectionResource = new
                {
                    value = shapedAuthorsWithLinks,
                    links = links
                };

                // return Ok(authors);
                // return Ok(authors.ShapeData(authorsResourceParameters.Fields));

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = authorsFromRepo.HasPrevious ?
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;
                var nextPageLink = authorsFromRepo.HasNext ?
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;
                var paginationMetadata = new
                {
                    totalCount = authorsFromRepo.TotalCount,
                    pageSize = authorsFromRepo.PageSize,
                    currentPage = authorsFromRepo.CurrentPage,
                    totalPages = authorsFromRepo.TotalPages,
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                return Ok(authors.ShapeData(authorsResourceParameters.Fields));
            }
        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            serchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            serchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            serchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }
        }

        //[HttpGet("{id}")]
        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
                return BadRequest();

            var authorsFromRepo = _libraryRepository.GetAuthor(id);

            if (authorsFromRepo == null)
                return NotFound();

            var author = _mapper.Map<AuthorDto>(authorsFromRepo);

            var links = CreateLinksForAuthor(id, fields);

            var linkedResourceToReturn = author.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            //return new JsonResult(author);

            // return Ok(author);
            //return Ok(author.ShapeData(fields));

            return Ok(linkedResourceToReturn);
        }

        /// <summary>
        /// Creating a resource
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost(Name =("CreateAuthor"))]
        [RequestHeaderMatchesMediaType("Content-Type",
            new[] { "application/vnd.marvin.author.full+json"})]
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

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            //return CreatedAtRoute("GetAuthor",
            //    new { id = authorToReturn.Id },
            //    authorToReturn);
            return CreatedAtRoute("GetAuthor",
                new { id = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpPost(Name = ("CreateAuthorWithDateOfDeath"))]
        [RequestHeaderMatchesMediaType("Content-Type",
            new[] { "application/vnd.marvin.authorwithdateofdeath.full+json" })]
        public IActionResult CreateAuthorWithDateOfDeath([FromBody] AuthorForCreationWithDateOfDeathDto author)
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

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            //return CreatedAtRoute("GetAuthor",
            //    new { id = authorToReturn.Id },
            //    authorToReturn);
            return CreatedAtRoute("GetAuthor",
                new { id = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        /// <summary>
        /// This method handles posts to a single resource
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteAuthor")]
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

        //HATEOAS => Dynamic Approch for single resources
        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {

                links.Add(new LinkDto(_urlHelper.Link("GetAuthor",
                        new { id = id }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(new LinkDto(_urlHelper.Link("GetAuthor",
                        new { id = id, fields = fields }),
                    "self",
                    "GET"));
            }

            links.Add(new LinkDto(_urlHelper.Link("DeleteAuthor",
                        new { id = id }),
                    "delete_author",
                    "DELETE"));

            links.Add(
                new LinkDto(_urlHelper.Link("CreateBookForAuthor", new { authorId = id }),
                "create_book_for_author",
                "POST"));

            links.Add(new LinkDto(_urlHelper.Link("GetBooksForAuthor",
                       new { authorId = id }),
                   "books",
                   "GET"));

            return links;
        }

        //HATEOAS => Dynamic collection approch
        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorsResourceParameters authorsResourceParameters,
            bool hasNext,bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self
            links.Add (
                new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.Current),
                "self",
                "GET"));

            if (hasNext)
            {
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,
                    ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }
            return links;
        }
    }
}
