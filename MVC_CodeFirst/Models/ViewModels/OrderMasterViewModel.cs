using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace MVC_CodeFirst.Models.ViewModels
{
    public class OrderMasterViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [Display(Name = "Contact Number")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Contact Address is required")]
        [Display(Name = "Contact Address")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string ContactAddress { get; set; }

        public int? CustomerId { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public List<OrderDetailsViewModel> OrderDetailsList { get; set; }

        public OrderMasterViewModel()
        {
            OrderDetailsList = new List<OrderDetailsViewModel>();
            OrderDate = DateTime.Now;
        }

    }
}