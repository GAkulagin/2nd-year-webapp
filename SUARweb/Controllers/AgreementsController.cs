using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Threading;
using SUARweb.Models;
using SUARweb.Exporters;

namespace SUARweb.Controllers
{
    public class AgreementsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: Agreements
        public ActionResult Index(string status, string apartment, string renter, string lessor, string dateSort, 
            string firstDate, string secondDate)
        {
            var agreements = db.Agreements.Include(a => a.Agreement_Status).Include(a => a.Apartment).Include(a => a.Client).Include(a => a.Pay_Frequency);

            if (!String.IsNullOrEmpty(status)) agreements = agreements.Where(a => a.Agreement_Status.Status.Contains(status));
            if (!String.IsNullOrEmpty(apartment)) agreements = agreements.Where(p => (p.Apartment.GetAdress()).Contains(apartment));
            if (!String.IsNullOrEmpty(renter)) agreements = agreements.Where(p => (p.Client.GetPassportAndFullname()).Contains(renter));
            if (!String.IsNullOrEmpty(lessor)) agreements = agreements.Where(p => (p.Apartment.Client.GetPassportAndFullname()).Contains(lessor));

            DateTime fd, sd;
            bool fdOk = DateTime.TryParse(firstDate, out fd);
            bool sdOk = DateTime.TryParse(secondDate, out sd);

            if(dateSort != "Не сортировать" && fdOk && sdOk)
            {
                if (dateSort == "Начала") agreements = agreements.Where(a => a.StartDate >= fd && a.StartDate <= sd);
                else if (dateSort == "Окончания") agreements = agreements.Where(a => a.EndDate >= fd && a.EndDate <= sd);
                else if (dateSort == "По сроку действия") agreements = agreements.Where(a => a.StartDate >= fd && a.EndDate <= sd);
            }

            ViewBag.DataSortTypes = new SelectList(new List<string>()
            {
                "Не сортировать",
                "Начала",
                "Окончания",
                "По сроку действия"
            });

            return View(agreements.ToList());
        }

        // GET: Agreements/Create
        public ActionResult Create()
        {
            var apartments = db.Apartments.Where(a => a.StatusId == RentalStatusCode.Vacant).Select(a => new
            {
                Text = a.Building.District.Settlement.Subject.Name + " ," +
                       a.Building.District.Settlement.Settlement_Type.Type + " " +
                       a.Building.District.Settlement.Name + ", " +
                       a.Building.Street + ", д. " +
                       a.Building.Number + ", кв. " + 
                       a.Number,
            Value = a.ID
            });
            var renters = db.Clients.Select(r => new
            {
                Text = r.PassportID + " " + r.Fullname,
                Value = r.PassportID
            });

            ViewBag.ApartmentId = new SelectList(apartments, "Value", "Text");
            ViewBag.RenterId = new SelectList(renters, "Value", "Text");
            ViewBag.PayFrequencyId = new SelectList(db.PayFrequency, "ID", "Frequency");
            return View();
        }

        // POST: Agreements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,StartDate,EndDate,PayFrequencyId,PaySum,RenterId,ApartmentId")] Agreement agreement)
        {
            agreement.StatusId = AgreementStatusCode.Active;

            if (ModelState.IsValid)
            {
                db.Agreements.Add(agreement);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var apartments = db.Apartments.Where(a => a.StatusId == RentalStatusCode.Vacant).Select(a => new
            {
                Text = a.Building.District.Settlement.Subject.Name + " ," +
                       a.Building.District.Settlement.Settlement_Type.Type + " " +
                       a.Building.District.Settlement.Name + ", " +
                       a.Building.Street + ", д. " +
                       a.Building.Number + ", кв. " +
                       a.Number,
                Value = a.ID
            });
            var renters = db.Clients.Select(r => new
            {
                Text = r.PassportID + " " + r.Fullname,
                Value = r.PassportID
            });

            ViewBag.ApartmentId = new SelectList(apartments, "Value", "Text", apartments.Select(a => a.Value).Where(v => v == agreement.ApartmentId).First());
            ViewBag.RenterId = new SelectList(renters, "Value", "Text", renters.Select(r => r.Value).Where(v => v == agreement.RenterId).First());
            ViewBag.PayFrequencyId = new SelectList(db.PayFrequency, "ID", "Frequency", agreement.PayFrequencyId);
            return View(agreement);
        }

        // GET: Agreements/SetStatus/5
        public ActionResult SetStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agreement agreement = db.Agreements.Find(id);
            if (agreement == null)
            {
                return HttpNotFound();
            }

            var statuses = db.AgreementStatus.Where(rs => rs.ID != AgreementStatusCode.Expired);
            ViewBag.StatusId = new SelectList(statuses, "ID", "Status", agreement.StatusId);

            return View(agreement);
        }

        // POST: Agreements/SetStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetStatus(int ID, int Status)
        {
            Agreement agreement = db.Agreements.Find(ID);
            agreement.StatusId = Status;

            if (ModelState.IsValid)
            {
                db.Entry(agreement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var statuses = db.AgreementStatus.Where(rs => rs.ID != AgreementStatusCode.Expired);
            ViewBag.StatusId = new SelectList(statuses, "ID", "Status", agreement.StatusId);

            return View(agreement);
        }

        // GET: Agreements/Prolong/5
        public ActionResult Prolong(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agreement agreement = db.Agreements.Find(id);
            if (agreement == null)
            {
                return HttpNotFound();
            }

            return View(agreement);
        }

        // POST: Agreements/Prolong/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Prolong(int ID, DateTime EndDate)
        {
            Agreement agreement = db.Agreements.Find(ID);
            if (EndDate <= agreement.EndDate) ModelState.AddModelError("EndDate", "Новая дата окончания меньше старой");

            if (ModelState.IsValid)
            {
                agreement.EndDate = EndDate;
                db.Entry(agreement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(agreement);
        }

        // GET: Agreements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Agreement agreement = db.Agreements.Find(id);
            if (agreement == null)
            {
                return HttpNotFound();
            }
            return View(agreement);
        }

        // POST: Agreements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Agreement agreement = db.Agreements.Find(id);

            try
            {
                db.Agreements.Remove(agreement);
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
            var exporter = new ExcelExporter(db.Agreements.ToList<IExportableEntity>());
            string filename = $"agreements_{DateTime.UtcNow.ToShortDateString()}.xlsx";

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

