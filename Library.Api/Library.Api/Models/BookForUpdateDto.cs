using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Models
{
    public class BookForUpdateDto : BookForManipulationDto
    {
        //[Required(ErrorMessage = "You should fill out title .")]
        //[MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters")]
        //public string Title { get; set; }

        //[Required]
        //[MaxLength(500, ErrorMessage = "The description shouldn't have more than 500 characters ")]
        //public string Description { get; set; }


        [Required(ErrorMessage ="You should fill out a description.")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
