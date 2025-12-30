using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    public class SearchController : Controller
    {
        
        public ActionResult Index(string query)
        {
            ViewBag.Query = query; 
            return View();
            
        }
    }
}