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
        public static Color[] colors = new Color[] { Color.FromArgb(255, 0, 200, 0), Color.FromArgb(255, 0, 0, 200), Color.FromArgb(255, 127, 127, 127) };
        public static int[] pixelPositions = new int[] { 372, 407, 475, 542, 612, 680, 713 };
        public static string[][] allKeys = new string[][] { new string[] { "d", "f", "j", "k" }, new string[] { "d", "f", "b", "j", "k" }, new string[] { "s", "d", "f", "j", "k", "l" }, 
            new string[] { "s", "d", "f", "b", "j", "k", "l" }, new string[] { "s", "d", "space", "j", "k", "e", "r", "ralt", "u", "i"} };
        public static string[] keys;
        public static int speed = 8;
        public static int delay;
        public static int mode;

        public static Bitmap screenPixel = new Bitmap(342, 1);
        public static Color[] cols;
        public static bool[] lockedKey;
        public static bool[] pushedLong;
        public static bool[] upLong;

        //Start Methods
        public static Color[] GetColors()
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(GetDesktopWindow()))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    BitBlt(hDC, 0, 0, 706, 1, hSrcDC, 305, 577, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            for (int i = 0; i < mode; i++)
            {
                cols[i] = screenPixel.GetPixel(pixelPositions[i] - pixelPositions[0], 0);
            }
            return cols;
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

                Thread.Sleep(1);
            }
        }

        static void Main(string[] args)
        {
            while (true)
            {
                //Select 4, 5,or 7 key mode
                while (mode != 4 && mode != 5 && mode != 6 && mode != 7)
                {
                    Console.Write("\n4, 5, 6, or 7 key: ");
                    try
                    {
                        mode = int.Parse(Console.ReadKey().KeyChar.ToString());
                    }
                    catch { }
                }

                //Not Done
                speed = int.Parse(Console.ReadLine());
                delay = (int)MathF.Round(4160 * MathF.Pow(speed, -1) - 10);

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

                //Main Thread
                while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                {
                    Thread t = new Thread(new ParameterizedThreadStart(PushOutputThread));
                    output = new List<string>();

                    Color[] color = GetColors();
                    for (int i = 0; i < mode; i++)
                    {
                        if (color[i].G >= colors[0].G && color[i].R <= 200) //Check Pixels for short (green) notes
                        {
                            if (!lockedKey[i])
                            {
                                lockedKey[i] = true;
                                output.Add(keys[i]);
                                continue;
                            }
                            continue;
                        }
                        if (color[i].B >= colors[1].B && color[i].R <= 200) //Check Pixels for long (blue) notes
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

                    Thread.Sleep(1);
                }
            }
        }
    }
}