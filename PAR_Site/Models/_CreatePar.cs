using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PAR_Site.Models
{
    public class _CreatePar
    {
        public _CreatePar()
        {
            Files = new List<HttpPostedFileBase>();
        }
        public List<HttpPostedFileBase> Files { get; set; }
        public string ProjectType { get; set; }
        public string ProjectPhase { get; set; }
        public string Location { get; set; }
        public string TechName { get; set; }
        public string Issue { get; set; }
        public string WorkOrderNo { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
    }
}