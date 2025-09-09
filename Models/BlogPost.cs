using System.ComponentModel.DataAnnotations;

namespace SadiclarasanWeb.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Başlık zorunludur")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "İçerik zorunludur")]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Özet en fazla 500 karakter olabilir")]
        public string? Summary { get; set; }
        
        public string? ImageUrl { get; set; }
        
        // SEO Fields
        [StringLength(200, ErrorMessage = "SEO başlık en fazla 200 karakter olabilir")]
        public string? SeoTitle { get; set; }
        
        [StringLength(300, ErrorMessage = "Meta açıklama en fazla 300 karakter olabilir")]
        public string? MetaDescription { get; set; }
        
        [StringLength(100, ErrorMessage = "SEO URL en fazla 100 karakter olabilir")]
        public string? SeoUrl { get; set; }
        
        public string? MetaKeywords { get; set; }
        
        public string? OgImage { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? Tags { get; set; }
        
        public int ViewCount { get; set; } = 0;
        
        public string? Author { get; set; }
        
        public int ReadingTime { get; set; } = 0; // dakika cinsinden
        
        public bool IsFeatured { get; set; } = false;
    }
}
