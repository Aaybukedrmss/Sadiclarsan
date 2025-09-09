using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadiclarasanWeb.Data;
using SadiclarasanWeb.Models;
using System.Text.RegularExpressions;

namespace SadiclarasanWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // Blog Management
        public async Task<IActionResult> BlogList()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            var blogs = await _context.BlogPosts
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
            return View(blogs);
        }

        public IActionResult CreateBlog()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(BlogPost blog, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Resim yükleme işlemi
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, "images", "blog", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    
                    blog.ImageUrl = "/images/blog/" + fileName;
                }

                // Slug (SeoUrl) oluşturma / benzersizleştirme
                if (string.IsNullOrWhiteSpace(blog.SeoUrl))
                {
                    blog.SeoUrl = GenerateUniqueSlug(blog.Title);
                }
                else
                {
                    blog.SeoUrl = GenerateUniqueSlug(blog.SeoUrl);
                }

                blog.CreatedDate = DateTime.Now;
                _context.BlogPosts.Add(blog);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Blog yazısı başarıyla oluşturuldu.";
                return RedirectToAction("BlogList");
            }

            return View(blog);
        }

        public async Task<IActionResult> EditBlog(int id)
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(int id, BlogPost blog, IFormFile? imageFile)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = await _context.BlogPosts.FindAsync(id);
                    if (existingBlog == null)
                    {
                        return NotFound();
                    }

                    // Resim yükleme işlemi
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(existingBlog.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_environment.WebRootPath, existingBlog.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, "images", "blog", fileName);
                        
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        
                        existingBlog.ImageUrl = "/images/blog/" + fileName;
                    }

                    existingBlog.Title = blog.Title;
                    existingBlog.Content = blog.Content;
                    existingBlog.Summary = blog.Summary;
                    existingBlog.Tags = blog.Tags;
                    existingBlog.IsActive = blog.IsActive;
                    existingBlog.UpdatedDate = DateTime.Now;

                    // SEO alanları
                    existingBlog.SeoTitle = blog.SeoTitle;
                    existingBlog.MetaDescription = blog.MetaDescription;
                    existingBlog.MetaKeywords = blog.MetaKeywords;
                    existingBlog.OgImage = string.IsNullOrWhiteSpace(blog.OgImage) ? existingBlog.OgImage : blog.OgImage;

                    // SeoUrl boşsa başlıktan üret; doluysa normalize et ve benzersizleştir
                    var desiredSlug = string.IsNullOrWhiteSpace(blog.SeoUrl) ? blog.Title : blog.SeoUrl;
                    existingBlog.SeoUrl = GenerateUniqueSlug(desiredSlug, existingBlog.Id);

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("BlogList");
            }
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog != null)
            {
                // Resim dosyasını sil
                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.BlogPosts.Remove(blog);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blog yazısı başarıyla silindi.";
            }

            return RedirectToAction("BlogList");
        }

        // Contact Messages
        public async Task<IActionResult> ContactMessages()
        {
            var messages = await _context.Contacts
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                contact.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ContactMessages");
        }

        // SEO Settings
        public async Task<IActionResult> SeoSettings()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var seoSettings = await _context.SeoSettings.FirstOrDefaultAsync();
            if (seoSettings == null)
            {
                seoSettings = new SeoSettings
                {
                    SiteTitle = "Sadıçlarsan | Endüstriyel Mutfak Havalandırma Çözümleri",
                    SiteDescription = "Endüstriyel mutfaklar için havalandırma ve filtreleme çözümlerinde lider firma. 2000 yılından beri hava kalitesi için çalışıyoruz.",
                    SiteKeywords = "endüstriyel mutfak, havalandırma, filtreleme, hava kalitesi, mutfak sistemleri",
                    SiteUrl = "https://www.sadiclarsan.com.tr",
                    EnableSitemap = true,
                    EnableRobotsTxt = true,
                    TwitterCard = "summary_large_image"
                };
            }
            return View(seoSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeoSettings(SeoSettings seoSettings)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                var existingSettings = await _context.SeoSettings.FirstOrDefaultAsync();
                if (existingSettings == null)
                {
                    seoSettings.UpdatedDate = DateTime.Now;
                    seoSettings.UpdatedBy = User.Identity.Name ?? "Admin";
                    _context.SeoSettings.Add(seoSettings);
                }
                else
                {
                    existingSettings.SiteTitle = seoSettings.SiteTitle;
                    existingSettings.SiteDescription = seoSettings.SiteDescription;
                    existingSettings.SiteKeywords = seoSettings.SiteKeywords;
                    existingSettings.SiteUrl = seoSettings.SiteUrl;
                    existingSettings.GoogleAnalyticsId = seoSettings.GoogleAnalyticsId;
                    existingSettings.GoogleSearchConsole = seoSettings.GoogleSearchConsole;
                    existingSettings.FacebookPixelId = seoSettings.FacebookPixelId;
                    existingSettings.OgImage = seoSettings.OgImage;
                    existingSettings.TwitterCard = seoSettings.TwitterCard;
                    existingSettings.EnableSitemap = seoSettings.EnableSitemap;
                    existingSettings.EnableRobotsTxt = seoSettings.EnableRobotsTxt;
                    existingSettings.UpdatedDate = DateTime.Now;
                    existingSettings.UpdatedBy = User.Identity.Name ?? "Admin";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "SEO ayarları başarıyla güncellendi.";
                return RedirectToAction("SeoSettings");
            }

            return View(seoSettings);
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }

        private string GenerateUniqueSlug(string text, int? currentId = null)
        {
            var baseSlug = GenerateSlug(text);
            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug = Guid.NewGuid().ToString("n").Substring(0, 8);
            }

            var slug = baseSlug;
            int suffix = 1;
            while (_context.BlogPosts.Any(b => b.SeoUrl == slug && (!currentId.HasValue || b.Id != currentId.Value)))
            {
                slug = $"{baseSlug}-{suffix++}";
            }
            return slug;
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            text = text.Trim().ToLowerInvariant();

            // Türkçe karakter dönüşümleri
            text = text.Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i").Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");
            text = text.Replace("Ç", "c").Replace("Ğ", "g").Replace("İ", "i").Replace("Ö", "o").Replace("Ş", "s").Replace("Ü", "u");

            // Alfanumerik ve boşluk/dash dışını sil
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            // Boşlukları tek tireye indir
            text = Regex.Replace(text, @"\s+", "-");
            // Çoklu tireleri tek tire yap
            text = Regex.Replace(text, @"-+", "-");
            // Baş/son tireleri kırp
            text = text.Trim('-');
            return text;
        }

        // Şifre Değiştirme
        public IActionResult ChangePassword()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Tüm alanları doldurunuz.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Yeni şifreler eşleşmiyor.";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Şifre en az 6 karakter olmalıdır.";
                return View();
            }

            var username = User.Identity.Name;
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);

            if (admin == null)
            {
                ViewBag.Error = "Admin kullanıcı bulunamadı.";
                return View();
            }

            // Mevcut şifreyi kontrol et
            var currentPasswordHash = HashPassword(currentPassword);
            if (admin.PasswordHash != currentPasswordHash)
            {
                ViewBag.Error = "Mevcut şifre yanlış.";
                return View();
            }

            // Yeni şifreyi kaydet
            admin.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction("Index");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "SadiclarasanSalt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
