using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ImageCutter
{
    class Program
    {
        static void Main(string[] args)
        {
            ColorMatrix colorMatrix = new ColorMatrix(
              new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            var imgs = Directory.EnumerateFiles(
                @"D:\ClassificationMarkup",
                "*.jpg", 
                SearchOption.AllDirectories)
                .ToList();
            var outpath = @"D:\Classification";
            
            foreach(var img in imgs)
            {
                var count = 0;

                var xmlFile = Path.ChangeExtension(img, ".xml");
                if (!File.Exists(xmlFile))
                    continue;

                Console.WriteLine(xmlFile);

                using var realImg = new Bitmap(img);

                var bmpsToSave = new List<Bitmap>();

                foreach(XElement obj in XElement.Load(xmlFile).Elements("object"))
                {
                    var bndbox = obj.Element("bndbox");

                    var xmin = int.Parse(bndbox.Element("xmin").Value);
                    var ymin = int.Parse(bndbox.Element("ymin").Value);
                    var xmax = int.Parse(bndbox.Element("xmax").Value);
                    var ymax = int.Parse(bndbox.Element("ymax").Value);

                    var width   = xmax - xmin;                    
                    var height  = ymax - ymin;

                    var crop = new Bitmap(
                        446,
                        446,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    using var g = Graphics.FromImage(crop);
                    using var attr = new ImageAttributes();
                    attr.SetColorMatrix(colorMatrix);
                    g.DrawImage(
                        realImg,
                        new Rectangle(0, 0, 446, 446),
                        xmin, ymin, width, height,
                        GraphicsUnit.Pixel,
                        attr);

                    bmpsToSave.Add(crop);
                }

                foreach(var bmp in bmpsToSave)
                {
                    bmp.Save(
                        Path.Combine(
                            outpath,
                            $"{Path.GetFileNameWithoutExtension(img)}.{++count}.jpg"));
                }
            }
        }
    }
}
