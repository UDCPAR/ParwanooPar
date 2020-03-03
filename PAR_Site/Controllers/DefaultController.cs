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
        NagParTestEntities db = new NagParTestEntities();
        
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
            var Techlist = db.tblUserIndexes.Where(x => x.AccountStatusID == 2).Select(p => new SelectListItem()
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
            int TID = Convert.ToInt32(ViewBag.ddlValue);
            var today = DateTime.Now.Date;
            var fiveday = today.AddDays(-5);
            var LessThenTen = today.AddDays(-10);
            if (TID > 0)
            {
                var beforefiveday = db.tblTicketIndexes.Where(x => x.TicketOpen >= fiveday && x.TicketStatusID != 3 && x.TicketStatusID != 6 && x.TicketStatusID != 7 && x.TechID == TID).Count();
                ViewBag.beforefiveday = beforefiveday;
                var beforeTenday = db.tblTicketIndexes.Where(x => x.TicketOpen >= LessThenTen && x.TicketOpen < fiveday && x.TicketStatusID != 3 && x.TicketStatusID != 6 && x.TicketStatusID != 7 && x.TechID == TID).Count();
                ViewBag.beforeTenday = beforeTenday;
                var total = db.tblTicketIndexes.Where(x => x.TicketClose == null && x.TicketOpen < LessThenTen && x.TicketStatusID != 3 && x.TicketStatusID != 6 && x.TicketStatusID != 7 && x.TechID == TID).Count();
                ViewBag.TotalCountForTen = total;
            }
            else
            {
                var beforefiveday = db.tblTicketIndexes.Where(x => x.TicketOpen >= fiveday && x.TicketClose == null && x.TicketStatusID != 3 && x.TicketStatusID != 6 && x.TicketStatusID != 7).Count();
                ViewBag.beforefiveday = beforefiveday;
                var beforeTenday = db.tblTicketIndexes.Where(x => x.TicketOpen >= LessThenTen && x.TicketOpen < fiveday && x.TicketClose == null && x.TicketStatusID != 3 && x.TicketStatusID != 6 && x.TicketStatusID != 7).Count();
                ViewBag.beforeTenday = beforeTenday;
                var total = db.tblTicketIndexes.Where(x => x.TicketClose == null && x.TicketOpen < LessThenTen && x.TicketStatusID != 3 && x.TicketStatusID != 6 && x.TicketStatusID != 7).Count();
                ViewBag.TotalCountForTen = total;
            }
            int userid = Convert.ToInt32(Session["UserID"]);
            ViewBag.TechParCount = db.tblTicketIndexes.Where(x => (x.TicketStatusID == 1 || x.TicketStatusID == 8 || x.TicketStatusID == 6) && x.TechID == userid).Count();
            ViewBag.UserParCount = db.tblTicketIndexes.Where(x => (x.TicketStatusID == 1 || x.TicketStatusID == 8 || x.TicketStatusID == 6) && x.UserID == userid).Count();
            return View();
        }
        public ActionResult OpenPars(int ddlValue)
        {
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
                                where t.TicketStatusID == 1 || t.TicketStatusID == 8
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
                                where t.TicketStatusID != 3 && t.TicketStatusID != 6 && t.TicketStatusID != 7 && t.TechID.Equals(ddlValue)
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
            int userid = Convert.ToInt32(Session["UserID"]);
            ViewModel viewModel = new ViewModel();
            List<tblUserIndex> tblUsers = db.tblUserIndexes.ToList();
            List<tblTicketIndex> tblTickets = db.tblTicketIndexes.ToList();
            try
            {
                var Open = (from t in tblTickets
                            join u in tblUsers
                            on t.UserID equals u.UserID
                            where t.TicketStatusID != 3 && t.TicketStatusID != 7 && t.TechID.Equals(userid)
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
                            where t.TicketStatusID != 3 && t.TicketStatusID != 7 && t.UserID.Equals(userid)
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