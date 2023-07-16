using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web.Mvc;
using SUARweb.Models;
using SUARweb.Exporters;

namespace SUARweb.Controllers
{
    public class RentersDebtsController : Controller
    {
        private SuarDbContext db = new SuarDbContext();

        // GET: RentersDebts
        public ActionResult Index(string renter, int? agr, string dateSort, string firstDate, string secondDate, int? minSum, int? maxSum)
        {
            var agreements = db.Agreements.Include(a => a.Agreement_Status).Include(a => a.Apartment).Include(a => a.Client).Include(a => a.Pay_Frequency);
            List<Debt> RDlist = new List<Debt>();

            foreach(var agreement in agreements)
                RDlist.Add(new Debt(agreement));

            if (!String.IsNullOrEmpty(renter)) RDlist = RDlist.Where(rd => rd.Agreement.Client.GetPassportAndFullname().Contains(renter)).ToList();
            if (agr != null) RDlist = RDlist.Where(rd => rd.Agreement.ID == agr).ToList();

            DateTime fd, sd;
            bool fdOk = DateTime.TryParse(firstDate, out fd);
            bool sdOk = DateTime.TryParse(secondDate, out sd);

            if (dateSort != "Не сортировать" && fdOk && sdOk)
            {
                if (dateSort == "Начала") RDlist = RDlist.Where(a => a.Agreement.StartDate >= fd && a.Agreement.StartDate <= sd).ToList();
                else if (dateSort == "Окончания") RDlist = RDlist.Where(a => a.Agreement.EndDate >= fd && a.Agreement.EndDate <= sd).ToList();
                else if (dateSort == "По сроку действия") RDlist = RDlist.Where(a => a.Agreement.StartDate >= fd && a.Agreement.EndDate <= sd).ToList();
            }

            if (minSum != null && maxSum != null)
            {
                RDlist = RDlist.Where(a => a.Difference >= minSum && a.Difference <= maxSum).ToList();
            }

            ViewBag.DataSortTypes = new SelectList(new List<string>()
            {
                "Не сортировать",
                "Начала",
                "Окончания",
                "По сроку действия"
            });

            return View(RDlist);
        }

        public ActionResult ExportToExcel()
        {
            var agreements = db.Agreements.Include(a => a.Agreement_Status).Include(a => a.Apartment).Include(a => a.Client).Include(a => a.Pay_Frequency);
            var RDlist = new List<IExportableEntity>();

            foreach (var agreement in agreements)
                RDlist.Add(new Debt(agreement));

            var exporter = new ExcelExporter(RDlist);
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