using System.ComponentModel.DataAnnotations;

namespace SadiclarasanWeb.Models
{
    public class SeoSettings
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Site başlığı zorunludur")]
        [StringLength(100, ErrorMessage = "Site başlığı en fazla 100 karakter olabilir")]
        public string SiteTitle { get; set; } = string.Empty;
        
        [StringLength(300, ErrorMessage = "Site açıklaması en fazla 300 karakter olabilir")]
        public string? SiteDescription { get; set; }
        
        public string? SiteKeywords { get; set; }
        
        [StringLength(100, ErrorMessage = "Site URL en fazla 100 karakter olabilir")]
        public string? SiteUrl { get; set; }
        
        public string? GoogleAnalyticsId { get; set; }
        
        public string? GoogleSearchConsole { get; set; }
        
        public string? FacebookPixelId { get; set; }
        
        public string? OgImage { get; set; }
        
        public string? TwitterCard { get; set; }
        
        public bool EnableSitemap { get; set; } = true;
        
        public bool EnableRobotsTxt { get; set; } = true;
        
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        
        public string? UpdatedBy { get; set; }
    }
}
