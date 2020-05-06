using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PAR_Site.Models;

namespace PAR_Site.Controllers
{

    public class DefaultController : Controller
    {
        NagParDemoEntities db = new NagParDemoEntities();
        public ActionResult Index(string ddlValuestr = null)
        {
            int ddlValue;
            if (Request["TechName"] == "")
            {
                ddlValue = 0;
            }
            else
            {
                ddlValuestr = Request["TechName"];
                ddlValue = Convert.ToInt32(ddlValuestr);
            }
            ViewBag.ddlValue = ddlValue;
            string S_Project = Session["Selected_Project"].ToString();
            var Techlist = db.tblUserIndexes.Where(x => x.AccountStatusID == 2 && x.Project.Contains(S_Project)).Select(p => new SelectListItem()
            {
                Value = p.UserID.ToString(),
                Text = p.FirstName + " " + p.LastName
            }).ToList();

            ViewBag.username = Techlist;
            TotalCount();
            OpenPars(ddlValue);
            return View();
        }
        public ActionResult TotalCount()
        {
            string S_Project = Session["Selected_Project"].ToString();
            int TID = Convert.ToInt32(ViewBag.ddlValue);
            var today = DateTime.Now.Date;
            var fiveday = today.AddDays(-5);
            var LessThenTen = today.AddDays(-10);
            if (TID > 0)
            {
                var beforefiveday = db.tblTicketIndexes.Where(x => x.TicketOpen >= fiveday && x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.TechID == TID && x.ProjectType.Contains(S_Project)).Count();
                ViewBag.beforefiveday = beforefiveday;
                var beforeTenday = db.tblTicketIndexes.Where(x => x.TicketOpen >= LessThenTen && x.TicketOpen < fiveday && x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.TechID == TID && x.ProjectType.Contains(S_Project)).Count();
                ViewBag.beforeTenday = beforeTenday;
                var total = db.tblTicketIndexes.Where(x => x.TicketClose == null && x.TicketOpen < LessThenTen && x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.TechID == TID && x.ProjectType.Contains(S_Project)).Count();
                ViewBag.TotalCountForTen = total;
            }
            else
            {
                var beforefiveday = db.tblTicketIndexes.Where(x => x.TicketOpen >= fiveday && x.TicketClose == null && x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.ProjectType.Contains(S_Project)).Count();
                ViewBag.beforefiveday = beforefiveday;
                var beforeTenday = db.tblTicketIndexes.Where(x => x.TicketOpen >= LessThenTen && x.TicketOpen < fiveday && x.TicketClose == null && x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.ProjectType.Contains(S_Project)).Count();
                ViewBag.beforeTenday = beforeTenday;
                var total = db.tblTicketIndexes.Where(x => x.TicketClose == null && x.TicketOpen < LessThenTen && x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.ProjectType.Contains(S_Project)).Count();
                ViewBag.TotalCountForTen = total;
            }
            int userid = Convert.ToInt32(Session["UserID"]);
            ViewBag.TechParCount = db.tblTicketIndexes.Where(x => x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.TechID == userid && x.ProjectType.Contains(S_Project)).Count();
            ViewBag.UserParCount = db.tblTicketIndexes.Where(x => x.TicketStatusID != 3 && x.TicketStatusID != 7 && x.UserID == userid && x.ProjectType.Contains(S_Project)).Count();
            return View();
        }
        public ActionResult OpenPars(int ddlValue)
        {
            string S_Project = Session["Selected_Project"].ToString();
            ViewModel viewModel = new ViewModel();
            List<tblUserIndex> tblUsers = db.tblUserIndexes.ToList();
            List<tblTicketIndex> tblTickets = db.tblTicketIndexes.ToList();
            try
            {
                if (ddlValue == 0)
                {
                    var Open = (from t in tblTickets
                                join u in tblUsers
                                on t.UserID equals u.UserID
                                where t.TicketStatusID != 3 && t.TicketStatusID != 7 && t.ProjectType.Contains(S_Project)
                                select new ViewModel
                                {
                                    Map = t.Map,
                                    TicketTittle = t.TicketTitle,
                                    TicketID = t.TicketID,
                                    Status = db.tblTicketStatusIndexes.Where(x => x.TicketStatusID == t.TicketStatusID).FirstOrDefault().TicketStatus,
                                    OpenDate = t.TicketOpen.ToString("MM/dd/yyyy"),
                                    InitiatedBy = u.FirstName + " " + u.LastName,
                                    AssignTo = tblUsers.Where(x => x.UserID == t.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault()
                                }).ToList();
                    ViewBag.showing = "All";
                    return View(Open);
                }
                else
                {
                    var Open = (from t in tblTickets
                                join u in tblUsers
                                on t.UserID equals u.UserID
                                where t.TicketStatusID != 3 && t.TicketStatusID != 7 && t.TechID.Equals(ddlValue) && t.ProjectType.Contains(S_Project)
                                select new ViewModel
                                {
                                    Map = t.Map,
                                    TicketTittle = t.TicketTitle,
                                    TicketID = t.TicketID,
                                    Status = db.tblTicketStatusIndexes.Where(x => x.TicketStatusID == t.TicketStatusID).FirstOrDefault().TicketStatus,
                                    OpenDate = t.TicketOpen.ToString("MM/dd/yyyy"),
                                    InitiatedBy = u.FirstName + " " + u.LastName,
                                    AssignTo = tblUsers.Where(x => x.UserID == t.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault()
                                }).ToList();
                    ViewBag.showing = tblUsers.Where(x => x.UserID == ddlValue).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
                    return View(Open);
                }

            }
            catch (Exception)
            {
                return View("Error", "Shared");
            }
        }
        public ActionResult UserAllPars()
        {
            string S_Project = Session["Selected_Project"].ToString();
            int userid = Convert.ToInt32(Session["UserID"]);
            ViewModel viewModel = new ViewModel();
            List<tblUserIndex> tblUsers = db.tblUserIndexes.ToList();
            List<tblTicketIndex> tblTickets = db.tblTicketIndexes.ToList();
            try
            {
                var Open = (from t in tblTickets
                            join u in tblUsers
                            on t.UserID equals u.UserID
                            where t.TicketStatusID != 3 && t.TicketStatusID != 7 && t.TechID.Equals(userid) && t.ProjectType.Contains(S_Project)
                            select new ViewModel
                            {
                                Map = t.Map,
                                TicketTittle = t.TicketTitle,
                                TicketID = t.TicketID,
                                Status = db.tblTicketStatusIndexes.Where(x => x.TicketStatusID == t.TicketStatusID).FirstOrDefault().TicketStatus,
                                OpenDate = t.TicketOpen.ToString("MM/dd/yyyy"),
                                InitiatedBy = u.FirstName + " " + u.LastName,
                                AssignTo = tblUsers.Where(x => x.UserID == t.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault()
                            }).ToList();
                ViewBag.OpenTechUser = Open;
                var UserOpenPar = (from t in tblTickets
                                   join u in tblUsers
                                   on t.UserID equals u.UserID
                                   where t.TicketStatusID != 3 && t.TicketStatusID != 7 && t.UserID.Equals(userid) && t.ProjectType.Contains(S_Project)
                                   select new ViewModel
                                   {
                                       Map = t.Map,
                                       TicketTittle = t.TicketTitle,
                                       TicketID = t.TicketID,
                                       Status = db.tblTicketStatusIndexes.Where(x => x.TicketStatusID == t.TicketStatusID).FirstOrDefault().TicketStatus,
                                       OpenDate = t.TicketOpen.ToString("MM/dd/yyyy"),
                                       InitiatedBy = u.FirstName + " " + u.LastName,
                                       AssignTo = tblUsers.Where(x => x.UserID == t.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault()
                                   }).ToList();
                ViewBag.UserOpenPar = UserOpenPar;
            }
            catch (Exception)
            {
                return View("Error", "Shared");
            }
            return View();
        }

    }
}