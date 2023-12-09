using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using System.Threading.Tasks;




namespace webattempt.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public bool ImageProcessed { get; private set; } = false;
        public string ErrorMessage { get; private set; }

        public IndexModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string ProcessedImageUrl { get; private set; }

        public IActionResult OnPost()
        {
            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        ImageFile.CopyTo(stream);
                        Bitmap originalBitmap = new Bitmap(stream);

                        // Process the image (replace this with your custom processing logic)
                        Bitmap processedBitmap = ProcessImage(originalBitmap);

                        // Save the processed image to TempData for later retrieval
                        TempData["ProcessedBitmap"] = processedBitmap;

                        ImageProcessed = true;
                    }
                }
                else
                {
                    ErrorMessage = "Please select a valid image file.";
                }
                ProcessedImageUrl = Url.Action("GetProcessedImage");
                TempData["ProcessedImageUrl"] = ProcessedImageUrl;

                ImageProcessed = true;
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"An error occurred: {ex}");

                // Display a user-friendly error message
                ErrorMessage = "An unexpected error occurred. Please try again later.";
            }

            return Page();
        }

        public IActionResult OnGetProcessedImage()
        {
            // Retrieve the processed image from TempData
            if (TempData["ProcessedImageUrl"] is string imageUrl)
            {
                return new JsonResult(new { imageUrl });
            }
            return new JsonResult(new { }); // Return an empty JSON object if no image is found
        }

        public IActionResult OnGetDownloadProcessedImage()
        {
            // Retrieve the processed image from TempData
            if (TempData["ProcessedBitmap"] is Bitmap processedBitmap)
            {
                MemoryStream stream = new MemoryStream();
                processedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Position = 0;

                return new FileStreamResult(stream, "image/jpeg")
                {
                    FileDownloadName = "processed_image.jpg"
                };
            }

            return NotFound();
        }

        private Bitmap ProcessImage(Bitmap originalBitmap)
        {
            var resized = ImageChanging1.resizeImage(originalBitmap, 100, 100);
            PaletteGenerator.Generate(resized, 100);
            resized = ImageChanging1.resizeImage(originalBitmap, 1000, 1100);
            return resized;
        }
    }
}
//    public class IndexModel : PageModel
//    {
//        private IWebHostEnvironment Environment;
//        public IndexModel(IWebHostEnvironment _environment)
//        {
//            Environment = _environment;
//        }
//        //public IFormFile ImageResult { get; set; }

//        public class YourController : Controller
//        {
//            [HttpGet]
//            public IActionResult Index()
//            {
//                return View();
//            }

//            [HttpPost]
//            public IActionResult UploadImage(IFormFile file)
//            {
//                if (file != null && file.Length > 0)
//                {
//                    using (var stream = file.OpenReadStream())
//                    {
//                        // Process the image stream as needed (e.g., convert to pixel art)
//                        Bitmap processedImage = ProcessImage(stream);

//                        // Optionally, you can save the processedImage to a MemoryStream
//                        using (var memoryStream = new MemoryStream())
//                        {
//                            processedImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
//                            memoryStream.Position = 0;

//                            // Return the processed image as a downloadable file
//                            return File(memoryStream, "image/png", "processed_image.png");
//                        }
//                    }
//                }

//                // Handle the case where no file is uploaded
//                return RedirectToAction("Index");
//            }

//            [HttpPost]
//            public async Task<IActionResult> ProcessImage(Stream imageFile)
//            {
//                var proc = PaletteGenerator.Generate(imageFile, 100);
//                // Process the image (replace this with your actual image processing logic)
//                string processedImageUrl = await ProcessImageAsync(proc);

//                // Return the processed image URL to the client
//                return Content(processedImageUrl);
//            }


//            [HttpGet]
//            public IActionResult GetProcessingStatus()
//            {
//                // Simulate processing status (replace this with your actual logic)
//                string processingStatus = "Processing in progress...";

//                // Return the processing status to the client
//                return Content(processingStatus);
//            }

//            private async Task<string> ProcessImageAsync(IFormFile imageFile)
//            {
//                // Replace this with your actual image processing logic
//                // For example, save the file, perform processing, and return the processed image URL
//                if (imageFile != null && imageFile.Length > 0)
//                {
//                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
//                    var filePath = Path.Combine(uploadDir, imageFile.FileName);

//                    using (var stream = new FileStream(filePath, FileMode.Create))
//                    {
//                        await imageFile.CopyToAsync(stream);
//                    }

//                    // Perform your image processing logic here and get the processed image URL
//                    string processedImageUrl = $"/images/{imageFile.FileName}";

//                    return processedImageUrl;
//                }
//                return null;
//            }
//        }

//        //public IActionResult OnPost(IFormFile imageFile)
//        //{
//        //    if (imageFile == null || imageFile.Length == 0)
//        //    {
//        //        // Handle the case where no file is uploaded
//        //        // You may want to return an error or redirect to the same page
//        //        return Page();
//        //    }

//        //    string path = Path.Combine(this.Environment.WebRootPath, "Uploads");
//        //    if (!Directory.Exists(path))
//        //    {
//        //        Directory.CreateDirectory(path);
//        //    }
//        //    string filename = Path.GetFileName(imageFile.FileName);

//        //    string filePath = Path.Combine("wwwroot/images", Guid.NewGuid().ToString() + filename);

//        //    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
//        //    {
//        //        imageFile.CopyTo(fileStream);
//        //    }
//        //    PixelateImage(filePath);
//        //    ImageResult = "/images/" + Guid.NewGuid().ToString() + filename;

//        //    //    // Call the pixelation process using your C# code
//        //    //    // Example: PixelateImage(filePath);

//        //    //    ImageResult = "/images/" + uniqueFileName;

//        //    return Page();
//        //}

//       // private void PixelateImage(string ImageResult)
//        //{
//            // Your C# code to pixelate the image
//            // Example:
//            // Implement the pixelation algorithm and save the pixelated image to a new file

//            //PaletteGenerator.Generate(ImageResult, 300);
//        //}

//        //private readonly ILogger<IndexModel> _logger;

//        //public IndexModel(ILogger<IndexModel> logger)
//        //{
//        //    _logger = logger;
//        //}

//        public void OnGet()
//        {

//        }
//    }
