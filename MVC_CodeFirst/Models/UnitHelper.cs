using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_CodeFirst.Models
{
    public class UnitHelper
    {
        public static List<string> GetUnits()
        {
            return new List<string>
            {
                "pcs",      // Piece
                "kg",       // Kilogram
                "g",        // Gram
                "liter",    // Liter
                "ml",       // Milliliter
                "box",      // Box
                "pack",     // Pack
                "dozen",    // Dozen
                "meter",    // Meter
                "cm",       // Centimeter
                "inch",     // Inch
                "bottle",   // Bottle
                "can",      // Can
                "bag",      // Bag
                "others"     // Other
            };
        }

    }
}