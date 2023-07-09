using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;
using ClosedXML.Excel;

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
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Здания");

                worksheet.Cell("A1").Value = "Субъект РФ";
                worksheet.Cell("B1").Value = "Населенный пункт";
                worksheet.Cell("C1").Value = "Район";
                worksheet.Cell("D1").Value = "Улица";
                worksheet.Cell("E1").Value = "Номер";
                worksheet.Cell("F1").Value = "Литера";
                worksheet.Cell("G1").Value = "Год постройки";
                worksheet.Cell("H1").Value = "Год капремонта";
                worksheet.Cell("I1").Value = "Тип конструкции";
                worksheet.Cell("J1").Value = "Тип отопления";
                worksheet.Cell("K1").Value = "Тип планировки";
                worksheet.Cell("L1").Value = "Консьерж";
                worksheet.Cell("M1").Value = "Домофон";
                worksheet.Cell("N1").Value = "Ограждения";
                worksheet.Cell("O1").Value = "Подземная парковка";
                worksheet.Cell("P1").Value = "Детская площадка";
                worksheet.Cell("Q1").Value = "Лифт";
                worksheet.Row(1).Style.Font.Bold = true;

                int row = 2;

                foreach (var b in db.Buildings)
                {
                    worksheet.Cell(row, 1).Value = b.District.Settlement.Subject.Name;
                    worksheet.Cell(row, 2).Value = b.District.Settlement.Settlement_Type.Type + " " + b.District.Settlement.Name;
                    worksheet.Cell(row, 3).Value = b.District.Name;
                    worksheet.Cell(row, 4).Value = b.Street;
                    worksheet.Cell(row, 5).Value = b.Number;
                    worksheet.Cell(row, 6).Value = b.Litera;
                    worksheet.Cell(row, 7).Value = b.BuildYear;
                    worksheet.Cell(row, 8).Value = b.OverhaulYear;
                    worksheet.Cell(row, 9).Value = b.Construction_Type.ConstructionType;
                    worksheet.Cell(row, 10).Value = b.Heating_Type.HeatingType;
                    worksheet.Cell(row, 11).Value = b.Planning_Type.PlanningType;
                    worksheet.Cell(row, 12).Value = b.Concierge;
                    worksheet.Cell(row, 13).Value = b.Domofon;
                    worksheet.Cell(row, 14).Value = b.Fence;
                    worksheet.Cell(row, 15).Value = b.UndegroundParking;
                    worksheet.Cell(row, 16).Value = b.Playground;
                    worksheet.Cell(row, 17).Value = b.Elevator;

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"buildings_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
    }
}
