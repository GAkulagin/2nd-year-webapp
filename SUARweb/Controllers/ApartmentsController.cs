using System;
using System.Collections.Generic;
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
    public class ApartmentsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: Apartments
        public ActionResult Index(string adress, string lessor, int? status, int? roomCount)
        {
            var apartments = db.Apartments.Include(a => a.Balcony_Type).Include(a => a.Building).Include(a => a.Client).Include(a => a.Internet_Conn).Include(a => a.Rental_Status).Include(a => a.Television);

            if (!String.IsNullOrEmpty(adress)) apartments = apartments.Where(a => a.GetAdress().Contains(adress));

            if (!String.IsNullOrEmpty(lessor)) apartments = apartments.Where(p => p.Client.GetPassportAndFullname().Contains(lessor));

            if (status != null && status != 0) 
                apartments = apartments.Where(p => p.Rental_Status.ID == status);

            if (roomCount != null) apartments = apartments.Where(a => a.RoomCount == roomCount);

            List<Rental_Status> statusTypes = db.RentalStatus.ToList();
            statusTypes.Insert(0, new Rental_Status { ID = 0, Status = "Все" });
            ViewBag.StatusTypes = new SelectList(statusTypes, "Id", "Status");

            return View(apartments.ToList());
        }


        // GET: Apartments/Create
        public ActionResult Create()
        {
            var adresses = db.Buildings.Select(a => new
            {
                Text = a.District.Settlement.Subject.Name + " ," +
                       a.District.Settlement.Settlement_Type.Type + " " +
                       a.District.Settlement.Name + ", " +
                       a.Street + ", д. " +
                       a.Number,
                Value = a.ID
            });
            var lessors = db.Clients.Select(c => new
            {
                Text = c.PassportID + " " + c.Fullname,
                Value = c.PassportID
            });
            var statuses = db.RentalStatus.ToList();
            statuses.RemoveAt(1); // убрать "Сдается"

            ViewBag.BalconyTypeId = new SelectList(db.BalconyType, "ID", "BalconyType");
            ViewBag.BuildingId = new SelectList(adresses, "Value", "Text");
            ViewBag.LessorId = new SelectList(lessors, "Value", "Text");
            ViewBag.InternetConnTypeId = new SelectList(db.InternetConn, "ID", "ConnectionType");
            ViewBag.StatusId = new SelectList(statuses, "ID", "Status");
            ViewBag.TvTypeId = new SelectList(db.Televisions, "ID", "TVtype");
            return View();
        }

        // POST: Apartments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,StatusId,Number,RoomCount,TotalArea,LivingArea,BalconyTypeId,Fridge,Stove,WashMachine,AirConditioner,InternetConnTypeId,TvTypeId,WithPets,WithChildren,ForEvents,BuildingId,LessorId")] Apartment apartment)
        {
            if(apartment.Number < 1) ModelState.AddModelError("Number", "Неверный ввод данных");
            if (apartment.RoomCount < 1) ModelState.AddModelError("RoomCount", "Неверный ввод данных");

            if (ModelState.IsValid)
            {
                db.Apartments.Add(apartment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var adresses = db.Buildings.Select(a => new
            {
                Text = a.District.Settlement.Subject.Name + " ," +
                       a.District.Settlement.Settlement_Type.Type + " " +
                       a.District.Settlement.Name + ", " +
                       a.Street + ", д. " +
                       a.Number,
                Value = a.ID
            });
            var lessors = db.Clients.Select(c => new
            {
                Text = c.PassportID + " " + c.Fullname,
                Value = c.PassportID
            });
            var statuses = db.RentalStatus.ToList();
            statuses.RemoveAt(1); // убрать "Сдается"

            ViewBag.BalconyTypeId = new SelectList(db.BalconyType, "ID", "BalconyType", apartment.BalconyTypeId);
            ViewBag.BuildingId = new SelectList(adresses, "Value", "Text", adresses.Select(a => a.Value).Where(v => v == apartment.BuildingId).First());
            ViewBag.LessorId = new SelectList(lessors, "Value", "Text", lessors.Select(l => l.Value).Where(v => v == apartment.LessorId).First());
            ViewBag.InternetConnTypeId = new SelectList(db.InternetConn, "ID", "ConnectionType", apartment.InternetConnTypeId);
            ViewBag.StatusId = new SelectList(statuses, "ID", "Status", apartment.StatusId);
            ViewBag.TvTypeId = new SelectList(db.Televisions, "ID", "TVtype", apartment.TvTypeId);
            return View(apartment);
        }

        // GET: Apartments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }

            var adresses = db.Buildings.Select(a => new
            {
                Text = a.District.Settlement.Subject.Name + " ," +
                       a.District.Settlement.Settlement_Type.Type + " " +
                       a.District.Settlement.Name + ", " +
                       a.Street + ", д. " +
                       a.Number,
                Value = a.ID
            });
            var lessors = db.Clients.Select(c => new
            {
                Text = c.PassportID + " " + c.Fullname,
                Value = c.PassportID
            });

            List<Rental_Status> statuses = new List<Rental_Status>();
            if (apartment.Rental_Status.ID == 2) statuses.Add(db.RentalStatus.Find(2));
            else statuses.AddRange(db.RentalStatus.Where(rs => rs.ID != 2));

            ViewBag.BalconyTypeId = new SelectList(db.BalconyType, "ID", "BalconyType", apartment.BalconyTypeId);
            ViewBag.BuildingId = new SelectList(adresses, "Value", "Text", adresses.Select(a => a.Value).Where(v => v == apartment.BuildingId).First());
            ViewBag.LessorId = new SelectList(lessors, "Value", "Text", lessors.Select(l => l.Value).Where(v => v == apartment.LessorId).First());
            ViewBag.InternetConnTypeId = new SelectList(db.InternetConn, "ID", "ConnectionType", apartment.InternetConnTypeId);
            ViewBag.StatusId = new SelectList(statuses, "ID", "Status", apartment.StatusId);
            ViewBag.TvTypeId = new SelectList(db.Televisions, "ID", "TVtype", apartment.TvTypeId);
            return View(apartment);
        }

        // POST: Apartments/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,StatusId,Number,RoomCount,TotalArea,LivingArea,BalconyTypeId,Fridge,Stove,WashMachine,AirConditioner,InternetConnTypeId,TvTypeId,WithPets,WithChildren,ForEvents,BuildingId,LessorId")] Apartment apartment)

        {
            if (ModelState.IsValid)
            {
                db.Entry(apartment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var adresses = db.Buildings.Select(a => new
            {
                Text = a.District.Settlement.Subject.Name + " ," +
                       a.District.Settlement.Settlement_Type.Type + " " +
                       a.District.Settlement.Name + ", " +
                       a.Street + ", д. " +
                       a.Number,
                Value = a.ID
            });
            var lessors = db.Clients.Select(c => new
            {
                Text = c.PassportID + " " + c.Fullname,
                Value = c.PassportID
            });

            ViewBag.BalconyTypeId = new SelectList(db.BalconyType, "ID", "BalconyType", apartment.BalconyTypeId);
            ViewBag.BuildingId = new SelectList(adresses, "Value", "Text", adresses.Select(a => a.Value).Where(v => v == apartment.BuildingId).First());
            ViewBag.LessorId = new SelectList(lessors, "Value", "Text", lessors.Select(l => l.Value).Where(v => v == apartment.LessorId).First());
            ViewBag.InternetConnTypeId = new SelectList(db.InternetConn, "ID", "ConnectionType", apartment.InternetConnTypeId);
            ViewBag.StatusId = new SelectList(db.RentalStatus, "ID", "Status", apartment.StatusId);
            ViewBag.TvTypeId = new SelectList(db.Televisions, "ID", "TVtype", apartment.TvTypeId);
            return View(apartment);
        }

        // GET: Apartments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }
            return View(apartment);
        }

        // POST: Apartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Apartment apartment = db.Apartments.Find(id);
            try
            {
                db.Apartments.Remove(apartment);
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
                var worksheet = workbook.Worksheets.Add("Квартиры");

                worksheet.Cell("A1").Value = "Адрес здания";
                worksheet.Cell("B1").Value = "Номер квартиры";
                worksheet.Cell("C1").Value = "Арендодатель";
                worksheet.Cell("D1").Value = "Арендный статус";
                worksheet.Cell("E1").Value = "Число комнат";
                worksheet.Cell("F1").Value = "Общая площадь";
                worksheet.Cell("G1").Value = "Жилая площадь";
                worksheet.Cell("H1").Value = "Холодильник";
                worksheet.Cell("I1").Value = "Кухонная плита";
                worksheet.Cell("J1").Value = "Стиральная машина";
                worksheet.Cell("K1").Value = "Кондиционер";
                worksheet.Cell("L1").Value = "С детьми";
                worksheet.Cell("M1").Value = "С питомцами";
                worksheet.Cell("N1").Value = "Для мероприятий";
                worksheet.Cell("O1").Value = "Балкон";
                worksheet.Cell("P1").Value = "Интернет";
                worksheet.Cell("Q1").Value = "Телевидение";
                worksheet.Row(1).Style.Font.Bold = true;

                int row = 2;

                foreach (var b in db.Apartments)
                {
                    worksheet.Cell(row, 1).Value = b.GetAdress();
                    worksheet.Cell(row, 2).Value = b.Number;
                    worksheet.Cell(row, 3).Value = b.Client.GetPassportAndFullname();
                    worksheet.Cell(row, 4).Value = b.Rental_Status.Status;
                    worksheet.Cell(row, 5).Value = b.RoomCount;
                    worksheet.Cell(row, 6).Value = b.TotalArea;
                    worksheet.Cell(row, 7).Value = b.LivingArea;
                    worksheet.Cell(row, 8).Value = b.Fridge;
                    worksheet.Cell(row, 9).Value = b.Stove;
                    worksheet.Cell(row, 10).Value = b.WashMachine;
                    worksheet.Cell(row, 11).Value = b.AirConditioner;
                    worksheet.Cell(row, 12).Value = b.WithChildren;
                    worksheet.Cell(row, 13).Value = b.WithPets;
                    worksheet.Cell(row, 14).Value = b.ForEvents;
                    worksheet.Cell(row, 15).Value = b.Balcony_Type.BalconyType;
                    worksheet.Cell(row, 16).Value = b.Internet_Conn.ConnectionType;
                    worksheet.Cell(row, 17).Value = b.Television.TVtype;

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"apartments_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
    }

}
