using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_CodeFirst.Models
{
    public class Customer
    {

        [Key]
        public int CustomerId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactAddress { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

    }
}