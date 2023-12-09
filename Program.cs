using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System;
using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


public class PaletteGenerator
{
    public static List<Color> palette1 = new List<Color> {
        Color.Red,
        Color.Green,
        Color.Blue,
        Color.Orange,
        Color.Orchid,
        Color.Pink,
        Color.Purple,
        Color.White,
        Color.Yellow,
        Color.Black,
        Color.Gray,
        Color.LightGray,
        Color.DarkGray,
        Color.DarkGreen,
        Color.LightGreen,
        Color.DarkOrange,
    };
    public static int block_size;
    public static List<string> CreateRgbHexLines(List<Color> colors)
    {
        // Create a list of strings representing the colors in the palette as RGB hexadecimal values
        var lines = new List<string>();
        foreach (var color in colors)
        {
            lines.Add($"#{color.R:X2}{color.G:X2}{color.B:X2}");
        }
        // Return the list
        return lines;
    }
    public static void FillBlock(Bitmap bitmap, int startX, int startY, int blockSize, Color color)
    {
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, startX, startY, blockSize, blockSize);
            }
        }
    }
    public static Color GetClosestColor(Color targetColor, List<Color> palette)
    {
        Color closestColor = palette[0];
        int closestDistance = int.MaxValue;

        foreach (Color color in palette)
        {
            int distance = Math.Abs(targetColor.R - color.R) + Math.Abs(targetColor.G - color.G) + Math.Abs(targetColor.B - color.B);
            if (distance < closestDistance)
            {
                closestColor = color;
                closestDistance = distance;
            }
        }

        return closestColor;
    }
    static Color GetAverageColor(Bitmap bitmap, int startX, int startY, int blockSize)
    {
        int totalR = 0, totalG = 0, totalB = 0;
        int count = 0;

        for (int y = startY; y < startY + blockSize && y < bitmap.Height; y++)
        {
            for (int x = startX; x < startX + blockSize && x < bitmap.Width; x++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                totalR += pixelColor.R;
                totalG += pixelColor.G;
                totalB += pixelColor.B;
                count++;
            }
        }
        if (count > 0)
        {
            return Color.FromArgb(totalR / count, totalG / count, totalB / count);
        }
        else
        {
            // Handle the case where count is zero (e.g., return a default color)
            return GetClosestColor(bitmap.GetPixel(startX, startY), palette1); // You can change this to a suitable default color
        }
    }
    
    public static void Generate(Bitmap bitmap, int numClusters)
    {
        // Extract the colors from the bitmap
        var colors = ExtractColors(bitmap);

        // Cluster the colors using K-Means
        var clusters = KMeansCluster(colors, numClusters);

        // Select the most representative color from each cluster
        var dominantColors = new List<Color>();

        int width = bitmap.Width;
        int height = bitmap.Height;

        foreach (var cluster in clusters)
        {
            var representative = cluster.OrderByDescending(c => c.Value).First().Key;
            dominantColors.Add(representative);
        }

        //dominantColors = new List<Color> { Color.Red, Color.Green, Color.Blue, Color.Yellow };
        Bitmap outputBitmap = new Bitmap(bitmap.Width, bitmap.Height);
        for (int x = 0; x < bitmap.Width; x += block_size)
        {
            for (int y = 0; y < bitmap.Height; y += block_size)
            {
                Color averageColor = GetAverageColor(bitmap, x, y, block_size);
                Color closestColor = GetClosestColor(averageColor, dominantColors);
                FillBlock(outputBitmap, x, y, block_size, closestColor);
            }
        }
    }

    static Color GetNearestPaletteColor(Color color, List<Color> palette)
    {
        // Find the nearest color in the custom palette
        Color nearestColor = palette[0];
        double minDistance = Distance(color, nearestColor);

        foreach (Color paletteColor in palette)
        {
            double distance = Distance(color, paletteColor);
            if (distance < minDistance)
            {
                nearestColor = paletteColor;
                minDistance = distance;
            }
        }
        return nearestColor;
    }
    static Dictionary<Color, int> ExtractColors(Bitmap bitmap)
    {
        // Create a dictionary to store the colors and their frequencies
        var colors = new Dictionary<Color, int>();

        // Iterate over the pixels in the bitmap
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                if (colors.ContainsKey(color))
                {
                    colors[color]++;
                }
                else
                {
                    colors[color] = 1;
                }
            }
        }
        return colors;
    }

    static List<Dictionary<Color, int>> KMeansCluster(Dictionary<Color, int> colors, int numClusters)
    {
        // Initialize the clusters
        var clusters = new List<Dictionary<Color, int>>();
        for (int i = 0; i < numClusters; i++)
        {
            clusters.Add(new Dictionary<Color, int>());
        }

        // Select the initial cluster centers randomly
        var centers = colors.Keys.OrderBy(c => Guid.NewGuid()).Take(numClusters).ToArray();

        // Loop until the clusters stabilize
        var changed = true;
        while (changed)
        {
            changed = false;
            // Assign each color to the nearest cluster center
            foreach (var color in colors.Keys)
            {
                var nearest = FindNearestCenter(color, centers);
                var clusterIndex = Array.IndexOf(centers, nearest);
                clusters[clusterIndex][color] = colors[color];
            }
            int count = 0;
            // Recompute the cluster centers
            for (int i = 0; i < numClusters; i++)
            {
                var sumR = 0;
                var sumG = 0;
                var sumB = 0;
                foreach (var color in clusters[i].Keys)
                {
                    sumR += color.R;
                    sumG += color.G;
                    sumB += color.B;
                    count++;
                }
                if (count > 0)
                {
                    var r = (byte)(sumR / count);
                    var g = (byte)(sumG / count);
                    var b = (byte)(sumB / count);
                    var newCenter = Color.FromArgb(r, g, b);
                    if (!newCenter.Equals(centers[i]))
                    {
                        centers[i] = newCenter;
                        changed = true;
                    }
                } else
                {
                    var newCenter = palette1[i];
                }
            }
        }

        // Return the clusters
        return clusters;
    }

    static Color FindNearestCenter(Color color, Color[] centers)
    {
        var nearest = centers[0];
        var minDist = double.MaxValue;
        foreach (var center in centers)
        {
            var dist = Distance(color, center);
            if (dist < minDist)
            {
                nearest = center;
                minDist = dist;
            }
        }

        return nearest;
    }

    static double Distance(Color c1, Color c2)
    {
        var r = c1.R - c2.R;
        var g = c1.G - c2.G;
        var b = c1.B - c2.B;
        return Math.Sqrt(r * r + g * g + b * b);
    }
}
public static class ImageChanging1
{
    public static Bitmap KMeansClusteringSegmentation(this Bitmap image, int clusters)
    {
        int w = image.Width;
        int h = image.Height;

        BitmapData image_data = image.LockBits(
            new Rectangle(0, 0, w, h),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        int bytes = image_data.Stride * image_data.Height;
        byte[] buffer = new byte[bytes];

        Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
        image.UnlockBits(image_data);

        byte[] result = new byte[bytes];
        int[] means = new int[clusters];
        Random rnd = new Random();

        for (int i = 0; i < clusters; i++)
        {
            int init_mean = rnd.Next(1, 255);
            while (means.Contains((byte)init_mean))
            {
                init_mean = rnd.Next(1, 255);
            }
            means[i] = (byte)init_mean;
        }

        double error = new double();
        List<byte>[] samples = new List<byte>[clusters];

        while (true)
        {
            for (int i = 0; i < clusters; i++)
            {
                samples[i] = new List<byte>();
            }

            for (int i = 0; i < bytes; i += 3)
            {
                double norm = 999;
                int cluster = 0;

                for (int j = 0; j < clusters; j++)
                {
                    double temp = Math.Abs(buffer[i] - means[j]);
                    if (norm > temp)
                    {
                        norm = temp;
                        cluster = j;
                    }
                }
                samples[cluster].Add(buffer[i]);

                for (int c = 0; c < 3; c++)
                {
                    result[i + c] = (byte)means[cluster];
                }
            }

            int[] new_means = new int[clusters];

            for (int i = 0; i < clusters; i++)
            {
                for (int j = 0; j < samples[i].Count(); j++)
                {
                    new_means[i] += samples[i][j];
                }

                new_means[i] /= (samples[i].Count() + 1);
            }

            int new_error = 0;

            for (int i = 0; i < clusters; i++)
            {
                new_error += Math.Abs(means[i] - new_means[i]);
                means[i] = new_means[i];
            }

            if (error == new_error)
            {
                break;
            }
            else
            {
                error = new_error;
            }
        }

        Bitmap res_img = new Bitmap(w, h);
        return res_img;
    }
    private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
    {
        return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
    }
    public static Bitmap resizeImage(Bitmap image,
                     int canvasWidth, int canvasHeight)
    {
        //Image image = Image.FromFile(path);
        System.Drawing.Image thumbnail = new Bitmap(canvasWidth, canvasHeight); // changed parm names
        System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(thumbnail);

        graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphic.SmoothingMode = SmoothingMode.HighQuality;
        graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphic.CompositingQuality = CompositingQuality.HighQuality;

        int origWidth = image.Width;
        int origHeight = image.Height;

        // To preserve the aspect ratio
        float ratioX = (float)canvasWidth / (float)origWidth;
        float ratioY = (float)canvasHeight / (float)origHeight;
        float ratio = Math.Min(ratioX, ratioY);

        //        // now we can get the new height and width
        int newHeight = Convert.ToInt32(origHeight * ratio);
        int newWidth = Convert.ToInt32(origWidth * ratio);

        //        // Now calculate the X,Y position of the upper-left corner 
        //        // (one of these will always be zero)
        int posX = Convert.ToInt32((canvasWidth - (origWidth * ratio)) / 2);
        int posY = Convert.ToInt32((canvasHeight - (origHeight * ratio)) / 2);

        graphic.Clear(Color.White); // white padding
        graphic.DrawImage(image, posX, posY, newWidth, newHeight);
        return image;
    }
    public static void SaveWith(this Bitmap b)
    {
        // Get an ImageCodecInfo object that represents the JPEG codec.
        ImageCodecInfo imageCodecInfo = ImageChanging1.GetEncoderInfo(ImageFormat.Jpeg);

        // Create an Encoder object for the Quality parameter.
        System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

        // Create an EncoderParameters object. 
        EncoderParameters encoderParameters = new EncoderParameters(1);

        // Save the image as a JPEG file with quality level.

        EncoderParameter encoderParameter = new EncoderParameter(encoder, 100L);
        encoderParameters.Param[0] = encoderParameter;
        //Directory.CreateDirectory(folder);
        //string path = folder + @"\" + name + ".jpg";
        string path = @"C:\Users\Tony\Downloads\pixels2\pixels2\noviy.jpg";
        b.Save(path, imageCodecInfo, encoderParameters);
    }


    internal static class Program
    {
        public static void Main(string[] args)
        {
            //var folder = System.IO.Path.GetDirectoryName(@"C:\Users\Tony\Desktop");
            //string filepath = @"C:\spidersona.png";
            //PaletteGenerator.Generate("C:\\blep.png", 300);
            //PaletteGenerator.CreateRgbHexLines(PaletteGenerator.KmeansCluster)
        }
    }
}

