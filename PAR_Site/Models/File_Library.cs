using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PAR_Site.Models
{
    public class File_Library
    {
        public string TicketID { get; set; }
        public string FileName { get; set; }
        public string WorkOrder { get; set; }
        public int FileSize { get; set; }
        public int FileID { get; set; }
    }
}