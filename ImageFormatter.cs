using ImagetoPIM;
using PILCreator;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageToTitleScreen
{
    public static class ImageFormatter
    {
        enum ScreenState { WIDESCREEN = 7, STANDARD = 4 };

        public static void FormatBackground(string image_path)
        {
            Bitmap source_image = new Bitmap(image_path);

            // size of 16 by 9 image scaled to the screen height of 480 px
            Bitmap scaled_image = ResizeImage(source_image, 854, 480);
            // crop the edges of the scaled image so it becomes 4 by 3
            Bitmap standard_image = scaled_image.Clone(new System.Drawing.Rectangle(107, 0, 640, 480), scaled_image.PixelFormat);
            // scale the image down to the expected dimensions from the original game (before stretching horizontally to widescreen in-game)
            Bitmap widescreen_image = ResizeImage(scaled_image, 640, 480);

            // generate pieces for both widescreen (16:9) and standard (4:3) resolution
            if (!GenerateBackgroundPieces(widescreen_image, ScreenState.WIDESCREEN))
                Console.WriteLine("ERROR: Failed to make all 16:9 pieces\n");
            if (!GenerateBackgroundPieces(standard_image, ScreenState.STANDARD))
                Console.WriteLine("ERROR: Failed to make all 4:3 pieces\n");
        }

        // for some god awful reason, the exact logo size is about 400 by 256 pixels tall
        public static void FormatLogo(string logo_path, string backdrop_path)
        {
            string[] intended_file_names = { "ttl0100.png", "ttl0101.png", "ttl0500.png", "ttl0501.png" };
            int[] intended_bit_depths = { 32, 32, 8, 8 };

            var logo_start = new Bitmap(logo_path);
            var backdrop_start = new Bitmap(backdrop_path);

            // resize images to 400 by 256 and add an extra 112 pixels to the right sides of each
            logo_start = ResizeImage(logo_start, 400, 256);
            backdrop_start = ResizeImage(backdrop_start, 400, 256);
            Bitmap logo = new Bitmap(512, 256);
            Bitmap backdrop = new Bitmap(512, 256);
            Graphics g_logo = Graphics.FromImage(logo);
            g_logo.Clear(Color.Transparent);
            Graphics g_backdrop = Graphics.FromImage(backdrop);
            g_backdrop.Clear(Color.Transparent);
            // draw in the pixels (very slow, btu will redo later)
            for (int x = 0; x < 400; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    logo.SetPixel(x, y, logo_start.GetPixel(x, y));
                    backdrop.SetPixel(x, y, backdrop_start.GetPixel(x, y));
                }
            }

            var slices = SliceToLogo(logo, backdrop);
            for (int i = 0; i < slices.Length; i++)
            {
                slices[i].Save(intended_file_names[i], ImageFormat.Png);
                if (!File.Exists(intended_file_names[i]))
                {
                    Console.WriteLine("ERROR: Failed to write PNG file\n");
                    return;
                }
            }
            for (int i = 0; i < slices.Length; i++)
            {
                if (!PIMConverter.ConvertToPIM(intended_file_names[i], intended_bit_depths[i]))
                {
                    Console.WriteLine("ERROR: Failed to write a PIM file\n");
                    return;
                }
                File.Delete(intended_file_names[i]);
                intended_file_names[i] = Path.ChangeExtension(intended_file_names[i], "PIM");
            }
            if (!PILCreator.PILCreator.GeneratePIL(intended_file_names, "TITLE.PIL"))
                Console.WriteLine("ERROR: Failed to write a PIL file\n");
            foreach (var file in intended_file_names)
                File.Delete(file);
            return;
        }

        // add padding to image and slice into pieces
        // assumes source image is 640 by 480 already
        // return true if 6 pieces were made
        private static bool GenerateBackgroundPieces(Bitmap image, ScreenState screen)
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
            bool sliced = SliceToBackground(formatted_image, screen);
            return sliced;
        }

        // slice image into 6 equally sized pieces
        // assumes input is 768 by 512 already
        private static bool SliceToBackground(Bitmap input, ScreenState screen)
        {
            const string PREFIX = "TTL0";
            const string SUFFIX = ".PNG";

            int total = 0;
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Bitmap current_piece = input.Clone(new System.Drawing.Rectangle(256 * x, 256 * y, 256, 256), input.PixelFormat);
                    string output_name = PREFIX + ((int)screen).ToString() + y.ToString() + x.ToString() + SUFFIX;
                    current_piece.Save(output_name);
                    if (File.Exists(output_name))
                        total++;
                }
            }
            return (total == 6);
        }

        // slice logo and backdrop into two 256 by 256 pieces each
        // assumes inputs are 512 by 256
        // outputs pieces to a bitmap array to be converted to PIM files
        private static Bitmap[] SliceToLogo(Bitmap logo, Bitmap backdrop)
        {
            Bitmap[] pieces = new Bitmap[4];
            for (int i = 0; i < 2; i++)
            {
                var logo_piece = logo.Clone(new System.Drawing.Rectangle(256 * i, 0, 256, 256), logo.PixelFormat);
                pieces[i] = logo_piece;
                var backdrop_piece = backdrop.Clone(new System.Drawing.Rectangle(256 * i, 0, 256, 256), backdrop.PixelFormat);
                pieces[i + 2] = backdrop_piece;
            }
            return pieces;
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
