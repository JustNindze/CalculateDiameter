using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace CalculateDiameter
{
    class ImageIntensity
    {
        private Bitmap Image;
        private Bitmap GrayScaleImage;
        private List<Pixel> GrayScaleImagePixels;
        private List<Pixel> RequiredGrayScaleImagePixels;
        private Pixel MaxIntensityPixel;
        private float HalfIntensity;
        private string ImageName;

        public ImageIntensity(string imageName)
        {
            Image = new Bitmap(imageName);
            ImageName = imageName;
        }

        public void ConvertImageToGrayScale()
        {
            GrayScaleImage = new Bitmap(Image.Width, Image.Height);

            for (int x = 0; x < Image.Width; x++)
            {
                for (int y = 0; y < Image.Height; y++)
                {
                    Color pixelColor = Image.GetPixel(x, y);
                    int grayScale = (int)((pixelColor.R * 0.3) + (pixelColor.G * 0.59) + (pixelColor.B * 0.11));
                    Color newPixelColor = Color.FromArgb(pixelColor.A, grayScale, grayScale, grayScale);
                    GrayScaleImage.SetPixel(x, y, newPixelColor);
                }
            }
        }

        public void LoadGrayScaleImagePixels()
        {
            GrayScaleImagePixels = new List<Pixel>();

            for (int x = 0; x < GrayScaleImage.Width; x++)
            {
                for (int y = 0; y < GrayScaleImage.Height; y++)
                {
                    Color pixelColor = GrayScaleImage.GetPixel(x, y);

                    GrayScaleImagePixels.Add(new Pixel()
                    {
                        PixelColor = pixelColor,
                        CoordinateX = x,
                        CoordinateY = y
                    });
                }
            }
            LoadMaxIntensityPixel();

            RequiredGrayScaleImagePixels = GrayScaleImagePixels.Where(x => x.CoordinateX == MaxIntensityPixel.CoordinateX &&
                x.PixelColor != Color.FromArgb(255, 0, 0, 0)).ToList();
        }


        public Dictionary<string, Pixel> GetHalfIntensityPixels(int angle)
        {
            var result = new Dictionary<string, Pixel>();

            var minIntensityDifference11 = float.MaxValue;
            var minIntensityDifference21 = float.MaxValue;
            var minIntensityDifference12 = float.MaxValue;
            var minIntensityDifference22 = float.MaxValue;
            var halfIntensityPixel11 = new Pixel();
            var halfIntensityPixel21 = new Pixel();
            var halfIntensityPixel12 = new Pixel();
            var halfIntensityPixel22 = new Pixel();
            for (int i = 0; i < RequiredGrayScaleImagePixels.Count(); i++)
            {
                var pixel = RequiredGrayScaleImagePixels[i];
                var transformedX = pixel.CoordinateX - MaxIntensityPixel.CoordinateX;
                var transformedY = pixel.CoordinateY - MaxIntensityPixel.CoordinateY;
                var transformedCurrentX1 = GetRotatedAxisX(transformedX, transformedY, angle);
                var transformedCurrentY1 = GetRotatedAxisY(transformedX, transformedY, angle);
                var transformedCurrentX2 = GetRotatedAxisX(transformedX, transformedY, angle + 90);
                var transformedCurrentY2 = GetRotatedAxisY(transformedX, transformedY, angle + 90);
                var currentX1 = transformedCurrentX1 + MaxIntensityPixel.CoordinateX;
                var currentY1 = transformedCurrentY1 + MaxIntensityPixel.CoordinateY;
                var currentX2 = transformedCurrentX2 + MaxIntensityPixel.CoordinateX;
                var currentY2 = transformedCurrentY2 + MaxIntensityPixel.CoordinateY;
                var halfIntensityPixelInfo11 = GetHalfIntensityPixel(pixel, MaxIntensityPixel, currentX1, currentY1, HalfIntensity,
                    minIntensityDifference11, true);
                var halfIntensityPixelInfo21 = GetHalfIntensityPixel(pixel, MaxIntensityPixel, currentX1, currentY1, HalfIntensity,
                    minIntensityDifference21, false);
                var halfIntensityPixelInfo12 = GetHalfIntensityPixel(pixel, MaxIntensityPixel, currentX2, currentY2, HalfIntensity,
                    minIntensityDifference12, true);
                var halfIntensityPixelInfo22 = GetHalfIntensityPixel(pixel, MaxIntensityPixel, currentX2, currentY2, HalfIntensity,
                    minIntensityDifference22, false);
                if (halfIntensityPixelInfo11 != null)
                {
                    halfIntensityPixel11 = (Pixel)halfIntensityPixelInfo11[0];
                    minIntensityDifference11 = (float)halfIntensityPixelInfo11[1];
                }
                if (halfIntensityPixelInfo21 != null)
                {
                    halfIntensityPixel21 = (Pixel)halfIntensityPixelInfo21[0];
                    minIntensityDifference21 = (float)halfIntensityPixelInfo21[1];
                }
                if (halfIntensityPixelInfo12 != null)
                {
                    halfIntensityPixel12 = (Pixel)halfIntensityPixelInfo12[0];
                    minIntensityDifference12 = (float)halfIntensityPixelInfo12[1];
                }
                if (halfIntensityPixelInfo22 != null)
                {
                    halfIntensityPixel22 = (Pixel)halfIntensityPixelInfo22[0];
                    minIntensityDifference22 = (float)halfIntensityPixelInfo22[1];
                }
            }

            result.Add("halfIntensityPixel11", halfIntensityPixel11);
            result.Add("halfIntensityPixel21", halfIntensityPixel21);
            result.Add("halfIntensityPixel12", halfIntensityPixel12);
            result.Add("halfIntensityPixel22", halfIntensityPixel22);

            return result;
        }

        public void SaveDiametersImage(string imageName, Pixel halfIntensityPixel11, Pixel halfIntensityPixel21, Pixel halfIntensityPixel12, 
            Pixel halfIntensityPixel22)
        {
            GrayScaleImage.SetPixel(MaxIntensityPixel.CoordinateX, MaxIntensityPixel.CoordinateY, Color.Blue);
            GrayScaleImage.SetPixel(halfIntensityPixel11.CoordinateX, halfIntensityPixel11.CoordinateY, Color.Blue);
            GrayScaleImage.SetPixel(halfIntensityPixel21.CoordinateX, halfIntensityPixel21.CoordinateY, Color.Blue);
            GrayScaleImage.SetPixel(halfIntensityPixel12.CoordinateX, halfIntensityPixel12.CoordinateY, Color.Blue);
            GrayScaleImage.SetPixel(halfIntensityPixel22.CoordinateX, halfIntensityPixel22.CoordinateY, Color.Blue);
            GrayScaleImage.Save(string.Format("{0}\\diameters_{1}", Program.DiametersImages, imageName), ImageFormat.Png);
        }

        public int GetCoordinateY(int[] maxIntensityPixelsCoordinateY, IEnumerable<Pixel> maxIntensityPixels, int maxIntensityPixelCoordinateX)
        {
            int maxIntensityPixelCoordinateY = 0;
            for (var n = 0; n < (int)Math.Round((double)maxIntensityPixelsCoordinateY.Count() / 2); n++)
            {
                var currentMaxIntensityPixelCoordinateY = maxIntensityPixelsCoordinateY[n];

                if (maxIntensityPixels.Any(x => x.CoordinateX == maxIntensityPixelCoordinateX && x.CoordinateY == currentMaxIntensityPixelCoordinateY))
                {
                    maxIntensityPixelCoordinateY = currentMaxIntensityPixelCoordinateY;
                }
            }

            return maxIntensityPixelCoordinateY;
        }

        private void LoadMaxIntensityPixel()
        {
            var maxIntensity = GrayScaleImagePixels.Max(x => x.PixelColor.GetBrightness());
            var maxIntensityPixels = GrayScaleImagePixels.Where(x => x.PixelColor.GetBrightness() == maxIntensity);
            if (maxIntensityPixels.Count() == 1)
            {
                MaxIntensityPixel = maxIntensityPixels.FirstOrDefault();
            }
            else
            {
                var maxIntensityPixelsCoordinateX = maxIntensityPixels.OrderBy(x => x.CoordinateX).Select(x => x.CoordinateX).ToArray();
                var maxIntensityPixelsCoordinateY = maxIntensityPixels.OrderBy(x => x.CoordinateY).Select(x => x.CoordinateY).ToArray();
                var maxIntensityPixelCoordinateX = maxIntensityPixelsCoordinateX[(int)Math.Round((double)maxIntensityPixelsCoordinateX.Count() / 2)];
                var maxIntensityPixelCoordinateY = GetCoordinateY(maxIntensityPixelsCoordinateY, maxIntensityPixels, maxIntensityPixelCoordinateX);
                MaxIntensityPixel = maxIntensityPixels.FirstOrDefault(x =>
                    x.CoordinateX == maxIntensityPixelCoordinateX && x.CoordinateY == maxIntensityPixelCoordinateY);
            }
            HalfIntensity = MaxIntensityPixel.PixelColor.GetBrightness() / 2;
        }

        private int GetRotatedAxisX(int x, int y, float rotationAngle)
        {
            return (int)Math.Round(x * Math.Cos(rotationAngle * Math.PI / 180) + y * Math.Sin(rotationAngle * Math.PI / 180), 
                MidpointRounding.AwayFromZero);
        }

        private int GetRotatedAxisY(int x, int y, float rotationAngle)
        {
            return (int)Math.Round(-x * Math.Sin(rotationAngle * Math.PI / 180) + y * Math.Cos(rotationAngle * Math.PI / 180), 
                MidpointRounding.AwayFromZero);
        }

        private object[] GetHalfIntensityPixel(Pixel pixel, Pixel maxIntensityPixel, int currentX, int currentY, float halfIntensity, 
            float minIntensityDifference, bool isMore)
        {
            if (((pixel.CoordinateY < maxIntensityPixel.CoordinateY) && isMore) || ((pixel.CoordinateY > maxIntensityPixel.CoordinateY) && !isMore))
            {
                Color? currentPixelColor = null;
                try
                {
                    currentPixelColor = GrayScaleImage.GetPixel(currentX, currentY);
                }
                catch (Exception)
                {
                }

                if (currentPixelColor == null)
                {
                    return null;
                }

                var currentMinIntensityDifference = Math.Abs(currentPixelColor.Value.GetBrightness() - halfIntensity);
                if (currentMinIntensityDifference < minIntensityDifference)
                {
                    minIntensityDifference = currentMinIntensityDifference;
                    return new object[] { new Pixel 
                    {
                        CoordinateX = currentX,
                        CoordinateY = currentY,
                        PixelColor = (Color)currentPixelColor
                    }, minIntensityDifference };
                }
            }
            return null;
        }
    }
}
