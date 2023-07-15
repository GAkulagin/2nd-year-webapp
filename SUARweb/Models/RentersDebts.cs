using System.Collections.Generic;
using System.ComponentModel;
using SUARweb.Exporters;

namespace SUARweb.Models
{
    public class RentersDebts : IExportableEntity
    {
        [DisplayName("Код договора")]
        public int AgreementId { get; set; }
        [DisplayName("Арендатор")]
        public string Renter { get; set; }
        [DisplayName("Частота платы")]
        public string PayFrequency { get; set; }
        [DisplayName("Сумма платы")]
        public decimal PaySum { get; set; }
        [DisplayName("Необходимо выплатить")]
        public decimal HaveToPay { get; set; }
        [DisplayName("Выплачено")]
        public decimal Paid { get; set; }
        [DisplayName("Задолженность")]
        public decimal Difference
        {
            get
            {
                if (HaveToPay - Paid < 0) return 0;
                else return HaveToPay - Paid;
            }
            set
            {
                if (value < 0) Difference = 0;
                else Difference = value;
            }
        }
        [DisplayName("Дата начала")]
        public System.DateTime StartDate { get; set; }
        [DisplayName("Дата окончания")]
        public System.DateTime EndDate { get; set; }


        public Dictionary<string, dynamic> GetExportData()
        {
            return new Dictionary<string, dynamic>()
            {
                { "Код договора", AgreementId },
                { "Арендатор", Renter },
                { "Дата начала", StartDate },
                { "Дата окончания", EndDate },
                { "Частота платы", PayFrequency },
                { "Сумма платы", PaySum },
                { "Плата по договору", HaveToPay },
                { "Выплачено", Paid },
                { "Задолженность", Difference },
            };
        }
    }
}