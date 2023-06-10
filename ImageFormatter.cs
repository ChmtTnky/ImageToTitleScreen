using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageToTitleScreen
{
    public static class ImageFormatter
    {
        const string PREFIX = "TTL0";
        const string SUFFIX = ".PNG";

        enum ScreenState { WIDESCREEN = 7, STANDARD = 4 };

        public static void FormatImage(string image_path)
        {
            Bitmap source_image = new Bitmap(image_path);

            // size of 16 by 9 image scaled to the screen height of 480 px
            Bitmap scaled_image = ResizeImage(source_image, 854, 480);
            Bitmap standard_image = scaled_image.Clone(new System.Drawing.Rectangle(107, 0, 640, 480), scaled_image.PixelFormat);
            Bitmap widescreen_image = ResizeImage(scaled_image, 640, 480);

            // generate pieces for both widescreen (16:9) and standard (4:3) resolution
            if (!GeneratePieces(widescreen_image, ScreenState.WIDESCREEN))
                Console.WriteLine("ERROR: Failed to make all 16:9 pieces");
            if (!GeneratePieces(standard_image, ScreenState.STANDARD))
                Console.WriteLine("ERROR: Failed to make all 4:3 pieces");
        }

        // add padding to image and slice into pieces
        // assumes source image is 640 by 480 already
        // return true if 6 pieces were made
        private static bool GeneratePieces(Bitmap image, ScreenState screen)
        {
            // game uses six 256 by 256 images with black padding the right and bottom edges. the actual size is 640 by 480
            Bitmap formatted_image = new Bitmap(768, 512);
            Graphics g = Graphics.FromImage(formatted_image);
            g.Clear(Color.Black);
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    formatted_image.SetPixel(x, y, image.GetPixel(x, y));
                }
            }
            int slices = SliceImage(formatted_image, screen);
            return (slices == 6);
        }

        // slice image into 6 pieces
        // assumes input is 768 by 512 already
        // output the number of images that were made
        private static int SliceImage(Bitmap input, ScreenState screen)
        {
            int total = 0;
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Bitmap current_piece = input.Clone(new System.Drawing.Rectangle(256 * x, 256 * y, 256, 256), input.PixelFormat);
                    current_piece.Save(PREFIX + (int)screen + y + x + SUFFIX);
                    if (File.Exists(PREFIX + (int)screen + y + x + SUFFIX))
                        total++;
                }
            }
            return total;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
