using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Models
{
    public class BookForCreationDto : BookForManipulationDto
    {
        //[Required(ErrorMessage ="You should fill out title .")]
        //[MaxLength(100,ErrorMessage ="The title shouldn't have more than 100 characters")]
        //public string Title { get; set; }

        //[MaxLength(500,ErrorMessage ="The description shouldn't have more than 500 characters ")]
        //public string Description { get; set; }
    }
}
