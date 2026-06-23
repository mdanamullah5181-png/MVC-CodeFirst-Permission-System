using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace MVC_CodeFirst.Models
{
    public class ProductCategory
    {
        [Key]
        public int ProductCategoryId { get; set; }

        [Required, Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }


        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<OrderDetail> Orderdetails { get; set; }

    }
}