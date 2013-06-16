using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SanzaiGuokrCore.Util
{
    public enum GuokrImageType
    {
        Image,
        Thumbnail,
        NotGuokrImage
    };
    public class GuokrImageInfo
    {
        public static string patternImage = @"(?<urlbase>.*)/image/(?<hash>[\w-_]*).(?<ext>jpg|png|gif|bmp|jpeg)";
        public static string patternThumbnail = @"(?<urlbase>.*)/thumbnail/(?<hash>[\w-_]*)_(\d{2,3})?x(\d{2,3})?.(?<ext>jpg|png|gif|bmp|jpeg)";

        public GuokrImageType Type;
        public string hash;
        public int width;
        public int height;
        public string ext;
        public string urlbase;
        public string url;

        public GuokrImageInfo(string url)
        {
            var m = Regex.Match(url, patternImage);
            if (m.Success)
                Type = GuokrImageType.Image;
            else
            {
                m = Regex.Match(url, patternThumbnail);
                if (m.Success)
                    Type = GuokrImageType.Thumbnail;
                else
                {
                    Type = GuokrImageType.NotGuokrImage;
                    this.url = url;
                    return;
                }
            }

            this.url = url;
            hash = m.Groups["hash"].Value;
            ext = m.Groups["ext"].Value;
            urlbase = m.Groups["urlbase"].Value;

            var b = Convert.FromBase64String(hash.Replace('-', '+').Replace('_', '/'));
            if (b.Length != 42)
                throw new ArgumentOutOfRangeException();
            width = (b[33] << 8) + b[32];
            height = (b[37] << 8) + b[36];
        }
        public string ToThumbnail(int width = 200, int height = 0)
        {
            if (Type == GuokrImageType.NotGuokrImage)
                return url;
            if (width == 0 && height == 0)
                throw new ArgumentOutOfRangeException();

            if (width != 0 && width > this.width)
                width = this.width;
            if (height != 0 && height > this.height)
                height = this.height;

            return string.Format("{0}/thumbnail/{1}_{2}x{3}.{4}", this.urlbase, this.hash,
                width == 0 ? "" : width.ToString(),
                height == 0 ? "" : height.ToString(),
                this.ext);
        }
        public string ToImage()
        {
            if (Type == GuokrImageType.NotGuokrImage)
                return url;

            return string.Format("{0}/image/{1}.{2}", urlbase, hash, ext);
        }
    }
}
