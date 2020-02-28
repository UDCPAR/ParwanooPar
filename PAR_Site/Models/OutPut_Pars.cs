using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PAR_Site.Models
{
    public class OutPut_Pars
    {

        public int ParNumber { get; set; }
        public string ParStatus { get; set; }
        public DateTime OpenDate { get; set; }
        public string ClosedDate { get; set; }
        public string Initiatedby { get; set; }
        public string AssignTo { get; set; }
        public string Divison { get; set; }
        public string Issue { get; set; }
        public string WorkOrder { get; set; }
        public string Par_Subject { get; set; }
        public string Par_Description { get; set; }
        public string Comments { get; set; }
        public List<string> AllComments { get; set; }
        
    }
}