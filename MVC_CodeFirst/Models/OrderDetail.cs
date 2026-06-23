using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_CodeFirst.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        public int? OrderId { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? ProductId { get; set; }

        public int OrderQuantity { get; set; }
        public string OrderUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }


        public virtual Order Order { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public virtual Product Product { get; set; }

    }
}