using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SUARweb.Controllers
{
    public class SettlementsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: Settlements
        public ActionResult Index(string subject, string settlement)
        {
            var settlements = db.Settlements.Include(s => s.Settlement_Type).Include(s => s.Subject);

            if (!String.IsNullOrEmpty(subject)) settlements = settlements.Where(s => s.Subject.Name.Contains(subject));
            if (!String.IsNullOrEmpty(settlement)) settlements = settlements.Where(s => (s.Settlement_Type.Type + " " + s.Name).Contains(settlement));

            return View(settlements.ToList());
        }

        // GET: Settlements/Create
        public ActionResult Create()
        {
            ViewBag.TypeId = new SelectList(db.SettlementType, "ID", "Type");
            ViewBag.SubjectId = new SelectList(db.Subjects, "ID", "Name");
            return View();
        }

        // POST: Settlements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,TypeId,SubjectId")] Settlement settlement, string districtList)
        {
            char[] sep = { '\n' };
            string[] districts = new string[1];

            if (!String.IsNullOrEmpty(districtList)) districts = districtList.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            else districts[0] = String.Format("нет районов ({0})", settlement.Name);

            if (ModelState.IsValid)
            {
                db.Settlements.Add(settlement);
                db.SaveChanges();

                foreach(string str in districts)
                {
                    db.Districts.Add(new District { Name = str, SettlementId = settlement.ID });
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.TypeId = new SelectList(db.SettlementType, "ID", "Type", settlement.TypeId);
            ViewBag.SubjectId = new SelectList(db.Subjects, "ID", "Name", settlement.SubjectId);
            return View(settlement);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
