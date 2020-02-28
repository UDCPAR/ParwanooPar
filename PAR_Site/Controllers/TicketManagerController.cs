using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PAR_Site.Models;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace PAR_Site.Controllers
{

    public class TicketManagerController : Controller
    {
        NagParTestEntities db = new NagParTestEntities();
        // GET: TicketManager
        public ActionResult CreatePars()
        {
            var Project = db.tblSubjectIndexes.Select(x => new SelectListItem
            {
                Text = x.Subject,
                Value = x.Subject
            }).ToList();
            ViewBag.ProjectType = Project;
            var ProjectPhase = db.tblProjectPhases.Select(x => new SelectListItem
            {
                Text = x.Description,
                Value = x.Description
            }).ToList();
            ViewBag.ProjectPhase = ProjectPhase;
            var Techlist = db.tblUserIndexes.Where(x => x.AccountStatusID == 2).Select(p => new SelectListItem()
            {
                Value = p.UserID.ToString(),
                Text = p.FirstName + " " + p.LastName
            }).ToList();
            ViewBag.username = Techlist;
            var item = db.tblCategories.Select(p => new SelectListItem()
            {
                Value = p.Category,
                Text = p.Category
            }).ToList();
            ViewBag.Issue = item;

            return View();
        }
        [HttpPost]
        public ActionResult CreatePars(_CreatePar par)
        {

            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            string TicketNumber = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                }
                while (TicketNumber.IndexOf(character) != -1);
                TicketNumber += character;
            }
            string TicketNum = Session["CustomerID"] + "-" + Session["UserID"] + "-" + TicketNumber;
            tblTicketIndex ticketIndex = new tblTicketIndex();
            if (ModelState.IsValid)
            {
                try
                {
                    DateTime date = DateTime.Now;
                    ticketIndex.ProjectType1 = par.ProjectType.TrimEnd();
                    ticketIndex.ProjectPhase = par.ProjectPhase;
                    ticketIndex.Location = par.Location;
                    ticketIndex.DivisionName = par.Location;
                    ticketIndex.Category = par.Issue;
                    ticketIndex.Map = par.WorkOrderNo;
                    ticketIndex.TicketTitle = par.Subject;
                    ticketIndex.TicketDescription = par.Description;
                    ViewBag.Description = par.Description;
                    ticketIndex.CustomerID = Convert.ToInt32(Session["CustomerID"]);
                    ticketIndex.UserID = Convert.ToInt32(Session["UserID"]);
                    ViewBag.Techid = Convert.ToInt32(par.TechName);
                    ticketIndex.TechID = Convert.ToInt32(par.TechName);
                    ticketIndex.TicketOpen = date;
                    ticketIndex.TicketStatusID = 1;
                    ticketIndex.OpenedBy = 1;
                    ticketIndex.TicketNumber = TicketNum;
                    db.tblTicketIndexes.Add(ticketIndex);
                    db.SaveChanges();
                    var id = (from s in db.tblTicketIndexes
                              orderby s.TicketID descending
                              select s.TicketID).ToList().FirstOrDefault();
                    ViewBag.TicketID = id;
                    TempData["TicketID"] = id;
                    Mail();
                }
                catch (Exception)
                {
                    return View("Error", "Shared");
                }
                var files = par.Files.ToList();
                //var fileCheck = par.Files[0];
                ////if (fileCheck != null)
                if (files[0] != null)
                {
                    foreach (HttpPostedFileBase file in files)
                    {
                        try
                        {
                            DateTime date = DateTime.Now;
                            string fname = file.FileName;
                            fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
                            file.SaveAs(fname);
                            tblFileLibrary tblFile = new tblFileLibrary();
                            var fileName = Path.GetFileName(file.FileName);
                            var ext = Path.GetExtension(file.FileName);
                            int byteCount = file.ContentLength;
                            tblFile.FileName = fileName;
                            tblFile.OFileName = fileName;
                            tblFile.FileExt = ext;
                            tblFile.FileSize = byteCount;
                            tblFile.UploadDateTime = date;
                            tblFile.TicketID = Convert.ToInt32(ViewBag.TicketID);
                            tblFile.UserID = Convert.ToInt32(Session["UserID"]);
                            tblFile.CustomerID = Convert.ToInt32(Session["CustomerID"]);
                            db.tblFileLibraries.Add(tblFile);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return View("Error", "Shared");
                        }
                    }

                }
            }

            return RedirectToAction("UpdatePar", "TicketManager", new { id = ViewBag.TicketID });
        }
        [HttpPost]
        public ActionResult UpdateRecord(EditViewModel editView)
        {
            try
            {
                var obj = db.tblTicketIndexes.Where(x => x.TicketID == editView.ParNum).FirstOrDefault();
                int StatusID = Convert.ToInt32(editView.ParStatus);
                obj.TicketStatusID = StatusID;
                obj.Category = editView.Issue;
                obj.Map = editView.WorkOrderNo;
                if (StatusID == 3 || StatusID == 7)
                {
                    obj.ClosedBy = 1;
                    obj.TicketClose = DateTime.Now;
                }
                else
                {
                    obj.ClosedBy = null;
                    obj.TicketClose = null;
                }
                obj.TicketTitle = editView.Subject;
                obj.TicketDescription = editView.Description;
                obj.TechID = Convert.ToInt32(editView.AssignTo);
                db.SaveChanges();
                return RedirectToAction("UpdatePar", "TicketManager", new { id = editView.ParNum });
            }
            catch (Exception)
            {
                return View("Error", "Shared");
            }

        }
        public ActionResult UpdatePar(int id)
        {

            EditViewModel model = new EditViewModel();
            try
            {
                var updatecomments = (from d in db.tblTicketDatas
                                      join u in db.tblUserIndexes
                                      on d.UserID equals u.UserID
                                      where d.TicketID == id
                                      select new All_Updates
                                      {
                                          Techname = u.FirstName + " " + u.LastName,
                                          UpdateComment = d.UpdateDescription,
                                          Date = d.UpdateDateTime
                                      }).ToList();
                ViewBag.updatecomments = updatecomments;
                var totalfiles = db.tblFileLibraries.Where(x => x.TicketID == id).ToList();
                ViewBag.totalfiles = totalfiles;
                ViewBag.totalfilescount = totalfiles.Count();
                if (id > 0)
                {
                    var par = db.tblTicketIndexes.Where(x => x.TicketID == id).FirstOrDefault();
                    model.ParNum = par.TicketID;
                    Session["TickedID"] = par.TicketID;
                    TempData["TID"] = par.TicketID;
                    model.Opened = par.TicketOpen.ToString("MM/dd/yyyy");
                    var TechID = par.UserID;
                    TempData["UserID"] = par.UserID;
                    var TName = db.tblUserIndexes.Where(x => x.UserID == TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
                    model.InitiatedBy = TName;
                    var Techid = par.TechID;
                    TempData["TechID"] = par.TechID;
                    ViewBag.AssignName = db.tblUserIndexes.Where(x => x.AccountStatusID == 2).Select(p => new SelectListItem()
                    {
                        Value = p.UserID.ToString(),
                        Text = p.FirstName + " " + p.LastName
                    }).ToList();
                    ViewBag.selectedval = db.tblTicketIndexes.Where(x => x.TechID == par.TechID).Select(y => y.TechID).FirstOrDefault();
                    model.Divison = par.Location;
                    model.Issue = par.Category;
                    ViewBag.IssueText = par.Category.Trim();
                    var item = db.tblCategories.Select(p => new SelectListItem()
                    {
                        Value = p.Category.Trim(),
                        Text = p.Category.Trim()
                    }).ToList();
                    ViewBag.Issue = item;
                    model.WorkOrderNo = par.Map;
                    ViewBag.selectedvalue = par.TicketStatusID;
                    model.Subject = par.TicketTitle;
                    model.Description = par.TicketDescription;
                    model.ClosedDate = par.TicketClose.ToString();
                    Session["TicketNum"] = par.TicketNumber;
                    int AID = Convert.ToInt32(Session["AccountTypeID"]);
                    if (AID == 1)
                    {
                        var AccountId = db.tblTicketStatusIndexes.Select(y => new SelectListItem
                        {
                            Value = y.TicketStatusID.ToString(),
                            Text = y.TicketStatus,
                        }).ToList();
                        ViewBag.AccountId = AccountId;
                    }
                    else
                    {
                        if (par.TicketStatusID == 3)
                        {
                            var AccountId = db.tblTicketStatusIndexes.Where(x => x.UserAccountTypeID == 2 || x.StatusOrder == 4).Select(y => new SelectListItem
                            {
                                Value = y.TicketStatusID.ToString(),
                                Text = y.TicketStatus,
                            }).ToList();
                            ViewBag.AccountId = AccountId;
                        }
                        else if (par.TicketStatusID == 7)
                        {
                            var AccountId = db.tblTicketStatusIndexes.Where(x => x.UserAccountTypeID == 2 || x.StatusOrder == 5).Select(y => new SelectListItem
                            {
                                Value = y.TicketStatusID.ToString(),
                                Text = y.TicketStatus,
                            }).ToList();
                            ViewBag.AccountId = AccountId;
                        }
                        else
                        {
                            var AccountId = db.tblTicketStatusIndexes.Where(x => x.UserAccountTypeID == 2).Select(y => new SelectListItem
                            {
                                Value = y.TicketStatusID.ToString(),
                                Text = y.TicketStatus,
                            }).ToList();
                            ViewBag.AccountId = AccountId;
                        }
                    }

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "Default");
                }
            }
            catch (Exception)
            {
                return View("Error", "Shared");
            }
        }
        [HttpPost]
        public ActionResult UpdateNewRecord(EditViewModel viewModel)
        {
            DateTime date = DateTime.Now;
            tblTicketData ticketData = new tblTicketData();
            tblTicketIndex ticketIndex = new tblTicketIndex();
            int ddlvalue = Convert.ToInt32(Request["ParStatus"]);
            int TechID = Convert.ToInt32(Request["AssignTo"]);
            int ticketid = Convert.ToInt32(Session["TickedID"]);
            try
            {
                if (ddlvalue == 3 || ddlvalue == 7)
                {
                    var obj = db.tblTicketIndexes.Where(x => x.TicketID == ticketid).FirstOrDefault();
                    obj.TicketClose = date;
                    obj.ClosedBy = Convert.ToInt32(Session["UserID"]);
                    obj.TicketStatusID = ddlvalue;
                    db.SaveChanges();
                }
                if (ddlvalue == 1 || ddlvalue == 6 || ddlvalue == 8)
                {
                    var obj = db.tblTicketIndexes.Where(x => x.TicketID == ticketid).FirstOrDefault();
                    obj.TicketClose = null;
                    obj.ClosedBy = null;
                    obj.TicketStatusID = ddlvalue;
                    db.SaveChanges();
                }
                if (TechID > 0)
                {
                    var obj = db.tblTicketIndexes.Where(x => x.TicketID == ticketid).FirstOrDefault();
                    obj.TechID = TechID;
                    db.SaveChanges();
                }
                if (viewModel.UpdateDiscription != null && viewModel.UpdateDiscription.Trim().Length > 0)
                {
                    ViewBag.UpdateDiscription = viewModel.UpdateDiscription;
                    ticketData.UpdateDescription = viewModel.UpdateDiscription;
                    ticketData.UpdateDateTime = date;
                    ticketData.TicketID = Convert.ToInt32(Session["TickedID"]);
                    TempData["TicketID"] = Convert.ToInt32(Session["TickedID"]);
                    ticketData.TicketNumber = Session["TicketNum"].ToString();
                    ticketData.UserID = Convert.ToInt32(Session["UserID"]);
                    ticketData.TechID = Convert.ToInt32(TempData["TechID"]);
                    ticketData.CustomerID = Convert.ToInt32(Session["CustomerID"]);
                    ticketData.UpdatedBy = 1;
                    db.tblTicketDatas.Add(ticketData);
                    db.SaveChanges();
                    Mail();
                    return RedirectToAction("UpdatePar", "TicketManager", new { id = ticketData.TicketID });
                }
            }
            catch (Exception)
            {
                return View("Error", "Shared");
                //throw ex;
            }
            //return RedirectToAction("Index", "Default");
            return RedirectToAction("UpdatePar", "TicketManager", new { id = viewModel.ParNum });
        }

        public ActionResult ViewTotalPars()
        {
            List<tblUserIndex> tblUsers = db.tblUserIndexes.ToList();
            List<tblTicketIndex> tblTickets = db.tblTicketIndexes.ToList();
            var TotalOpen = (from t in tblTickets
                             join u in tblUsers
                             on t.UserID equals u.UserID
                             where t.TicketStatusID != 3 && t.TicketStatusID != 7
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
            ViewBag.TotalOpen = TotalOpen;
            var TotalClosed = (from t in tblTickets
                               join u in tblUsers
                               on t.UserID equals u.UserID
                               where t.TicketStatusID == 3 || t.TicketStatusID == 7
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
            ViewBag.TotalClosed = TotalClosed;
            return View();
        }
        public void Mail()
        {
            string to = "";
            string GreatingName = "";
            int TicketID = Convert.ToInt32(TempData["TicketID"]);
            List<tblUserIndex> tblUsers = db.tblUserIndexes.ToList();
            var obj = db.tblTicketIndexes.Where(x => x.TicketID == TicketID).FirstOrDefault();
            int loggedUserID = Convert.ToInt32(Session["UserID"]);
            if (loggedUserID == obj.UserID)
            {
                to = tblUsers.Where(x => x.UserID == obj.TechID).Select(y => y.EmailAddress).FirstOrDefault();
                GreatingName = tblUsers.Where(x => x.UserID == obj.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
            }
            else if (loggedUserID == obj.TechID)
            {
                to = tblUsers.Where(x => x.UserID == obj.UserID).Select(y => y.EmailAddress).FirstOrDefault();
                GreatingName = tblUsers.Where(x => x.UserID == obj.UserID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
            }
            else
            {
                to = tblUsers.Where(x => x.UserID == obj.UserID).Select(y => y.EmailAddress).FirstOrDefault() + "," + tblUsers.Where(x => x.UserID == obj.TechID).Select(y => y.EmailAddress).FirstOrDefault();
            }
            var comment = db.tblTicketDatas.Where(x => x.TicketID == TicketID).ToList();
            string Initiatedby = tblUsers.Where(x => x.UserID == obj.UserID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
            string AssignName = tblUsers.Where(x => x.UserID == obj.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault();
            string Status = db.tblTicketStatusIndexes.Where(x => x.TicketStatusID == obj.TicketStatusID).FirstOrDefault().TicketStatus;
            string from = "";
            MailMessage message = new MailMessage(from, to);
            string HTML = "";
            HTML = HTML + "<HTML>";
            HTML = HTML + "<body>";
            HTML = HTML + "<table width='75%' cellpadding=" + 1 + " cellspacing=" + 0 + " border=" + 0 + ">";
            HTML = HTML + "<tr>";
            string MyLink = $"";
            HTML = HTML + "<td width=" + 0 + " class='linkcolor-green'><img src=" + '"' + "https://clients.udcus.com/Northwest_Natural_Par/images/UDCLogo.png" + '"' + "></td>";
            HTML = HTML + "<td width=" + 0 + " align='right' class='linkcolor-green'><img src=" + '"' + "https://localhost:44354/Images/logo-pge.gif" + '"' + " height=" + 65 + "></td>";
            HTML = HTML + "</tr>";
            HTML = HTML + "</table>";
            HTML = HTML + "<hr style='width: 75%;float: left;'>";
            HTML = HTML + "<Div style='margin-left:0%;text-align:justify; text-justify: inter-word'>";
            HTML = HTML + "<p>Hello <strong>" + GreatingName + "</strong>,</p>";
            HTML = HTML + "<p style='line-height: 1;' >Nag PAR Requesting your Review</p></br>";
            HTML = HTML + "<p><strong>Visit: http://www.parwanopar.com/Account/Login/"+obj.TicketID;
            HTML = HTML + "<h5 align='center' style='margin-right:25%;'>---(Please DO NOT reply to this email/Use the PAR system for communications regarding this PAR)---</h5>";
            HTML = HTML + "<table width='75%' height: auto bgcolor='#ffffff' border=" + 1 + " cellspacing=" + 1 + " cellpadding=" + 3 + " style='Allign:center'>";
            HTML = HTML + "<tbody>";

            HTML = HTML + "<tr>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#cccccc'><strong style='float: right'>PAR # :</strong></td>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#f0f0f0'>" + obj.TicketID + "</td>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>PAR Status :</strong></td>";
            HTML = HTML + "<td width='17%' class='_x2' bgcolor='#f0f0f0'>" + Status + "</td>";
            HTML = HTML + "</tr>";

            HTML = HTML + "<tr>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Opened :</strong></td>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#f0f0f0'>" + obj.TicketOpen + "</td>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Closed :</strong></td>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#f0f0f0'>" + obj.TicketClose + "</td>";
            HTML = HTML + "</tr>";

            HTML = HTML + "<tr>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Initiated By :</strong></td>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#f0f0f0'><strong>" + Initiatedby + "</strong></td>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Assigned To :</strong></td>";
            HTML = HTML + "<td width='17%' class='_x2' bgcolor='#f0f0f0'><strong>" + AssignName + "</strong></td>";
            HTML = HTML + "</tr>";

            HTML = HTML + "<tr>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Project Type :</strong></td>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#f0f0f0'>" + obj.ProjectType1 + "</td>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style='float: right'>Location :</strong></td>";
            HTML = HTML + "<td width='17%' class='_x2' bgcolor='#f0f0f0'>" + obj.Location + "</td>";
            HTML = HTML + "</tr>";
            HTML = HTML + "<tr>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Issue :</strong></td>";
            HTML = HTML + "<td width='15%' class='_x2' bgcolor='#f0f0f0'>" + obj.Category + "</td>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style='float: right'>Work Order No :</strong></td>";
            HTML = HTML + "<td width='17%' class='_x2' bgcolor='#f0f0f0'>" + obj.Map + "</td>";
            HTML = HTML + "</tr>";

            HTML = HTML + "<tr>";
            HTML = HTML + "<td width='10%' class='_x2' bgcolor='#cccccc'><strong style=' float: right'>Subject :</strong></td>";
            HTML = HTML + "<td width='25%' class='_x2' bgcolor='#f0f0f0' colspan=" + 3 + ">" + obj.TicketTitle + "</td>";
            HTML = HTML + "</tr>";

            HTML = HTML + "<tr>";
            HTML = HTML + "<td valign='top' width='10%' class='_x2' bgcolor='#cccccc'><strong style='float: right'>Description :</strong></td>";
            HTML = HTML + "<td width='25%' class='_x2' bgcolor='#f0f0f0' colspan=" + 3 + ">" + obj.TicketDescription + "</td>";
            HTML = HTML + "</tr>";
            string AllComments = "";
            if (comment.Count > 0)
            {
                HTML = HTML + "<tr>";
                HTML = HTML + "<td valign='top' width='10%' class='_x2' bgcolor='#cccccc'><strong style='float: right'>All Comments :</strong></td>";
                foreach (var item in comment)
                {
                    AllComments += "&#8986;" + "<strong>" + item.UpdateDateTime + " : " + db.tblUserIndexes.Where(x => x.UserID == item.UserID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault() + "</strong><br><p style=' text-align:justify; text-justify: inter-word;padding-left: 25px;line-height:auto; margin-top:0; margin-bottom:0;vertical-align: top;'>" + item.UpdateDescription + "</p>";
                }
                if (AllComments != null)
                {
                    AllComments = AllComments + "<br/>";
                    HTML = HTML + "<td width='25%'  colspan=" + 3 + ">" + AllComments + "</td>";
                    HTML = HTML + "</tr><br><br>";
                }
            }
            HTML = HTML + "</tbody>";
            HTML = HTML + "</table>";
            HTML = HTML + "</table>";
            HTML = HTML + "<p><br><strong>-UDC PAR System</strong><p2> <br> <br>";
            HTML = HTML + "<h5 align='center' style='margin-right:25%;'>---(Please DO NOT reply to this email/Use the PAR system for communications regarding this PAR)---</h5>";
            HTML = HTML + "</div>";
            HTML = HTML + "</body>";
            HTML = HTML + "</html>";

            message.Subject = "PAR Update: Subject " + obj.TicketTitle;
            message.IsBodyHtml = true;
            message.Body = HTML;
            SmtpClient smtp = new SmtpClient("smtp.office365.com", 25);
            System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential(from, "");
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = networkCredential;
            try
            {
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}