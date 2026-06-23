using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_CodeFirst.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }

    }
}