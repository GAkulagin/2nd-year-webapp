using System;
using System.Collections.Generic;
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
            var statuses = db.RentalStatus.Where(rs => rs.ID != RentalStatusCode.Occupied);

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
            var statuses = db.RentalStatus.Where(rs => rs.ID != RentalStatusCode.Occupied);

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
            if (apartment.Rental_Status.ID == RentalStatusCode.Occupied) 
                statuses.Add(db.RentalStatus.Where(rs => rs.ID == RentalStatusCode.Occupied).Single());

            else statuses.AddRange(db.RentalStatus.Where(rs => rs.ID != RentalStatusCode.Occupied));

            ViewBag.BalconyTypeId = new SelectList(db.BalconyType, "ID", "BalconyType", apartment.BalconyTypeId);
            ViewBag.BuildingId = new SelectList(adresses, "Value", "Text", adresses.Select(a => a.Value).Where(v => v == apartment.BuildingId).First());
            ViewBag.LessorId = new SelectList(lessors, "Value", "Text", lessors.Select(l => l.Value).Where(v => v == apartment.LessorId).First());
            ViewBag.InternetConnTypeId = new SelectList(db.InternetConn, "ID", "ConnectionType", apartment.InternetConnTypeId);
            ViewBag.StatusId = new SelectList(statuses, "ID", "Status", apartment.StatusId);
            ViewBag.TvTypeId = new SelectList(db.Televisions, "ID", "TVtype", apartment.TvTypeId);
            return View(apartment);
        }

        // POST: Apartments/Edit/5
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
            var exporter = new ExcelExporter(db.Apartments.ToList<IExportableEntity>());
            string filename = $"apartments_{DateTime.UtcNow.ToShortDateString()}.xlsx";

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
