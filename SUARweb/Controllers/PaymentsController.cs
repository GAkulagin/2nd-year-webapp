using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClosedXML.Excel;

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
            if (payment.Sum < 0) ModelState.AddModelError("Sum", "Неверный ввод данных");

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
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Платежи");

                worksheet.Cell("A1").Value = "Код договора";
                worksheet.Cell("B1").Value = "Плательщик";
                worksheet.Cell("C1").Value = "Получатель";
                worksheet.Cell("D1").Value = "Сумма";
                worksheet.Cell("E1").Value = "Дата и время";

                worksheet.Row(1).Style.Font.Bold = true;

                int row = 2;

                foreach (var b in db.Payments)
                {
                    worksheet.Cell(row, 1).Value = b.AgreementId;
                    worksheet.Cell(row, 2).Value = b.Agreement.Client.GetPassportAndFullname();
                    worksheet.Cell(row, 3).Value = b.Agreement.Apartment.Client.GetPassportAndFullname();
                    worksheet.Cell(row, 4).Value = b.Sum;
                    worksheet.Cell(row, 5).Value = b.DateAndTime;

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"payments_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
    }
}
