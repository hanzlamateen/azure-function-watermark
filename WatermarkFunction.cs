using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WatermarkFunction
{
    public static class Function1
    {
        [FunctionName("WatermarkFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processing a request.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrEmpty(requestBody))
                {
                    return new BadRequestObjectResult("Please pass request body.");
                }

                dynamic data = JsonConvert.DeserializeObject(requestBody);

                // Ensure that json contains desired properties.
                string validation = ValidateBody(data);
                if (string.IsNullOrEmpty(validation) == false)
                {
                    return new BadRequestObjectResult(validation);
                }

                // If watermark uri is provided then use it in watermark.
                if (string.IsNullOrEmpty(data.watermarkUri?.Value) == false)
                {
                    var imageBytes = GenerateWatermarkFromImage(data, log);
                    return new FileContentResult(imageBytes, "image/png");
                }
                // If watermark uri is not provided then use text in watermark.
                else
                {
                    var imageBytes = GenerateWatermarkFromText(data, log);
                    return new FileContentResult(imageBytes, "image/png");
                }

                //return (ActionResult)new OkObjectResult($"Hello");
            }
            catch (Exception ex)
            {
                log.LogError("C# HTTP trigger function error in a request.", ex);
                return new BadRequestObjectResult(ex.Message);
            }
            finally
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
            }

        }

        private static string ValidateBody(dynamic data)
        {
            if (string.IsNullOrEmpty(data.imageUri?.Value))
            {
                return "Please pass imageUri in body.";
            }
            if (string.IsNullOrEmpty(data.watermarkUri?.Value) && string.IsNullOrEmpty(data.watermarkText?.Value))
            {
                return "Please pass watermarkUri or watermarkText in body.";
            }
            if (Enum.IsDefined(typeof(Gravity), (Int32)data.watermarkLocation?.Value) == false)
            {
                return "Please pass valid watermarkLocation in body.";
            }

            return null;
        }
        private static string DownloadImageFromUrl(string url, ILogger log)
        {
            try
            {
                Uri uri = new Uri(url);
                string filename = Path.GetFileName(uri.LocalPath);
                string filePath = Path.Combine(Path.GetTempPath(), filename);

                using (var client = new WebClient())
                {
                    client.DownloadFile(url, filePath);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                return string.Empty;
            }
        }

        private static byte[] GenerateWatermarkFromImage(dynamic data, ILogger log)
        {
            byte[] imageBytes;

            var imagePath = DownloadImageFromUrl(data.imageUri.Value, log);
            var watermarkPath = DownloadImageFromUrl(data.watermarkUri.Value, log);

            using (MagickImage image = new MagickImage(imagePath))
            {
                using (MagickImage watermark = new MagickImage(watermarkPath))
                {
                    image.Composite(watermark, (Gravity)data.watermarkLocation.Value, CompositeOperator.Over);
                    imageBytes = image.ToByteArray();
                }
            }
            try
            {
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
                if (File.Exists(watermarkPath))
                {
                    File.Delete(watermarkPath);
                }
            }
            catch { }

            return imageBytes;
        }

        private static byte[] GenerateWatermarkFromText(dynamic data, ILogger log)
        {
            byte[] imageBytes;

            var imagePath = DownloadImageFromUrl(data.imageUri.Value, log);

            using (MagickImage image = new MagickImage(imagePath))
            {
                image.Settings.FillColor = MagickColors.Transparent;
                image.Settings.StrokeColor = MagickColors.Transparent;
                image.Settings.FontPointsize = 40;
                image.Annotate(data.watermarkText.Value, (Gravity)data.watermarkLocation.Value);
                imageBytes = image.ToByteArray();
            }
            try
            {
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
            catch { }

            return imageBytes;
        }
    }
}
