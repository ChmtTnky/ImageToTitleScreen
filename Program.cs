namespace ImageToTitleScreen
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Formats either a 16 by 9 image to use as the title screen background, or a pair of 25 by 16 (yes, really) images to use as the title screen logo\n" +
                    "The input images don't need to have the same dimensions as the originals, but it works best if they are the same\n" +
                    "The final output will either be 12 PNGs (background) or 1 PIL file (logo) that can be applied with the KingdomFileReplacer\n" +
                    "If a single file path is provided, Background mode is assumed\n" +
                    "If a folder path is provided, this will search for images with the names \"logo\" and \"backdrop\" to use automatically in Logo mode\n" +
                    "If two file paths are provided, the first path will be used as the logo and the second will be used as the backdrop in Logo mode\n" +
                    "Example Args:\n" +
                    "ImageToTitleScreen.exe <Path to Image/Folder>\n" +
                    "ImageToTitleScreen.exe <Path to Logo> <Path to Backdrop>\n");
                return;
            }
            else if (args.Length == 1)
            {
                // If path is a file, generate background.
                // If path is a directory, find "logo" and "backdrop" and use them to make the logo PIL.
                if (File.Exists(args[0]))
                    ImageFormatter.FormatBackground(args[0]);
                else if (Directory.Exists(args[0]))
                {
                    var files = Directory.GetFiles(args[0]);
                    string logo = string.Empty, backdrop = string.Empty;
                    foreach (var file in files)
                    {
                        if (Path.GetFileNameWithoutExtension(file) == "logo")
                        {
                            logo = file;
                            continue;
                        }
                        if (Path.GetFileNameWithoutExtension(file) == "backdrop")
                        {
                            backdrop = file;
                            continue;
                        }
                    }
                    if (logo == string.Empty || backdrop == string.Empty)
                    {
                        Console.WriteLine(
                            "Example Args:\n" +
                            "ImageToTitleScreen.exe <Path to Folder>\n");
                        return;
                    }
                    ImageFormatter.FormatLogo(logo, backdrop);
                }
            }
            else if (args.Length == 2)
            {
                string logo = args[0], backdrop = args[1];
                if (!File.Exists(logo) || !File.Exists(backdrop))
                {
                    Console.WriteLine(
                            "Example Args:\n" +
                            "ImageToTitleScreen.exe <Path to Logo> <Path to Backdrop>\n");
                    return;
                }
                ImageFormatter.FormatLogo(logo, backdrop);
            }
            else
            {
                Console.WriteLine(
                    "Invalid arguments\n" +
                    "Example Args:\n" +
                    "ImageToTitleScreen.exe <Path to Image/Folder>\n" +
                    "ImageToTitleScreen.exe <Path to Logo> <Path to Backdrop>\n");
                return;
            }
        }
    }
}