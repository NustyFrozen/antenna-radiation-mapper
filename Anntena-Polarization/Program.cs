using System.Runtime.InteropServices;

namespace Anntena_Polarization
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        public static string filename;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".png,.jpeg";
            dialog.Title = "בחר דיאגרמה";
            var results = dialog.ShowDialog();
            if (results == DialogResult.OK)
            {
                AllocConsole();
                UI overlay = new UI();
               
                overlay.origin = new Bitmap(dialog.FileNames[0]);
                filename = dialog.FileNames[0];
                overlay.Run();
                Polarizer.polar temp = new Polarizer.polar(new System.Numerics.Vector2(1,20));
                Console.WriteLine($"{temp.angle}:{temp.magnitude}");
                Console.WriteLine($"{temp.toCartisian().X}:{temp.toCartisian().X}");

                while (true)
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}