using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_CodeFirst.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Unit { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public int AvailableQuantity { get; set; }

        public string ProductImage { get; set; }


        public int? ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        [NotMapped]
        public HttpPostedFileBase UploadImage { get; set; }

    }
}