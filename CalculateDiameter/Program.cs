using System;
using System.Collections.Generic;
using System.IO;

namespace CalculateDiameter
{
    class Program
    {
        public static string DiametersImages = "DiametersImages";
        public static string OriginalImages = "OriginalImages";
        static void Main(string[] args)
        {
            if (!Directory.Exists(OriginalImages))
            {
                Directory.CreateDirectory(OriginalImages);
            }

            if (!Directory.Exists(DiametersImages))
            {
                Directory.CreateDirectory(DiametersImages);
            }
            else
            {
                ClearDiametersImagesDirectory();
            }

            var filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
            var imageFiles = GetFilesFrom(AppContext.BaseDirectory + OriginalImages, filters, false);

            foreach (var imageFile in imageFiles)
            {
                var startTime = DateTime.Now;

                var halfIntensityPixel11 = new Pixel();
                var halfIntensityPixel21 = new Pixel();
                var halfIntensityPixel12 = new Pixel();
                var halfIntensityPixel22 = new Pixel();

                var imageIntensity = new ImageIntensity(imageFile);
                imageIntensity.ConvertImageToGrayScale();
                imageIntensity.LoadGrayScaleImagePixels();

                float distance1 = 0;
                float distance2 = 0;
                for (int angle = 0; angle <= 180; angle++)
                {
                    try
                    {
                        var dataDictionary = imageIntensity.GetHalfIntensityPixels(angle);

                        var currentHalfIntensityPixel11 = dataDictionary["halfIntensityPixel11"];
                        var currentHalfIntensityPixel21 = dataDictionary["halfIntensityPixel21"];
                        var currentHalfIntensityPixel12 = dataDictionary["halfIntensityPixel12"];
                        var currentHalfIntensityPixel22 = dataDictionary["halfIntensityPixel22"];

                        float currentDistance1 = (float)Math.Sqrt(
                            Math.Pow(currentHalfIntensityPixel11.CoordinateX - currentHalfIntensityPixel21.CoordinateX, 2) +
                            Math.Pow(currentHalfIntensityPixel11.CoordinateY - currentHalfIntensityPixel21.CoordinateY, 2));
                        float currentDistance2 = (float)Math.Sqrt(
                            Math.Pow(currentHalfIntensityPixel12.CoordinateX - currentHalfIntensityPixel22.CoordinateX, 2) +
                            Math.Pow(currentHalfIntensityPixel12.CoordinateY - currentHalfIntensityPixel22.CoordinateY, 2));

                        if (currentDistance1 > distance1)
                        {
                            distance1 = currentDistance1;
                            distance2 = currentDistance2;

                            halfIntensityPixel11 = currentHalfIntensityPixel11;
                            halfIntensityPixel21 = currentHalfIntensityPixel21;
                            halfIntensityPixel12 = currentHalfIntensityPixel12;
                            halfIntensityPixel22 = currentHalfIntensityPixel22;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                var imageName = Path.GetFileName(imageFile);

                imageIntensity.SaveDiametersImage(imageName, halfIntensityPixel11, halfIntensityPixel21, halfIntensityPixel12, halfIntensityPixel22);

                var stopTime = DateTime.Now;
                TimeSpan executionTime = stopTime - startTime;
                if (distance1 >= distance2)
                {
                    PrintResult(imageName, distance1, distance2, executionTime);
                }
                else
                {
                    PrintResult(imageName, distance2, distance1, executionTime);
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void PrintResult(string imageName, float distance1, float distance2, TimeSpan executionTime)
        {
            Console.WriteLine(string.Format("{0}: d1 = {1:F5} ; d2 = {2:F5} | execution time: {3:F5} s", 
                imageName, 
                distance1, 
                distance2, 
                executionTime.TotalSeconds));
        }

        static string[] GetFilesFrom(string searchFolder, string[] filters, bool isRecursive)
        {
            List<string> filesFound = new List<string>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        static void ClearDiametersImagesDirectory()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(DiametersImages);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
