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
    public class ClientsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: Clients
        public ActionResult Index(string passport, string fullname, string phone,
            string inn, string kpp, string bik, string bankacc)
        {
            var clients = db.Clients.Include(c => c.Gender);

            if (!String.IsNullOrEmpty(passport)) clients = clients.Where(p => p.PassportID.Contains(passport));
            if (!String.IsNullOrEmpty(fullname)) clients = clients.Where(p => p.Fullname.Contains(fullname));
            if (!String.IsNullOrEmpty(phone)) clients = clients.Where(p => p.Phone.Contains(phone));
            if (!String.IsNullOrEmpty(inn)) clients = clients.Where(p => p.INN.Contains(inn));
            if (!String.IsNullOrEmpty(kpp)) clients = clients.Where(p => p.KPP.Contains(kpp));
            if (!String.IsNullOrEmpty(bik)) clients = clients.Where(p => p.BIK.Contains(bik));
            if (!String.IsNullOrEmpty(bankacc)) clients = clients.Where(p => p.BankAccount.Contains(bankacc));

            return View(clients.ToList());
        }


        // GET: Clients/Create
        public ActionResult Create()
        {
            ViewBag.GenderId = new SelectList(db.Genders, "ID", "Value");
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PassportID,Fullname,RegistrationAddress,Birthdate,GenderId,Citizenship,UFMScode,PassportDate,INN,KPP,BIK,BankAccount,Phone,Email")] Client client)
        {
            if(!CheckPassportID(client.PassportID))
                ModelState.AddModelError("PassportID", "Клиент с таким паспортом уже есть в базе данных");

            if (!CheckBirthDate(client.Birthdate))
                ModelState.AddModelError("Birthdate", "Клиент должен быть совершеннолетним");

            if (!CheckPassportDate(client.Birthdate, client.PassportDate))
                ModelState.AddModelError("PassportDate", "Срок действия паспорта клиента истек, либо дата введена неверно");

            if (ModelState.IsValid)
            {
                db.Clients.Add(client);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GenderId = new SelectList(db.Genders, "ID", "Value", client.Gender);
            return View(client);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            ViewBag.GenderId = new SelectList(db.Genders, "ID", "Value", client.Gender);
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PassportID,Fullname,RegistrationAddress,Birthdate,GenderId,Citizenship,UFMScode,PassportDate,INN,KPP,BIK,BankAccount,Phone,Email")] Client client)
        {

            if (!CheckBirthDate(client.Birthdate))
                ModelState.AddModelError("Birthdate", "Клиент должен быть совершеннолетним");

            if (!CheckPassportDate(client.Birthdate, client.PassportDate))
                ModelState.AddModelError("PassportDate", "Срок действия паспорта клиента истек, либо дата введена неверно");

            if (ModelState.IsValid)
            {
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GenderId = new SelectList(db.Genders, "ID", "Value", client.Gender);
            return View(client);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Client client = db.Clients.Find(id);
            try
            {
                db.Clients.Remove(client);
                db.SaveChanges();
            }
            catch
            {
                Thread.Sleep(2000);
            }
            return RedirectToAction("Index");
        }

        public ActionResult ExportToExcel()
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Клиенты");

                worksheet.Cell("A1").Value = "ФИО";
                worksheet.Cell("B1").Value = "Дата рождения";
                worksheet.Cell("C1").Value = "Пол";
                worksheet.Cell("D1").Value = "Гражданство";
                worksheet.Cell("E1").Value = "Адрес регистрации";
                worksheet.Cell("F1").Value = "Серия и номер паспорта";
                worksheet.Cell("G1").Value = "Дата выдачи паспорта";
                worksheet.Cell("H1").Value = "Код УФМС";
                worksheet.Cell("I1").Value = "ИНН";
                worksheet.Cell("J1").Value = "КПП";
                worksheet.Cell("K1").Value = "БИК";
                worksheet.Cell("L1").Value = "Расчетный счет";
                worksheet.Cell("M1").Value = "Телефон";
                worksheet.Cell("N1").Value = "Электронная почта";
                worksheet.Row(1).Style.Font.Bold = true;

                int col = 1;
                int row = 2;

                foreach(var c in db.Clients)
                {
                    worksheet.Cell(row, col).Value = c.Fullname;
                    worksheet.Cell(row, col + 1).Value = c.Birthdate.ToString("d");
                    worksheet.Cell(row, col + 2).Value = c.Gender.Value;
                    worksheet.Cell(row, col + 3).Value = c.Citizenship;
                    worksheet.Cell(row, col + 4).Value = c.RegistrationAddress;
                    worksheet.Cell(row, col + 5).Value = c.PassportID;
                    worksheet.Cell(row, col + 6).Value = c.PassportDate.ToString("d"); ;
                    worksheet.Cell(row, col + 7).Value = c.UFMScode;
                    worksheet.Cell(row, col + 8).Value = c.INN;
                    worksheet.Cell(row, col + 9).Value = c.KPP;
                    worksheet.Cell(row, col + 10).Value = c.BIK;
                    worksheet.Cell(row, col + 11).Value = c.BankAccount;
                    worksheet.Cell(row, col + 12).Value = c.Phone;
                    worksheet.Cell(row, col + 13).Value = c.Email;

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"clients_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        private bool CheckBirthDate(DateTime date)
        {
            var age = DateTime.Today.Year - date.Year;
            if (date > DateTime.Today.AddYears(-age)) age--;

            return age >= 18;
        }

        private bool CheckPassportDate(DateTime birthdate, DateTime passportDate)
        {
            var today = DateTime.Today;

            return !(passportDate < birthdate || passportDate > today);
        }

        private bool CheckPassportID(string passport)
        {
            bool ok = true;

            foreach(var c in db.Clients)
            {
                if(c.PassportID == passport)
                {
                    ok = false;
                    break;
                }
            }

            return ok;
        }
    }
}
