using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23524652_HW03.Models
{
    public class BookAvailabilityViewModel
   
    {
        public int BookId { get; set; }
        public string Name { get; set; }
        public int? PageCount { get; set; }  // Change to int? if it can be null
        public int? Point { get; set; }      // Change to int? if it can be null
        public bool IsAvailable { get; set; }

        public string Author { get; set; }
        public string Type { get; set; }
    }

}
