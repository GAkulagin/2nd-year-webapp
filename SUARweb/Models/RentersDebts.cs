using System.ComponentModel;

namespace SUARweb.Models
{
    public class RentersDebts
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
    }
}