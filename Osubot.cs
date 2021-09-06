using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

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
        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, int rop);


        //Public Vars
        public static Color[] colors = new Color[] { Color.FromArgb(255, 0, 255, 0), Color.FromArgb(255, 0, 0, 255) };
        public static int[] pixelPositions = new int[] { 372, 407, 475, 542, 612, 680, 713 };
        public static char[][] allChars = new char[][] { new char[] { 'd', 'f', 'j', 'k' }, new char[] { 'd', 'f', 'b', 'j', 'k' }, new char[] { 's', 'd', 'f', 'j', 'k', 'l' }, new char[] { 's', 'd', 'f', 'b', 'j', 'k', 'l' } };
        public static char[] chars;
        public static int delay = 510;
        public static int mode;

        public static Bitmap screenPixel = new Bitmap(342, 1);
        public static Color[] cols;
        public static bool[] lockedKey;
        public static bool[] pushedLong;
        public static bool[] upLong;


        //static Stopwatch stopWatch = new Stopwatch();

        //Start Methods
        public static Color[] GetColors()
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(GetDesktopWindow()))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    BitBlt(hDC, 0, 0, 342, 1, hSrcDC, 372, 577, (int)CopyPixelOperation.SourceCopy);
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
            Console.WriteLine(output);
            input.KeyPress(output.ToString(), 16);
        }
        //Delayed Key Hold for Long (Blue) Notes
        public static void PushKeyDown(object output)
        {
            Input input = new Input();
            Thread.Sleep(delay);
            Console.WriteLine(output.ToString() + "Down");
            input.KeyDown(output.ToString());
        }
        //Delayed Key Release for Long (Blue) Notes
        public static void PushKeyUp(object output)
        {
            Input input = new Input();
            Thread.Sleep(delay - 45);
            Console.WriteLine(output.ToString() + "UP");
            input.KeyUp(output.ToString());
        }

        //Thread for handling long (blue) notes
        public static void LongOutput()
        {
            Input input = new Input();
            string on;
            string off;

            while (true)
            {
                Thread ton = new Thread(new ParameterizedThreadStart(PushKeyDown));
                Thread toff = new Thread(new ParameterizedThreadStart(PushKeyUp));

                on = "";
                off = "";

                for (int i = 0; i < mode; i++)
                {
                    if (pushedLong[i] && !lockedKey[i])
                    {
                        lockedKey[i] = true;
                        upLong[i] = false;
                        on += chars[i];
                    }
                    else if (!pushedLong[i] && !upLong[i])
                    {
                        off += chars[i];
                        upLong[i] = true;
                    }
                }

                if (on != "")
                    ton.Start(on);
                if (off != "")
                    toff.Start(off);

                Thread.Sleep(1);
            }
        }

        static void Main(string[] args)
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
            //Initialize Variables depending on the game mode
            chars = allChars[mode - 4];
            cols = new Color[mode];
            lockedKey = new bool[mode];
            pushedLong = new bool[mode];
            upLong = new bool[mode];

            //Thread for Long (blue) note output
            Thread longOutput = new Thread(new ThreadStart(LongOutput));
            longOutput.Start();
            string output;

            //Main Thread
            while (true)
            {
                //stopWatch.Reset();
                //stopWatch.Start();
                Thread t = new Thread(new ParameterizedThreadStart(PushOutputThread));
                output = "";

                Color[] color = GetColors();
                for (int i = 0; i < mode; i++)
                {
                    if (color[i] == colors[0])
                    {
                        if (!lockedKey[i])
                        {
                            lockedKey[i] = true;
                            output += chars[i];
                            continue;
                        }
                        continue;
                    }
                    if (color[i] == colors[1])
                    {
                        pushedLong[i] = true;
                        continue;
                    }
                    lockedKey[i] = false;
                    pushedLong[i] = false;
                }

                if (output != "")
                {
                    t.Start(output);
                }

                Thread.Sleep(2);
                //stopWatch.Stop();
                //Console.WriteLine(stopWatch.ElapsedMilliseconds);
            }
        }
    }
}