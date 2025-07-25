using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Cache;


namespace wwwDesignersGuild.Models.Helper
{


    public class ImageResult : ActionResult
    {
        private string ImageID;
        private string Width;
        private string ImagePath;
        private short CacheTimeMinutes;
        private bool TryWebP;

        public ImageResult(string width, string imageId, string imagePath, short cacheTimeMinutes, bool tryWebP)
        {
            ImageID = Path.GetFileNameWithoutExtension(imageId);
            Width = width;
            ImagePath = imagePath;
            CacheTimeMinutes = cacheTimeMinutes;
            TryWebP = tryWebP;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                return;
            }

            var webp = false;

            //  Is image in cache? // - Tom, 23/10/2018 - removed; CloudFlare should be doing all of our caches.
            byte[] bytes = null;
            //ImageCacheProvider objImageCache = new ImageCacheProvider();
            //bytes = objImageCache.Get("IMAGE", _width, _imageId);
            
            if (bytes == null)
            {
                //  Read image from disk
                var jpegPath = String.Format("{0}\\{1}\\{2}.jpeg", ImagePath, Width, ImageID);
                var webpPath = String.Format("{0}\\{1}\\{2}.webp", ImagePath, Width, ImageID);

                if (TryWebP && File.Exists(webpPath))
                {
                    bytes = File.ReadAllBytes(webpPath);
                    webp = true;
                }
                else
                {
                    if (File.Exists(jpegPath))
                    {
                        bytes = File.ReadAllBytes(jpegPath);
                    }
                    else
                    {
                        return;
                    }
                }

                //objImageCache.Set("IMAGE", _width, _imageId, bytes, _cacheTimeMinutes);
            }

            try
            {

                /*
                
                var jpegQuality = 80;
                Image image;
                byte[] outputBytes;
                using (var inputStream = new MemoryStream(bytes))
                {
                    image = Image.FromStream(inputStream);
                    var jpegEncoder = ImageCodecInfo.GetImageDecoders()
                      .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);
                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, jpegEncoder, encoderParameters);
                        outputBytes = outputStream.ToArray();
                    }
                }
                 
                 */

                if (webp)
                {
                    MemoryStream imageStream = new MemoryStream(bytes);

                    //  Output image to browser
                    HttpResponseBase response = context.HttpContext.Response;
                    response.Headers.Add("Cache-Tag", $"ImageID-{ImageID}");
                    response.ContentType = "image/webp";
                    response.BufferOutput = true;
                    response.Clear();
                    byte[] buffer = new byte[4096];

                    while (true)
                    {
                        int read = imageStream.Read(buffer, 0, buffer.Length);

                        if (read == 0)
                        {
                            break;
                        }

                        response.OutputStream.Write(buffer, 0, read);
                    }
                    response.OutputStream.Flush();
                    response.End();
                    imageStream.Close();
                }
                else
                {
                    MemoryStream imageStream = new MemoryStream(bytes);

                    //  Output image to browser
                    HttpResponseBase response = context.HttpContext.Response;
                    response.Headers.Add("Cache-Tag", $"ImageID-{ImageID}");
                    response.ContentType = "image/jpeg";
                    response.BufferOutput = true;
                    response.Clear();
                    byte[] buffer = new byte[4096];

                    while (true)
                    {
                        int read = imageStream.Read(buffer, 0, buffer.Length);

                        if (read == 0)
                        {
                            break;
                        }

                        response.OutputStream.Write(buffer, 0, read);
                    }
                    response.OutputStream.Flush();
                    response.End();
                    imageStream.Close();
                }
            }
            catch (Exception)
            {
                //  Ignore error
            }
        }
    }
}