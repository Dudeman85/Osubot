using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace Osubot
{
    class Osubot : Form
    {
        //DLL Imports
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, int rop);

        //Public Vars
        public static Color[] colors = new Color[] { Color.FromArgb(255, 0, 200, 0), Color.FromArgb(255, 0, 0, 200), Color.FromArgb(255, 127, 127, 127), Color.FromArgb(255, 255, 255, 255) };
        public static int[] modePixels = new int[] { 644, 711, 778, 869 };
        public static int[] pixelPositions = new int[] { 372, 407, 475, 542, 612, 680, 713 };
        public static int[] fuckyPixelPositions = new int[] { 372, 407, 475, 542, 612, 799, 866, 934, 1000, 1006};
        public static string[][] allKeys = new string[][] { new string[] { "d", "f", "j", "k" }, new string[] { "d", "f", "b", "j", "k" }, new string[] { "s", "d", "f", "j", "k", "l" },
            new string[] { "s", "d", "f", "b", "j", "k", "l" }, new string[0], new string[0], new string[] { "d", "f", "space", "j", "k", "e", "r", "m", "u", "i"} };
        public static string[] keys;
        public static int speed = 0;
        public static int delay;
        public static int mode = 0;

        public static Bitmap screenPixels = new Bitmap(706, 1);
        public static Color[] cols;
        public static bool[] lockedKey;
        public static bool[] pushedLong;
        public static bool[] upLong;
        public static bool inSong = false;

        //Gets Pixels from the screen starting at x, y and ending at x + width, y + height
        public static Bitmap GetPixels(int x, int y, int width, int height)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixels))
            {
                using (Graphics gsrc = Graphics.FromHwnd(GetDesktopWindow()))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    BitBlt(hDC, 0, 0, width, height, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            return screenPixels;
        }

        //Delay Key Press for Short (Green) Notes
        public static void PushOutputThread(object output)
        {
            Input input = new Input();
            Thread.Sleep(delay);
            input.KeyPress((string[])output, 16);
        }
        //Delayed Key Hold for Long (Blue) Notes
        public static void PushKeyDown(object output)
        {
            Input input = new Input();
            Thread.Sleep(delay);
            input.KeyDown((string[])output);
        }
        //Delayed Key Release for Long (Blue) Notes
        public static void PushKeyUp(object output)
        {
            Input input = new Input();
            Thread.Sleep(delay);
            input.KeyUp((string[])output);
        }

        //Thread for handling long (blue) notes
        public static void LongOutput()
        {
            Input input = new Input();
            List<string> on;
            List<string> off;

            while (true)
            {
                if (inSong)
                {
                    Thread ton = new Thread(new ParameterizedThreadStart(PushKeyDown));
                    Thread toff = new Thread(new ParameterizedThreadStart(PushKeyUp));

                    on = new List<string>();
                    off = new List<string>();

                    for (int i = 0; i < mode; i++)
                    {
                        if (pushedLong[i] && !lockedKey[i])
                        {
                            lockedKey[i] = true;
                            upLong[i] = false;
                            on.Add(keys[i]);
                        }
                        else if (!pushedLong[i] && !upLong[i])
                        {
                            upLong[i] = true;
                            off.Add(keys[i]);
                        }
                    }

                    if (on.Count > 0)
                        ton.Start(on.ToArray());
                    if (off.Count > 0)
                        toff.Start(off.ToArray());
                }

                Thread.Sleep(1);
            }
        }

        static void Main(string[] args)
        {
            while (true)
            {
                //Select Speed
                while (speed < 1 || speed > 40)
                {
                    Console.Write("Enter game speed (1-40): ");
                    try
                    {
                        speed = int.Parse(Console.ReadLine());
                        delay = (int)MathF.Round(4160 * MathF.Pow(speed, -1) - 10);
                    }
                    catch { }
                }
                mode = 4;
                //Initialize Variables depending on the game mode
                keys = allKeys[mode - 4];
                cols = new Color[mode];
                lockedKey = new bool[mode];
                pushedLong = new bool[mode];
                upLong = new bool[mode];

                //Thread for Long (blue) note output
                Thread longOutput = new Thread(new ThreadStart(LongOutput));
                longOutput.Start();
                List<string> output;

                //Main Thread for short (green) notes
                while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                {
                    Thread t = new Thread(new ParameterizedThreadStart(PushOutputThread));
                    output = new List<string>();

                    //Get pixels from the screen
                    Bitmap bitmap = GetPixels(305, 577, 706, 1);
                    //Get the important pixels and their colors
                    for (int i = 0; i < mode; i++)
                    {
                        if(mode < 10)
                            cols[i] = bitmap.GetPixel(pixelPositions[i] - 305, 0);
                        else
                            cols[i] = bitmap.GetPixel(fuckyPixelPositions[i] - 305, 0);
                    }
                    
                    if (bitmap.GetPixel(0, 0) == colors[2] && !inSong)
                    {
                        inSong = true;

                        mode = 4;
                        if (bitmap.GetPixel(modePixels[0] - 305, 0) == colors[3])
                            mode = 5;
                        if (bitmap.GetPixel(modePixels[1] - 305, 0) == colors[3])
                            mode = 6;
                        if (bitmap.GetPixel(modePixels[2] - 305, 0) == colors[3])
                            mode = 7;
                        if (bitmap.GetPixel(modePixels[3] - 305, 0) == colors[3])
                            mode = 10;

                        Console.WriteLine("Entered " + mode + " key song");

                        //Initialize Variables depending on the game mode
                        keys = allKeys[mode - 4];
                        cols = new Color[mode];
                        lockedKey = new bool[mode];
                        pushedLong = new bool[mode];
                        upLong = new bool[mode];
                    }
                    if (Math.Abs(bitmap.GetPixel(0, 0).R - 127) > 50 && inSong)
                    {
                        inSong = false;
                        Console.WriteLine("Exited Song");
                    }

                    //Loop through each row of notes
                    if (inSong)
                    {
                        for (int i = 0; i < mode; i++)
                        {
                            if (cols[i].G >= colors[0].G && cols[i].R <= 200) //Check Pixels for short (green) notes
                            {
                                if (!lockedKey[i])
                                {
                                    lockedKey[i] = true;
                                    output.Add(keys[i]);
                                    continue;
                                }
                                continue;
                            }
                            if (cols[i].B >= colors[1].B && cols[i].R <= 200) //Check Pixels for long (blue) notes
                            {
                                pushedLong[i] = true;
                                continue;
                            }
                            lockedKey[i] = false;
                            pushedLong[i] = false;
                        }

                        if (output.Count > 0)
                        {
                            t.Start(output.ToArray());
                        }
                    }

                    Thread.Sleep(1);
                }
            }
        }
    }
}