using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SUARweb.Exporters;
using SUARweb.Models;

namespace SUARweb.Controllers
{
    public class PaymentsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: Payments
        public ActionResult Index(string renter, string lessor, int? agreement)
        {
            var payments = db.Payments.Include(p => p.Agreement);

            if (!String.IsNullOrEmpty(renter)) payments = payments.Where(p =>
             p.Agreement.Client.GetPassportAndFullname().Contains(renter));

            if (!String.IsNullOrEmpty(lessor)) payments = payments.Where(p =>
             p.Agreement.Apartment.Client.GetPassportAndFullname().Contains(lessor));

            if (agreement != null) payments = payments.Where(p => p.AgreementId == agreement);

            return View(payments.ToList());
        }

        // GET: Payments/Create
        public ActionResult Create(int? agreementID)
        {
            if (agreementID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = new Payment();
            payment.AgreementId = (int)agreementID;

            return View(payment);
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Sum,DateAndTime,AgreementId")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.DateAndTime = DateTime.Now;
                db.Payments.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payments.Find(id);
            db.Payments.Remove(payment);
            db.SaveChanges();
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
            var exporter = new ExcelExporter(db.Payments.ToList<IExportableEntity>());
            string filename = $"payments_{DateTime.UtcNow.ToShortDateString()}.xlsx";

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
