using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_CodeFirst.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }


        public virtual Customer Customer { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

    }
}