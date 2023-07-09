using System;
using System.Linq;
using System.Web.Mvc;

namespace SUARweb.Controllers
{
    public class HomeController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        public ActionResult Index()
        {
            var agreements = db.Agreements.Where(a => a.StatusId == 1); // действующие договоры

            foreach(var a in agreements)
            {
                if (a.EndDate.Date <= DateTime.Today) a.StatusId = 2; // меняем на истек
            }
            db.SaveChanges();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}