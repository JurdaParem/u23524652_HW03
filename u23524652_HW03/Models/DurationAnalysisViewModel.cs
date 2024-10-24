using System.Collections.Generic;

namespace u23524652_HW03.Models
{
    public class DurationAnalysisViewModel
    {
        public List<string> ReportFiles { get; set; } // List of report file names
        public DurationAnalysisReport Report { get; set; } // Report details for generating new reports
        
        // Optional: List to hold descriptions for each report file
        public Dictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>();
    }
}
