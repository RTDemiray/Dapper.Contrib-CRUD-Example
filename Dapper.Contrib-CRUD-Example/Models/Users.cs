using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Contrib_CRUD_Example.Models
{
    [Contrib.Extensions.Table("Users")]
    public class Users
    {
        [ExplicitKey]
        public Guid? RowGuid { get; set; }
        [Required(ErrorMessage = "Ad alanı zorunludur!"),StringLength(50, ErrorMessage = "50 karakterden fazla olamaz!")]
        public string first_name { get; set; }
        [Required(ErrorMessage = "Soyad alanı zorunludur!"), StringLength(50, ErrorMessage = "50 karakterden fazla olamaz!")]
        public string last_name { get; set; }
        [Required(ErrorMessage = "Email alanı zorunludur!"), StringLength(50, ErrorMessage = "50 karakterden fazla olamaz!"), EmailAddress(ErrorMessage = "Lütfen e-mail formatında giriniz!")]
        public string email { get; set; }
        [Required(ErrorMessage = "Cinsiyet alanı zorunludur!"), StringLength(50, ErrorMessage = "50 karakterden fazla olamaz!")]
        public string gender { get; set; }
        [Required(ErrorMessage = "İp adresi alanı zorunludur!"), StringLength(20, ErrorMessage = "20 karakterden fazla olamaz!")]
        public string ip_address { get; set; }
        public bool is_active { get; set; }
        [Column(TypeName = "Date")]
        public DateTime date_time { get; set; }
    }
}
