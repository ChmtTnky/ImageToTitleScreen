namespace ImageToTitleScreen
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Formats a 16 by 9 image to work as a title screen background for Dokapon Kingdom\n" +
                    "The image doesn't have to be 16 by 9, but it works best that way in-game\n" +
                    "The final output will be 12 images that can be applied using the KingdomFileReplacer\n" +
                    "Example: ImageToTitleScreen.exe <Path to Image>\n");
                return;
            }
            if (args.Length != 1)
            {
                Console.WriteLine(
                    "Invalid arguments\n" +
                    "Example: ImageToTitleScreen.exe <Path to Image>");
                return;
            }
            ImageFormatter.FormatImage(args[0]);
        }
    }
}