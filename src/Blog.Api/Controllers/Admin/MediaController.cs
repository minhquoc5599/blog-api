using Blog.Core.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly MediaSettings _mediaSettings;

        public MediaController(IWebHostEnvironment webHostEnvironment, IOptions<MediaSettings> mediaSettings)
        {
            _webHostEnvironment = webHostEnvironment;
            _mediaSettings = mediaSettings.Value;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult UploadImage(string type)
        {
            var allowImageTypes = _mediaSettings.AllowImageFileTypes?.Split(",");
            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return null;
            }

            var file = files[0];
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition)?.FileName?.Trim('"');
            if (allowImageTypes?.Any(x => filename?.EndsWith(x, StringComparison.OrdinalIgnoreCase) == true) == false)
            {
                throw new Exception("Not image format");
            }

            var imageFolder = $@"\{_mediaSettings.ImageFolder}\images\{type}\{DateTime.Now:MMyyyy}";

            var folder = _webHostEnvironment.WebRootPath + imageFolder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(folder, filename);
            using (var fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
            var path = Path.Combine(imageFolder, filename).Replace(@"\", @"/");
            return Ok(new { path });
        }

    }
}
