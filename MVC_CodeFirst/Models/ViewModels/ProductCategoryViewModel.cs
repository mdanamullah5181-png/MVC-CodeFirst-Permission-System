using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace MVC_CodeFirst.Models.ViewModels
{
    public class ProductCategoryViewModel
    {
        public int ProductCategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [Display(Name = "Category Description")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string CategoryDescription { get; set; }

    }
}