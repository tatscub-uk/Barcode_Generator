using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using ZXing;

namespace BarcodeGenerator.Controllers
{
    public class BarcodeGeneratorController : ApiController
    {
        private const string mediaTypeImage = "image/{0}";
        private const string types = @"
        PDF_417,
        QR_CODE";

        private enum OutputOption
        {
            image,
            fileSystem
        }

        private enum ImageExtension
        {
            Png,
            Jpeg,
            Bmp
        }

        [Route("")]
        public HttpResponseMessage Get()
        {
            StringBuilder body = new StringBuilder();

            body.AppendLine("Welcome to BarcordeGenerator");
            body.AppendLine("");
            body.AppendLine("Usage:");
            body.AppendLine(string.Format("{0}[barcodeFormat]/[message]", Request.RequestUri.OriginalString));
            body.AppendLine("");
            body.AppendLine("Let's try make a PDF 417 barcode with 'Hello World' content.");
            body.AppendLine(string.Format("{0}PDF_417/Hello World", Request.RequestUri.OriginalString));
            body.AppendLine("");
            body.AppendLine("barcodeFormat parameter:");
            body.AppendLine("PDF_417 or QR_CODE.");
            body.AppendLine("   PDF_417: This option will generate a PDF 417 barcode.");
            body.AppendLine("   QR_CODE: This option will generate a QR code barcode.");
            body.AppendLine("");
            body.AppendLine("message parameter:");
            body.AppendLine("   This option will be barcode's content.");
            body.AppendLine("");
            body.AppendLine("Optional parameters:");
            body.AppendLine("");
            body.AppendLine("Usage:");
            body.AppendLine(string.Format("{0}[barcodeFormat]/[message]?outputOption=fileSystem&imageFormat=jpeg&height=100&width=300", Request.RequestUri.OriginalString));
            body.AppendLine("");
            body.AppendLine("If any parameter would be missing the default value will be used instead.");
            body.AppendLine("");
            body.AppendLine("outputOption: image or fileSystem.");
            body.AppendLine("   Default value: image.");
            body.AppendLine("   image: Service will return image file.");
            body.AppendLine(string.Format("   fileSystem: Service will generate image file at {0}.", System.Configuration.ConfigurationManager.AppSettings["fileSystemPath"]));
            body.AppendLine("imageFormat: Png, Jpeg or Bmp.");
            body.AppendLine("   Default value: Png.");
            body.AppendLine("   This option will generate image file as desired imageFormat parameter.");
            body.AppendLine("height: number.");
            body.AppendLine("   Default value: image default height.");
            body.AppendLine("   This option will change image height.");
            body.AppendLine("width: number.");
            body.AppendLine("   Default value: image default width.");
            body.AppendLine("   This option will change image width.");

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            result.Content = new StringContent(body.ToString());

            return result;
        }

        [Route("{barcodeFormat}/{content}")]
        public HttpResponseMessage Get(string barcodeFormat, string content, int width = 0, int height = 0, string imageFormat = "png", string outputOption = "image")
        {
            HttpResponseMessage result = null;
            BarcodeFormat _barCodeFormat;
            OutputOption _outputOption;
            System.Drawing.Imaging.ImageFormat _imageFormat;

            if (!Enum.TryParse<OutputOption>(outputOption, true, out _outputOption))
            {
                _outputOption = OutputOption.image;
            }

            if (!TryParseImageExtension(imageFormat, out _imageFormat))
            {
                _imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                imageFormat = "png";
            }


            if (Enum.TryParse<BarcodeFormat>(barcodeFormat, true, out _barCodeFormat))
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    IBarcodeWriter writer = new BarcodeWriter { Format = _barCodeFormat, Options = new ZXing.Common.EncodingOptions { Width = width, Height = height, Margin = 0, PureBarcode = true } };

                    MemoryStream ms = new MemoryStream();

                    Image img = writer.Write(content);

                    img.Save(ms, _imageFormat);

                    switch (_outputOption)
                    {
                        case OutputOption.image:
                            result = new HttpResponseMessage(HttpStatusCode.OK);
                            result.Content = new ByteArrayContent(ms.ToArray());
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue(string.Format(mediaTypeImage, imageFormat));
                            break;
                        case OutputOption.fileSystem:
                            string fileName = string.Format(@"{0}\{1}.{2}", System.Configuration.ConfigurationManager.AppSettings["fileSystemPath"], Guid.NewGuid(), imageFormat);

                            result = new HttpResponseMessage(HttpStatusCode.OK);
                            img.Save(fileName);
                            result.Content = new StringContent(string.Format("Image {0} generated successful.", fileName));

                            break;
                    }
                }
                else
                {
                    result = new HttpResponseMessage(HttpStatusCode.PartialContent);
                    result.Content = new StringContent("Missing Parameter message Content.");
                }
            }
            else
            {
                result = new HttpResponseMessage(HttpStatusCode.PartialContent);
                result.Content = new StringContent(string.Format("BarcodeFormat invalid type. Check out the valid types:{0}", types));
            }

            return result;
        }

        private bool TryParseImageExtension(string imageFormat, out System.Drawing.Imaging.ImageFormat _imageFormat)
        {
            ImageExtension _imageExtension;
            bool result = true;

            if (Enum.TryParse<ImageExtension>(imageFormat, true, out _imageExtension))
            {
                switch (_imageExtension)
                {
                    case ImageExtension.Png:
                        _imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                    case ImageExtension.Jpeg:
                        _imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case ImageExtension.Bmp:
                        _imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                    default:
                        _imageFormat = null;
                        result = false;
                        break;
                }
            }
            else
            {
                _imageFormat = null;
                result = false;
            }

            return result;
        }
    }
}