using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Ez3Di.Installer.Core
{
    /// <summary>
    /// Multi-monitor visual element interaction using OpenCV template matching (CCoeffNormed).
    /// Detects installer UI templates across any connected screen (VirtualScreen spanning all monitors).
    /// </summary>
    public static class ImageCompare
    {
        public const double DefaultThreshold = 0.85;

        private static string TemplatesFolder =>
            Path.Combine(InstallerConstants.SolutionRoot, "Assets", "Templates");

        private static readonly ConcurrentDictionary<string, Mat> _templateCache = new();

        /// <summary>
        /// Locates the template PNG across any monitor and clicks its center coordinate.
        /// </summary>
        public static bool Click(string templateFileName, IntPtr hwnd = default, double threshold = DefaultThreshold)
        {
            var pt = Find(templateFileName, threshold);
            if (pt == null)
            {
                Console.WriteLine($"[ImageCompare] Template '{templateFileName}' not found on any screen (threshold: {threshold:P0}).");
                return false;
            }

            Console.WriteLine($"[ImageCompare] Clicking '{templateFileName}' at virtual coordinate ({pt.Value.X}, {pt.Value.Y})...");
            InputDriver.ClickAtPhysicalCoordinate(hwnd, pt.Value.X, pt.Value.Y);
            return true;
        }

        /// <summary>
        /// Searches for a template PNG across the combined virtual desktop (all screens).
        /// Returns the global virtual screen center point if matched, or null if not found.
        /// </summary>
        public static System.Drawing.Point? Find(string templateFileName, double threshold = DefaultThreshold)
        {
            string path = Path.Combine(TemplatesFolder, templateFileName);
            if (!File.Exists(path))
            {
                Console.WriteLine($"[ImageCompare] Warning: Template file not found: '{path}'");
                return null;
            }

            Rectangle vScreen = SystemInformation.VirtualScreen;
            try
            {
                using var bmp = new Bitmap(vScreen.Width, vScreen.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(vScreen.Left, vScreen.Top, 0, 0, vScreen.Size, CopyPixelOperation.SourceCopy);
                }

                using var sceneRaw = BitmapConverter.ToMat(bmp);
                using var scene = new Mat();
                if (sceneRaw.Channels() == 4)
                    Cv2.CvtColor(sceneRaw, scene, ColorConversionCodes.BGRA2BGR);
                else
                    sceneRaw.CopyTo(scene);

                var template = GetCachedTemplate(path);
                if (template.Width > scene.Width || template.Height > scene.Height)
                    return null;

                using var result = new Mat();
                Cv2.MatchTemplate(scene, template, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                Console.WriteLine($"[ImageCompare] Search '{templateFileName}' across VirtualScreen [{vScreen.Width}x{vScreen.Height}] — Confidence: {maxVal:P1} (Req: {threshold:P0})");

                if (maxVal >= threshold)
                {
                    int centerX = vScreen.Left + maxLoc.X + template.Width / 2;
                    int centerY = vScreen.Top + maxLoc.Y + template.Height / 2;

                    // Identify which monitor contains this point
                    var targetScreen = Screen.FromPoint(new System.Drawing.Point(centerX, centerY));
                    Console.WriteLine($"[ImageCompare] MATCHED '{templateFileName}' on display {targetScreen.DeviceName} at ({centerX}, {centerY})");
                    return new System.Drawing.Point(centerX, centerY);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageCompare] Error during template search: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Asserts that a template is visible on any screen.
        /// </summary>
        public static bool See(string templateFileName, double threshold = DefaultThreshold)
        {
            return Find(templateFileName, threshold) != null;
        }

        private static Mat GetCachedTemplate(string path)
        {
            return _templateCache.GetOrAdd(path, p =>
            {
                using var raw = Cv2.ImRead(p, ImreadModes.Unchanged);
                if (raw.Empty())
                    throw new FileNotFoundException($"Cannot load image template: '{p}'");

                var tMat = new Mat();
                if (raw.Channels() == 4)
                    Cv2.CvtColor(raw, tMat, ColorConversionCodes.BGRA2BGR);
                else if (raw.Channels() == 1)
                    Cv2.CvtColor(raw, tMat, ColorConversionCodes.GRAY2BGR);
                else
                    raw.CopyTo(tMat);

                return tMat;
            });
        }
    }
}
