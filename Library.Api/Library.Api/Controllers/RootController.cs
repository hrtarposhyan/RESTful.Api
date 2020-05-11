using Library.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Controllers
{
    [Route("api")]
    public class RootController:Controller
    {
        private readonly IUrlHelper _urlHelper;

        public RootController(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [HttpGet(Name ="GetRoot")]
        public IActionResult GetRoot([FromHeader(Name ="Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var links = new List<LinkDto>();

                // self
                links.Add(
                    new LinkDto(_urlHelper.Link("GetRoot", new { }),
                    "self",
                    "GET"));

                links.Add(
                    new LinkDto(_urlHelper.Link("GetAuthors", new { }),
                    "authors",
                    "GET"));

                links.Add(
                    new LinkDto(_urlHelper.Link("CreateAuthor", new { }),
                    "create_author",
                    "POST"));

                return Ok(links);
            }

            return NoContent();
        }
    }
}
