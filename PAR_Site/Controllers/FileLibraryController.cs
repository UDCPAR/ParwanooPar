using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PAR_Site.Models;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace PAR_Site.Controllers
{

    public class FileLibraryController : Controller
    {
        // GET: FileLibrary
        NagParDemoEntities db = new NagParDemoEntities();
        public ActionResult Filelibrary()
        {
            var Allfiles = (from t in db.tblFileLibraries
                            join u in db.tblTicketIndexes
                            on t.TicketID equals u.TicketID
                            select new File_Library
                            {
                                FileName = t.FileName,
                                FileSize = t.FileSize,
                                TicketID = t.TicketID.ToString(),
                                WorkOrder = u.Map,
                                FileID = t.FileID
                            }).ToList();
            return View(Allfiles);
        }
        [HttpPost]
        public ActionResult UploadFile()
        {
            DateTime date = DateTime.Now;
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
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
                        tblFile.TicketID = Convert.ToInt32(Session["TickedID"]);
                        tblFile.UserID = Convert.ToInt32(Session["UserID"]);
                        tblFile.CustomerID = Convert.ToInt32(Session["CustomerID"]);
                        db.tblFileLibraries.Add(tblFile);
                        db.SaveChanges();
                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        public ActionResult DownloadImage(int fileid)
        {
            var image = db.tblFileLibraries.Where(x => x.FileID == fileid).Select(y => y.FileName).FirstOrDefault();
            string fullpath = Path.Combine(Server.MapPath("~/Uploads/"), image);
            Response.AddHeader("Content-Disposition", "attachment; filename="+image);
            Response.WriteFile(fullpath);
            Response.End();
            return null;
        }
        public ActionResult DeleteImage(int fileid)
        {
            var image = db.tblFileLibraries.Where(x => x.FileID == fileid).FirstOrDefault();
            var filename = image.FileName;
            db.tblFileLibraries.Remove(image);
            db.SaveChanges();
            string fullPath = Request.MapPath("~/Uploads/" + filename);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            return RedirectToAction("UpdatePar", "TicketManager", new { id = image.TicketID });
        }

        // For redirect to current view
        public ActionResult DeleteImageFile(int fileid)
        {
            var image = db.tblFileLibraries.Where(x => x.FileID == fileid).FirstOrDefault();
            var filename = image.FileName;
            db.tblFileLibraries.Remove(image);
            db.SaveChanges();
            string fullPath = Request.MapPath("~/Uploads/" + filename);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            return RedirectToAction("Filelibrary", "FileLibrary");
        }
        public ActionResult SearchPar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SearchPar(string Answer,string Subject)
        {
            try
            {
                if (Subject != null)
                {
                    List<tblUserIndex> tblUsers = db.tblUserIndexes.ToList();
                    List<tblTicketIndex> tblTickets = db.tblTicketIndexes.ToList();
                    if (Answer =="Subject" || Answer== "Description")
                    {
                        var TotalOpen = (from t in tblTickets
                                         join u in tblUsers
                                         on t.UserID equals u.UserID
                                         where (t.TicketStatusID != 3 && t.TicketStatusID != 7) && (t.TicketTitle.Contains(Subject) || t.TicketDescription.Contains(Subject))
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
                    }
                    else
                    {
                        var TotalOpen = (from t in tblTickets
                                         join u in tblUsers
                                         on t.UserID equals u.UserID
                                         where (t.TicketStatusID != 3 && t.TicketStatusID != 7) && (t.TicketID==Convert.ToInt32(Subject))
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
                    }
                    var TotalClosed = (from t in tblTickets
                                       join u in tblUsers
                                       on t.UserID equals u.UserID
                                       where (t.TicketStatusID == 3 || t.TicketStatusID == 7) && (t.TicketTitle.Contains(Subject) || t.TicketDescription.Contains(Subject) )
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
                }
            }
            catch (Exception)
            {
                return View("Error", "Shared");
            }
            return View();
        }
        public ActionResult OutPutPars()
        {
            var ParList = db.tblTicketIndexes.ToList();
            foreach (tblTicketIndex par in ParList)
            {
                var datalist = db.tblTicketDatas.Where(x => x.TicketID == par.TicketID).ToList();
                par.TicketData = datalist;
                string AllComments = "";
                foreach (var item in datalist)
                {
                    AllComments += "#" + item.UpdateDateTime +" ["+ db.tblUserIndexes.Where(x => x.UserID == item.UserID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault() + "]:- " + item.UpdateDescription + "\n";
                }
                par.CommentAtOnce = AllComments;
            }
            var list = (from d in ParList
                        select new OutPut_Pars
                        {
                            ParNumber = d.TicketID,
                            ParStatus = db.tblTicketStatusIndexes.Where(x => x.TicketStatusID == d.TicketStatusID).Select(y => y.TicketStatus).FirstOrDefault(),
                            OpenDate = d.TicketOpen,
                            ClosedDate = d.TicketClose.ToString(),
                            Initiatedby = db.tblUserIndexes.Where(x => x.UserID == d.UserID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault(),
                            AssignTo = db.tblUserIndexes.Where(x => x.UserID == d.TechID).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault(),
                            Divison = d.Issue,
                            Issue = d.Category,
                            WorkOrder = d.Map,
                            Par_Subject = d.TicketTitle,
                            Par_Description = d.TicketDescription,
                            Comments = d.CommentAtOnce
                        }).ToList();
            var gv = new GridView();
            gv.DataSource = list;
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=OutPut_Excel_"+DateTime.UtcNow.ToString("MM/dd/yyyy")+".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View();
        }
    }
}