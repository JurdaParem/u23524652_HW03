using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23524652_HW03.Models
{
    public class LibraryViewModel
    {
        public PagedList<students> Students { get; set; }
        public PagedList<BookAvailabilityViewModel> Books { get; set; }  // Update here

        public PagedList<authors> Authors { get; set; }
        public PagedList<types> Types { get; set; }
        public PagedList <borrows> Borrows { get; set; }
    }

}