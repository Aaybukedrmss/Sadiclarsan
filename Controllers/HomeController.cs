using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadiclarasanWeb.Data;
using SadiclarasanWeb.Models;
using System.Diagnostics;

namespace SadiclarasanWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // En son 3 blog yazısını getir
            var recentBlogs = await _context.BlogPosts
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedDate)
                .Take(3)
                .ToListAsync();

            // SEO ayarlarını getir
            var seoSettings = await _context.SeoSettings.FirstOrDefaultAsync();

            ViewBag.RecentBlogs = recentBlogs;
            ViewBag.SeoSettings = seoSettings;
            return View();
        }

        [HttpGet("blog/{slug}")]
        public async Task<IActionResult> Blog(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.IsActive && (b.SeoUrl == slug || b.Id.ToString() == slug));

            if (post == null)
            {
                return NotFound();
            }

            var seoSettings = await _context.SeoSettings.FirstOrDefaultAsync();
            ViewBag.SeoSettings = seoSettings;

            ViewData["Title"] = post.SeoTitle ?? post.Title;
            ViewBag.OverrideDescription = post.MetaDescription ?? post.Summary ?? (post.Content.Length > 160 ? post.Content.Substring(0, 160) + "..." : post.Content);
            ViewBag.OverrideKeywords = post.MetaKeywords;
            ViewBag.OverrideOgImage = post.OgImage ?? post.ImageUrl;

            return View("Blog", post);
        }

        [HttpGet("sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            var seoSettings = await _context.SeoSettings.FirstOrDefaultAsync();
            var siteUrl = seoSettings?.SiteUrl ?? "https://www.sadiclarsan.com.tr";
            siteUrl = siteUrl.TrimEnd('/');

            var blogs = await _context.BlogPosts
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            var urls = new List<string>
            {
                $"  <url>\n    <loc>{siteUrl}/</loc>\n    <changefreq>weekly</changefreq>\n    <priority>1.0</priority>\n  </url>"
            };

            foreach (var b in blogs)
            {
                var slug = string.IsNullOrWhiteSpace(b.SeoUrl) ? b.Id.ToString() : b.SeoUrl;
                urls.Add($"  <url>\n    <loc>{siteUrl}/blog/{slug}</loc>\n    <changefreq>weekly</changefreq>\n    <priority>0.8</priority>\n  </url>");
            }

            var xml = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n{string.Join("\n", urls)}\n</urlset>";
            return Content(xml, "application/xml");
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogContent(int id)
        {
            var blog = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

            if (blog == null)
            {
                return NotFound();
            }

            // Görüntülenme sayısını artır
            blog.ViewCount++;
            await _context.SaveChangesAsync();

            return Json(new { 
                title = blog.Title,
                content = blog.Content,
                author = blog.Author ?? "Sadıçlarsan",
                date = blog.CreatedDate.ToString("dd MMMM yyyy"),
                imageUrl = blog.ImageUrl,
                viewCount = blog.ViewCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Lütfen tüm alanları doğru şekilde doldurunuz.";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
