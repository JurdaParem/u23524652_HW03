using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23524652_HW03.Models
{
    public class DurationAnalysisReport
    {
        public double AverageBorrowDuration { get; set; }
        public List<BookBorrowDetail> BorrowDetails { get; set; } = new List<BookBorrowDetail>();
    }
}