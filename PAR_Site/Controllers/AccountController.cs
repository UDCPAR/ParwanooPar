using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using PAR_Site.Models;


namespace PAR_Site.Controllers
{
    public class AccountController : Controller
    {
        private NagParTestEntities db = new NagParTestEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Login lg, int id = 0)
        {
            if (Session["UserName"] != null)
            {
                Session.Abandon();
                Session.RemoveAll();
                Session.Clear();
            }
            else
            {
                var obj = db.tblUserIndexes.Where(x => x.Username == lg.UserName && x.Password == lg.Password).FirstOrDefault();
                if (obj != null && lg.UserName.Equals(obj.Username) && lg.Password.Equals(obj.Password))
                {
                    if (obj.UserType == "1")
                    {
                        Session["UserType"] = "Admin";
                    }
                    var S_EXT = DateTime.UtcNow.AddMinutes(30);
                    Session["CountDownTime"] = S_EXT.ToString("ddd MMM dd yyy HH:mm:ss");
                    Session["UserName"] = obj.FirstName.ToString() + " " + obj.LastName.ToString();
                    Session["UserID"] = obj.UserID.ToString();
                    Session["CustomerID"] = obj.CustomerID.ToString();
                    Session["AccountTypeID"] = obj.AccountTypeID.ToString();
                    if (id > 0)
                    {
                        return RedirectToAction("UpdatePar", "TicketManager", new { id = id });
                    }
                    else
                        return RedirectToAction("Index", "Default");
                }
                else
                {
                    ViewBag.Message = "UserName and Password is Incorrect";
                }
            }
            return View(lg);
        }

        public JsonResult GetCurrentTime()
        {
            DateTime CTime = DateTime.UtcNow;
            string time = CTime.ToString("ddd MMM dd yyy HH:mm:ss");
            return Json(time, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Login", "Account");
        }
        public ActionResult AccountInformation()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AccountInformation(Account_Info Info)
        {
            int userid = Convert.ToInt32(Session["UserID"]);
            if (ModelState.IsValid)
            {
                var obj = db.tblUserIndexes.Where(x => x.Password == Info.OldPassword && x.UserID == userid).FirstOrDefault();
                if (obj != null)
                {
                    obj.Password = Info.Password;
                    db.SaveChanges();
                    Session.Abandon();
                    Session.Clear();
                    Session.RemoveAll();
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ViewBag.Message = "Current Password Not Match";
                }
            }
            return View();
        }
        public ActionResult SessionExpire()
        {
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
            return View();
        }
        //public JsonResult ExtendTime()
        //{
        //    Session.Timeout = 20;
        //    DateTime S_EXT = DateTime.UtcNow.AddMinutes(20);
        //    Session["IsActiveSession"] = S_EXT;
        //    Session["CountDownTime"] = S_EXT.ToString("ddd MMM dd yyy HH:mm:ss");
        //    DateTime WarningTime = S_EXT.AddMinutes(-2);
        //    Session["WarningTime"] = WarningTime.ToString("HH:mm:ss");
        //    string time = "";
        //    return Json(time, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult NewUser()
        {
            var list = new List<SelectListItem> {
                new SelectListItem { Value="1",Text="Yes"},
                new SelectListItem{ Value="2", Text="No"}
            };
            ViewBag.AccountTypeID = list;
            ViewBag.AccountStatusID = new List<SelectListItem>
            {
                new SelectListItem{ Value="2",Text="Yes"},
                 new SelectListItem{ Value="1",Text="No"}
            };
            ViewBag.UserType = new List<SelectListItem>
            {
                new SelectListItem{ Value="1",Text="Yes"},
                 new SelectListItem{ Value="2",Text="No"}
            };
            ViewBag.Userlist = db.tblUserIndexes.Where(x => x.UserType == "2").ToList();
            return View();
        }
        [HttpPost]
        public ActionResult NewUser(tblUserIndex tblUser)
        {
            try
            {
                if (tblUser.UserID > 0)
                {
                    var user = db.tblUserIndexes.Where(x => x.UserID == tblUser.UserID).FirstOrDefault();
                    user.FirstName = tblUser.FirstName;
                    user.LastName = tblUser.LastName;
                    user.Username = tblUser.Username;
                    user.Password = tblUser.Password;
                    user.EmailAddress = tblUser.EmailAddress;
                    user.AccountStatusID = tblUser.AccountStatusID;
                    user.AccountTypeID = tblUser.AccountTypeID;
                    user.UserType = tblUser.UserType;
                    db.SaveChanges();
                }
                else
                {
                    tblUserIndex userIndex = new tblUserIndex();
                    userIndex.CustomerID = 18;
                    userIndex.FirstName = tblUser.FirstName;
                    userIndex.LastName = tblUser.LastName;
                    userIndex.Username = tblUser.Username;
                    userIndex.Password = tblUser.Password;
                    userIndex.Phone = "x";
                    userIndex.EmailAddress = tblUser.EmailAddress;
                    userIndex.AccountTypeID = tblUser.AccountTypeID;
                    userIndex.AccountStatusID = tblUser.AccountStatusID;
                    userIndex.UserType = tblUser.UserType;
                    db.tblUserIndexes.Add(userIndex);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                return View("Error", "Shared");
            }
            return RedirectToAction("NewUser", "Account");
        }
        public JsonResult UserExist(string username)
        {
            string msg = "";
            var userlist = db.tblUserIndexes.Where(x => x.Username == username).FirstOrDefault();
            if (userlist != null)
            {
                msg = "Yes";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteUser(int id)
        {
            bool result = false;
            var user = db.tblUserIndexes.Where(x => x.UserID == id).FirstOrDefault();
            db.tblUserIndexes.Remove(user);
            db.SaveChanges();
            result = true;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditUser(int Id)
        {
            var data = db.tblUserIndexes.Where(x => x.UserID == Id).FirstOrDefault();
            string values = string.Empty;
            values = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(values, JsonRequestBehavior.AllowGet);
        }
    }
}