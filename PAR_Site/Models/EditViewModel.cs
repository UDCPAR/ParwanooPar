using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PAR_Site.Models
{
    public class EditViewModel
    {
        public int ParNum { get; set; }
        public string ParStatus { get; set; }
        public string Opened { get; set; }
        public string InitiatedBy { get; set; }
        public string AssignTo { get; set; }
        public string Divison { get; set; }
        public string Issue { get; set; }
        public string WorkOrderNo { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string ClosedDate { get; set; }
        public string UpdateDiscription { get; set; }

    }
    public class All_Updates
    {
        public string UpdateComment { get; set; }
        public DateTime Date { get; set; }
        public string Techname { get; set; }
        
    }
}