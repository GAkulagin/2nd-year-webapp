using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using SUARweb.Exporters;
using SUARweb.Models;

namespace SUARweb.Controllers
{
    public class BuildingsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: Buildings
        public ActionResult Index(string subject, string settlement, string district, string street,
            int? firstYear, int? secondYear)
        {
            var buildings = db.Buildings.Include(b => b.Construction_Type).Include(b => b.District).Include(b => b.Heating_Type).Include(b => b.Planning_Type);

            if (!String.IsNullOrEmpty(subject)) buildings = buildings.Where(p => p.District.Settlement.Subject.Name.Contains(subject));
            if (!String.IsNullOrEmpty(settlement)) buildings = buildings.Where(p => p.District.Settlement.Name.Contains(settlement));
            if (!String.IsNullOrEmpty(district)) buildings = buildings.Where(p => p.District.Name.Contains(district));
            if (!String.IsNullOrEmpty(street)) buildings = buildings.Where(p => p.Street.Contains(street));

            if(firstYear != null && secondYear != null) buildings = buildings.Where(b => b.BuildYear >= firstYear && b.BuildYear <= secondYear);

            return View(buildings.ToList());
        }

        // GET: Buildings/Create
        public ActionResult Create()
        {
            ViewBag.ConstructionTypeId = new SelectList(db.ConstructionType, "ID", "ConstructionType");
            ViewBag.HeatingTypeId = new SelectList(db.HeatingType, "ID", "HeatingType");
            ViewBag.PlanningTypeId = new SelectList(db.PlanningType, "ID", "PlanningType");

            var districts = db.Districts
                .Select(d => new
                {
                    Text = d.Settlement.Subject.Name + ", " + d.Settlement.Name + ", " + d.Name,
                    Value = d.ID
                }).ToList();
            ViewBag.DistrictId = new SelectList(districts, "Value", "Text");

            return View();
        }

        // POST: Buildings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Street,Number,Litera,DistrictId,PlanningTypeId,ConstructionTypeId,BuildYear,OverhaulYear,FloorCount,Concierge,Domofon,Fence,UndegroundParking,Playground,Elevator,HeatingTypeId")] Building building)
        {
            if(building.Number <= 0) ModelState.AddModelError("Number", "Неверный ввод данных");
            if (building.FloorCount <= 0) ModelState.AddModelError("FloorCount", "Неверный ввод данных");
            if(building.Litera != null && building.Litera.Length > 1) ModelState.AddModelError("Litera", "Введено более одного символа");

            if (building.BuildYear > DateTime.Today.Year || building.BuildYear < 0) ModelState.AddModelError("BuildYear", "Неверный ввод данных");

            if (building.OverhaulYear != null)
            {
                    if (building.OverhaulYear > DateTime.Today.Year) ModelState.AddModelError("OverhaulYear", "Неверный ввод данных");
                    if (building.OverhaulYear < building.BuildYear) ModelState.AddModelError("OverhaulYear", "Год капремонта не может быть меньше года постройки");
            }
            else building.OverhaulYear = 0;

            if (building.Litera == null) building.Litera = "";

            if (ModelState.IsValid)
            {
                db.Buildings.Add(building);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var districts = db.Districts
                .Select(d => new
                {
                    Text = d.Settlement.Subject.Name + " " + d.Settlement.Name + " " + d.Name,
                    Value = d.ID
                }).ToList();

            ViewBag.ConstructionTypeId = new SelectList(db.ConstructionType, "ID", "ConstructionType", building.ConstructionTypeId);
            ViewBag.DistrictId = new SelectList(districts, "Value", "Text", districts.Select(d => d.Value).Where(v => v == building.DistrictId).First());
            ViewBag.HeatingTypeId = new SelectList(db.HeatingType, "ID", "HeatingType", building.HeatingTypeId);
            ViewBag.PlanningTypeId = new SelectList(db.PlanningType, "ID", "PlanningType", building.PlanningTypeId);

            return View(building);
        }

        // GET: Buildings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            ViewBag.ConstructionTypeId = new SelectList(db.ConstructionType, "ID", "ConstructionType", building.ConstructionTypeId);
            ViewBag.HeatingTypeId = new SelectList(db.HeatingType, "ID", "HeatingType", building.HeatingTypeId);
            ViewBag.PlanningTypeId = new SelectList(db.PlanningType, "ID", "PlanningType", building.PlanningTypeId);
            var districts = db.Districts
                .Select(d => new
                {
                    Text = d.Settlement.Subject.Name + " " + d.Settlement.Name + " " + d.Name,
                    Value = d.ID
                });
            ViewBag.DistrictId = new SelectList(districts, "Value", "Text", districts.Select(d => d.Value).Where(v => v == building.DistrictId).First());

            return View(building);
        }

        // POST: Buildings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Street,Number,Litera,DistrictId,PlanningTypeId,ConstructionTypeId,BuildYear,OverhaulYear,FloorCount,Concierge,Domofon,Fence,UndegroundParking,Playground,Elevator,HeatingTypeId")] Building building)
        {
            if (building.Number <= 0) ModelState.AddModelError("Number", "Неверный ввод данных");
            if (building.FloorCount <= 0) ModelState.AddModelError("FloorCount", "Неверный ввод данных");
            if (building.Litera != null && building.Litera.Length > 1) ModelState.AddModelError("Litera", "Введено более одного символа");

            if (building.BuildYear > DateTime.Today.Year || building.BuildYear < 0) ModelState.AddModelError("BuildYear", "Неверный ввод данных");

            if (building.OverhaulYear != 0)
            {
                    if (building.OverhaulYear > DateTime.Today.Year) ModelState.AddModelError("OverhaulYear", "Неверный ввод данных");
                    if (building.OverhaulYear < building.BuildYear) ModelState.AddModelError("OverhaulYear", "Год капремонта не может быть меньше года постройки");
            }

            if (String.IsNullOrEmpty(building.Litera)) building.Litera = " ";

            if (ModelState.IsValid)
            {
                db.Entry(building).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ConstructionTypeId = new SelectList(db.ConstructionType, "ID", "ConstructionType", building.ConstructionTypeId);
            ViewBag.HeatingTypeId = new SelectList(db.HeatingType, "ID", "HeatingType", building.HeatingTypeId);
            ViewBag.PlanningTypeId = new SelectList(db.PlanningType, "ID", "PlanningType", building.PlanningTypeId);

            var districts = db.Districts
                .Select(d => new
                {
                    Text = d.Settlement.Subject.Name + " " + d.Settlement.Name + " " + d.Name,
                    Value = d.ID
                });
            ViewBag.DistrictId = new SelectList(districts, "Value", "Text", districts.Select(d => d.Value).Where(v => v == building.DistrictId).First());

            return View(building);
        }

        // GET: Buildings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Building building = db.Buildings.Find(id);
            try
            {
                db.Buildings.Remove(building);
                db.SaveChanges();
            }
            catch
            {
                Thread.Sleep(2000);
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }   


        public ActionResult ExportToExcel()
        {
            var exporter = new ExcelExporter(db.Buildings.ToList<IExportableEntity>());
            string filename = $"buildings_{DateTime.UtcNow.ToShortDateString()}.xlsx";

            using (var stream = exporter.ExportToMemoryStream())
            {
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = filename
                };
            }
        }
    }
}
