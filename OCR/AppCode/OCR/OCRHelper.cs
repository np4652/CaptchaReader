using System.Drawing;
using Tesseract;


namespace RoundpayFinTech.AppCode.HelperClass
{
    public class OCRHelper
    {
        public readonly string tesseractDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppCode/OCR/TesseractData");
        public string ReadTextFromImage(OcrModel request)
        {
            string result = "No Text Found";
            try
            {
                if (request.Image == null)
                {
                    result = "Please choose Image first";
                    goto Finish;
                }
                string name = request.Image.FileName;
                var image = request.Image;

                if (image.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        image.CopyTo(ms);
                        string destinationLanguage = !string.IsNullOrEmpty(request.DestinationLanguage) ? request.DestinationLanguage : DestinationLanguage.English;
                        using (var engine = new TesseractEngine(tesseractDataPath, destinationLanguage, EngineMode.Default))
                        {
                            using (var img = Pix.LoadFromMemory(ms.ToArray()))
                            {
                                var page = engine.Process(img);
                                result = page.GetText();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Something went wrong";
            }
        Finish:
            return String.IsNullOrWhiteSpace(result) ? "Ocr is finished. Return empty" : result;
        }

        public string ReadTextFromImage(byte[] imageBytes)
        {
            string result = "No Text Found";
            try
            {
                if (imageBytes == null)
                {
                    result = "Please choose Image first";
                    goto Finish;
                }
                Image image;
                using (var ms = new MemoryStream(imageBytes))
                {
                    image = Image.FromStream(ms);

                    string destinationLanguage = DestinationLanguage.English;
                    using (var engine = new TesseractEngine(tesseractDataPath, destinationLanguage, EngineMode.Default))
                    {
                        using (var img = Pix.LoadFromMemory(ms.ToArray()))
                        {
                            var page = engine.Process(img);
                            result = page.GetText();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Something went wrong";
            }
Finish:
            return String.IsNullOrWhiteSpace(result) ? "Ocr is finished. Return empty" : result;
        }
    }

    public class OcrModel
    {
        public string DestinationLanguage { get; set; }
        public IFormFile Image { get; set; }
    }

    public class DestinationLanguage
    {
        public const string English = "eng";
    }
}
