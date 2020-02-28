using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PAR_Site.Models
{
    public class Login
    {
        public int TicketId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    public class ViewModel
    {
        public string AssignTo { get; set; }
        public string InitiatedBy { get; set; }
        public string Map { get; set; }
        public string Status { get; set; }
        public string OpenDate { get; set; }
        public string TicketTittle { get; set; }
        public int TicketID { get; set; }
    }
    public class Account_Info
    {

        [Required(ErrorMessage = "Current Password is required.")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

}