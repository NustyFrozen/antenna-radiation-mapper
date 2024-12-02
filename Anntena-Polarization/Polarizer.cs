using Emgu.CV;
using Emgu.CV.Structure;
using SharpGen.Runtime;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anntena_Polarization
{
    
    internal class Polarizer
    {
        public static SixLabors.ImageSharp.Image<Rgba32> ConvertBitmapToImageSharp(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Save the Bitmap to a memory stream
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Load the memory stream into an ImageSharp image
                return SixLabors.ImageSharp.Image.Load<Rgba32>(memoryStream);
            }
        }
        public struct polar
        {
            public double magnitude;
            public double angle;
            private double highResolution;
            public polar(Vector2 cartisian)
            {
                magnitude = Math.Sqrt(Math.Pow(cartisian.X, 2) + Math.Pow(cartisian.Y, 2));
                angle = Math.Atan2(cartisian.Y, cartisian.X);
                angle = (angle * 180.0 / Math.PI);
                angle += 180;
                highResolution = angle;
                angle = Math.Round(angle);
            }
            public double getHighResolutionAngle() { return highResolution; }
            
           
            public Vector2 toCartisian()
            {
                var tempAngle = (angle / 180.0 * Math.PI);
                float x, y;
                x = (float)(magnitude * Math.Cos(tempAngle));
                y = (float)(magnitude * Math.Sin(tempAngle));
                return new Vector2(x,y);
            }
            public override string ToString()
            {
                return $"{angle}:{magnitude}";
            }
            public int average;
        }
       
        public static double lerp(double x1, double x2, double t) { return (x1 * (1.0 - t) + (x2) * t); }
        public static void updateData(List<polar> points,polar newPoint)
        {
            var angle = newPoint.angle;
            if (points.Exists(x=>x.angle == angle))
            {
                var temp = points.First(x => (x.angle) == Math.Round(angle));
                points.Remove(temp);
                temp.magnitude = (float)(temp.magnitude * temp.average + newPoint.magnitude) / (temp.average + 1);
                temp.average++;
            } else
            {
                points.Add(new polar() { angle = Math.Round(angle), average = 1, magnitude = newPoint.magnitude });
            }
        }
        public static (Bitmap,List<polar>) begin(Bitmap src,Point center,int start,int scale,int scalePixels,Rgb min,Rgb max)
        {
            var image = src.ToImage<Rgb,byte>();
            Hsv lowerLimit = new Hsv(min.Red, min.Green, min.Blue);
            Hsv upperLimit = new Hsv(max.Red, max.Green, max.Blue);
            Image<Gray, byte> imageHSVDest = new Image<Gray, byte>(src.Width,src.Height);
            imageHSVDest = image.InRange(min, max);
            byte[,,] data = imageHSVDest.Data;
            List<polar> points = new List<polar>();
            for (int i = image.Rows - 1; i >= 0; i--)
            {
                for (int j = image.Cols - 1; j >= 0; j--)
                {
                    if(data[i, j, 0] != 0)
                    {
                        
                        polar polarview = new polar(new Vector2(i - center.X, j - center.Y));
                        //polarview.magnitude = (polarview.magnitude / scalePixels * scale) + start;
                       
                        updateData(points, polarview);
                    }
                }
            }
            for (int i = 0; i < 360; i++)
            {
                if (!points.Exists(x => x.angle == i))
                {
                    var closest = points.MinBy(x => Math.Abs(Math.Abs(x.angle) - Math.Abs(i))).magnitude;
                    points.Add(new polar() { angle = i, magnitude = closest, average = 1 });
                }
                Console.WriteLine($"{points.First(x => x.angle == i)}");
            }
            points = points.OrderBy(x => x.angle).ToList();

            //flipping values due to mirroring
            polar[] flip = points.ToArray();
            for (int i = 0; i < 180; i++)
            {
                var temp = flip[i].magnitude;
                flip[i].magnitude = flip[359 - i].magnitude;
                flip[359 - i].magnitude = temp;
            }
            //shifting 270 degrees
            for (int i = 0; i < 360; i++)
            {
                flip[i].angle = (Convert.ToInt32(flip[i].angle + 270)) % 360;
            }
            Console.WriteLine($"total points {points.Count}");
            points = flip.ToList();
            //resorting
            points = points.OrderBy(x => x.angle).ToList();
            points.RemoveAll(x => Convert.ToInt16(x.angle) == 360); //cuz 0 == 360
            return (imageHSVDest.ToBitmap(),points);
        }
    }
}
