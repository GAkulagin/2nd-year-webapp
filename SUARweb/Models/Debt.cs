using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SUARweb.Exporters;

namespace SUARweb.Models
{
    public class Debt : IExportableEntity
    {
        private Agreement _agreement;
        
        public Debt(Agreement agr)
        {
            _agreement = agr;
        }

        [DisplayName("Договор")]
        public Agreement Agreement => _agreement;

        [DisplayName("Необходимо выплатить")]
        public decimal HaveToPay => GetDateDifference() * _agreement.PaySum;

        [DisplayName("Выплачено")]
        public decimal Paid => _agreement.Payments.Select(p => p.Sum).Sum();

        [DisplayName("Задолженность")]
        public decimal Difference => HaveToPay - Paid < 0 ? 0 : HaveToPay - Paid;


        private int GetDateDifference()
        {
            int result = 0;

            switch (_agreement.PayFrequencyId)
            {
                // dayly
                case 1:
                    {
                        result = (DateTime.Today - _agreement.StartDate).Days;
                        break;
                    }
                // weekly
                case 2:
                    {
                        int d = (DateTime.Today - _agreement.StartDate).Days;
                        result = d / 7;
                        if (d % 7 != 0) result++;
                        break;
                    }
                // monthly
                case 3:
                    {
                        int d = (DateTime.Today - _agreement.StartDate).Days;
                        result = d / 30;
                        if (d % 30 != 0) result++;
                        break;
                    }
            }

            return result;
        }

        public Dictionary<string, dynamic> GetExportData()
        {
            return new Dictionary<string, dynamic>()
            {
                { "Код договора", Agreement.ID },
                { "Арендатор", Agreement.Client.GetPassportAndFullname() },
                { "Дата начала", Agreement.StartDate },
                { "Дата окончания", Agreement.EndDate },
                { "Частота платы", Agreement.Pay_Frequency.Frequency },
                { "Сумма платы", Agreement.PaySum },
                { "Плата по договору", HaveToPay },
                { "Выплачено", Paid },
                { "Задолженность", Difference },
            };
        }
    }
}