using System.ComponentModel.DataAnnotations;

namespace SadiclarasanWeb.Models
{
    public class AdminUser
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre zorunludur")]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir")]
        public string? FullName { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastLoginDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string Role { get; set; } = "Admin";
    }
}
