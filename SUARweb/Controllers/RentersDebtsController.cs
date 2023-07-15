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
            List<RentersDebts> RDlist = new List<RentersDebts>();

            foreach(var agreement in agreements)
            {
                RDlist.Add(new RentersDebts
                {
                    AgreementId = agreement.ID,
                    Renter = agreement.Client.GetPassportAndFullname(),
                    PayFrequency = agreement.Pay_Frequency.Frequency,
                    PaySum = agreement.PaySum,
                    HaveToPay = GetDateDifference(agreement) * agreement.PaySum,
                    Paid = agreement.Payments.Select(p => p.Sum).Sum(),
                    StartDate = agreement.StartDate,
                    EndDate = agreement.EndDate
                });
            }

            if (!String.IsNullOrEmpty(renter)) RDlist = RDlist.Where(rd => rd.Renter.Contains(renter)).ToList();
            if (agr != null) RDlist = RDlist.Where(rd => rd.AgreementId == agr).ToList();

            DateTime fd, sd;
            bool fdOk = DateTime.TryParse(firstDate, out fd);
            bool sdOk = DateTime.TryParse(secondDate, out sd);

            if (dateSort != "Не сортировать" && fdOk && sdOk)
            {
                if (dateSort == "Начала") RDlist = RDlist.Where(a => a.StartDate >= fd && a.StartDate <= sd).ToList();
                else if (dateSort == "Окончания") RDlist = RDlist.Where(a => a.EndDate >= fd && a.EndDate <= sd).ToList();
                else if (dateSort == "По сроку действия") RDlist = RDlist.Where(a => a.StartDate >= fd && a.EndDate <= sd).ToList();
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

        // вернет разность между датами конца и начала действия договора
        // в зависимости от частоты платы
        // ежедневно - разница в днях, еженедельно - в неделях...
        // здесь неделя - временной отрезок 7 дней
        // месяц - временной отрезок 30 дней
        private int GetDateDifference(Agreement agreement)
        {
            int result = 0;

            switch (agreement.PayFrequencyId)
            {
                // dayly
                case 1:
                    {
                        result = (DateTime.Today - agreement.StartDate).Days;
                        break;
                    }
                // weekly
                case 2:
                    {
                        int d = (DateTime.Today - agreement.StartDate).Days;
                        result = d / 7;
                        if (d % 7 != 0) result++;
                        break;
                    }
                // monthly
                case 3:
                    {
                        int d = (DateTime.Today - agreement.StartDate).Days;
                        result = d / 30;
                        if (d % 30 != 0) result++;
                        break;
                    }
            }

            return result;
        }

        public ActionResult ExportToExcel()
        {
            var agreements = db.Agreements.Include(a => a.Agreement_Status).Include(a => a.Apartment).Include(a => a.Client).Include(a => a.Pay_Frequency);
            var RDlist = new List<IExportableEntity>();

            foreach (var agreement in agreements)
            {
                RDlist.Add(new RentersDebts
                {
                    AgreementId = agreement.ID,
                    Renter = agreement.Client.GetPassportAndFullname(),
                    PayFrequency = agreement.Pay_Frequency.Frequency,
                    PaySum = agreement.PaySum,
                    HaveToPay = GetDateDifference(agreement) * agreement.PaySum,
                    Paid = agreement.Payments.Select(p => p.Sum).Sum(),
                    StartDate = agreement.StartDate,
                    EndDate = agreement.EndDate
                });
            }

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