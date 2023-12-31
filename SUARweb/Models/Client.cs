//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SUARweb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using SUARweb.Exporters;
    using SUARweb.Models.CustomValidation;


    public partial class Client : IExportableEntity
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Client()
        {
            this.Agreements = new HashSet<Agreement>();
            this.Apartments = new HashSet<Apartment>();
        }


        [Key]
        [DisplayName("Паспорт")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [RegularExpression(@"^\d{4} \d{6}$", ErrorMessage = "Неверный формат ввода")]
        public string PassportID { get; set; }

        [DisplayName("ФИО")]
        [Required(ErrorMessage = "Это поле обязательно")]
        public string Fullname { get; set; }

        [DisplayName("Адрес регистрации")]
        [Required(ErrorMessage = "Это поле обязательно")]
        public string RegistrationAddress { get; set; }

        [DisplayName("Дата рождения")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [IsAdult(ErrorMessage = "Клиент должен быть совершеннолетним")]
        public System.DateTime Birthdate { get; set; }

        [DisplayName("Пол")]
        public int GenderId { get; set; }

        [DisplayName("Гражданство")]
        [Required(ErrorMessage = "Это поле обязательно")]
        public string Citizenship { get; set; }

        [DisplayName("Код УФМС")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [RegularExpression(@"^\d{3}-\d{3}$", ErrorMessage = "Неверный формат ввода")]
        public string UFMScode { get; set; }

        [DisplayName("Дата выдачи паспорта")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [CustomValidation(typeof(Client), "ValidatePassportDate")]
        public System.DateTime PassportDate { get; set; }

        [DisplayName("ИНН")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Неверный формат ввода")]
        public string INN { get; set; }

        [DisplayName("КПП")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Неверный формат ввода")]
        public string KPP { get; set; }

        [DisplayName("БИК")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Неверный формат ввода")]
        public string BIK { get; set; }

        [DisplayName("Номер счета")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [RegularExpression(@"^\d{20}$", ErrorMessage = "Неверный формат ввода")]
        public string BankAccount { get; set; }

        [DisplayName("Телефон")]
        [Required(ErrorMessage = "Это поле обязательно")]
        [Phone(ErrorMessage = "Неверный формат ввода")]
        public string Phone { get; set; }

        [DisplayName("Эл. почта")]
        [EmailAddress(ErrorMessage = "Неверный формат ввода")]
        public string Email { get; set; }


        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Agreement> Agreements { get; set; }
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Apartment> Apartments { get; set; }
        public virtual Gender Gender { get; set; }


        public Dictionary<string, dynamic> GetExportData()
        {
            return new Dictionary<string, dynamic>()
            {
                { "ФИО", Fullname },
                { "Дата рождения", Birthdate },
                { "Пол", Gender.Value },
                { "Гражданство", Citizenship },
                { "Адрес регистрации", RegistrationAddress },
                { "Серия и номер паспорта", PassportID },
                { "Дата выдачи паспорта", PassportDate },
                { "Код УФМС", UFMScode },
                { "ИНН", INN },
                { "КПП", KPP },
                { "БИК", BIK },
                { "Расчетный счет", BankAccount },
                { "Телефон", Phone },
                { "Электронная почта", Email },
            };
        }

        public string GetPassportAndFullname()
        {
            return PassportID + " " + Fullname;
        }


        public static ValidationResult ValidatePassportDate(object value, ValidationContext context)
        {
            if (value == null)
                return ValidationResult.Success;

            var date = (DateTime)value;
            var client = context.ObjectInstance as Client;

            if (date > client.Birthdate && date <= DateTime.Today)
                return ValidationResult.Success;

            return new ValidationResult("Неверный формат ввода");
        }
    }
}
