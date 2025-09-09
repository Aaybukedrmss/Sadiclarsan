using System.ComponentModel.DataAnnotations;

namespace SadiclarasanWeb.Models
{
    public class Contact
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(15, ErrorMessage = "Telefon en fazla 15 karakter olabilir")]
        public string? Phone { get; set; }
        
        [Required(ErrorMessage = "Konu zorunludur")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir")]
        public string Subject { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mesaj zorunludur")]
        [StringLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir")]
        public string Message { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
    }
}
