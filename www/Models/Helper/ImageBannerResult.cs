using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Cache;

namespace wwwDesignersGuild.Models.Helper
{

    public class ImageBannerResult : ActionResult
    {

        private readonly string Width;
        private readonly string ImageID;
        private readonly string ImagePath;
        public bool TryWebP { get; set; }

        public ImageBannerResult(string width, string imageId, string imagePath, bool tryWebP)
        {
            Width = width;
            ImageID = imageId;
            ImagePath = imagePath;
            TryWebP = tryWebP;
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                return;
            }

            byte[] bytes;

            var _filePath = String.Format("{0}\\{1}\\{2}.jpeg", ImagePath, Width, ImageID);
            var _gifFilePath = String.Format("{0}\\{1}\\{2}.gif", ImagePath, Width, ImageID);
            var webpPath = String.Format("{0}\\{1}\\{2}.webp", ImagePath, Width, ImageID);

            var contentType = "image/jpeg";

            if (TryWebP && File.Exists(webpPath))
            {
                _filePath = webpPath;
                contentType = "image/webp";
            }

            // Not-free upgrades!

            if (Width == "512")
            {
                if (File.Exists(_filePath.Replace("512", "1024")))
                {
                    _filePath = _filePath.Replace("512", "1024");
                }
                if (File.Exists(_gifFilePath.Replace("512", "1024")))
                {
                    _gifFilePath = _gifFilePath.Replace("512", "1024");
                }
            }

            if (Width == "1024")
            {
                if (File.Exists(_filePath.Replace("1024", "1546")))
                {
                    _filePath = _filePath.Replace("1024", "1024");
                }
                if (File.Exists(_gifFilePath.Replace("1024", "1024")))
                {
                    _gifFilePath = _gifFilePath.Replace("1024", "1024");
                }
            }

            if (File.Exists(_filePath))
            {
                bytes = File.ReadAllBytes(_filePath);
            }
            else if (File.Exists(_gifFilePath))
            {
                bytes = File.ReadAllBytes(_gifFilePath);
            }
            else
            {
                return;
            }

            try
            {
                MemoryStream imageStream = new MemoryStream(bytes);

                //  Output image to browser
                HttpResponseBase response = context.HttpContext.Response;
                response.ContentType = contentType;
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
            catch(Exception)
            {
                //  Ignore error
            }
        }
    }
}