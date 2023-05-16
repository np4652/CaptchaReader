using Microsoft.AspNetCore.Mvc;
using OCR.Models;
using RoundpayFinTech.AppCode.HelperClass;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;


namespace OCR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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

        [HttpPost(nameof(ReadTextFromImage_Old))]
        public IActionResult ReadTextFromImage_Old([FromForm] IFormFile file)
        {
            OCRHelper ocr = new OCRHelper();
            string result = string.Empty;
            try
            {
                result = ocr.ReadTextFromImage(new OcrModel
                {
                    DestinationLanguage=DestinationLanguage.English,
                    Image = file
                });
            }
            catch (Exception ex)
            {

            }
            return Json(result);
        }

        [HttpPost(nameof(ReadTextFromImage))]
        public IActionResult ReadTextFromImage([FromForm] IFormFile file)
        {
            OCRHelper ocr = new OCRHelper();
            try
            {
                var output = AdjustBrightnessContrast(file, 0.5f, 1.2f);
                return Json(new
                {
                    ocrText = ocr.ReadTextFromImage(output),
                    img = Convert.ToBase64String(output)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ocrText = ex.Message,
                    img = ""
                });
            }

        }

        [HttpPost(nameof(ReadTextFromBase64))]
        public IActionResult ReadTextFromBase64(string imageDataUrl)
        {
            OCRHelper ocr = new OCRHelper();
            string result = string.Empty;
            try
            {
                var imageData = Convert.FromBase64String(imageDataUrl.Split(',')[1]);
                //var output = BrightenImage(file);
                result = ocr.ReadTextFromImage(imageData);
                //result = Convert.ToBase64String(output);
            }
            catch (Exception ex)
            {

            }
            return Json(result);
        }

        private byte[] EnlargeImage(byte[] imageBytes)
        {
            try
            {
                // Load the image from a MemoryStream
                MemoryStream stream = new MemoryStream(imageBytes); // replace `imageBytes` with your actual byte array
                Image originalImage = Image.FromStream(stream);

                // Calculate the new width and height
                int newWidth = originalImage.Width * 2;
                int newHeight = originalImage.Height * 2;

                // Create a new Bitmap object with the new dimensions
                Bitmap enlargedImage = new Bitmap(newWidth, newHeight);

                // Create a Graphics object from the Bitmap
                Graphics graphics = Graphics.FromImage(enlargedImage);

                // Set the InterpolationMode to HighQualityBicubic to ensure high-quality enlargement
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // Draw the original image onto the new Bitmap object, scaled up to the new size
                graphics.DrawImage(originalImage, new Rectangle(0, 0, newWidth, newHeight));

                // Save the enlarged image to a MemoryStream
                MemoryStream outputStream = new MemoryStream();
                enlargedImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] outputBytes = outputStream.ToArray();

                return outputBytes;
                // You can now use the `outputBytes` as the enlarged image in your application
            }
            catch
            {
                return null;
            }

        }

        private byte[] BrightenImage(IFormFile formFile)
        {
            // Save the modified image to a byte array
            byte[] outputBytes = null;
            try
            {
                Image image;
                using (MemoryStream ms = new MemoryStream())
                {
                    formFile.CopyTo(ms);
                    image = Image.FromStream(ms);
                }
                float brightness = 0.6f; // Set the brightness level, between 0 and 1
                // Loop through all the pixels in the image
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        // Get the current pixel
                        Color pixel = ((Bitmap)image).GetPixel(x, y);

                        // Calculate the new brightness level
                        float newBrightness = pixel.GetBrightness() + brightness;


                        //Ensure the brightness level stays between 0 and 1
                        if (newBrightness > 1)
                        {
                            newBrightness = 1;
                        }
                        else if (newBrightness < 0)
                        {
                            newBrightness = 0;
                        }

                        // Create the new color with the adjusted brightness level
                        Color newColor = Color.FromArgb(pixel.A, (int)(pixel.R * newBrightness), (int)(pixel.G * newBrightness), (int)(pixel.B * newBrightness));

                        // Set the new pixel color
                        ((Bitmap)image).SetPixel(x, y, newColor);
                    }
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    outputBytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return outputBytes;
        }
        public static byte[] AdjustBrightnessContrast(IFormFile formFile, float brightness, float contrast)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    formFile.CopyTo(ms);
                    using (var image = new Bitmap(ms))
                    {
                        var matrix = new float[][] {
                        new float[] { contrast, 0f, 0f, 0f, 0f },
                        new float[] { 0f, contrast, 0f, 0f, 0f },
                        new float[] { 0f, 0f, contrast, 0f, 0f },
                        new float[] { 0f, 0f, 0f, 1f, 0f },
                        new float[] { brightness, brightness, brightness, 0f, 1f }
                    };
                        var attributes = new ImageAttributes();
                        attributes.SetColorMatrix(new ColorMatrix(matrix));

                        var resultImage = new Bitmap(image.Width, image.Height);

                        using (var graphics = Graphics.FromImage(resultImage))
                        {
                            graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                        }

                        using (var ms2 = new MemoryStream())
                        {
                            resultImage.Save(ms2, ImageFormat.Png);
                            return ms2.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}