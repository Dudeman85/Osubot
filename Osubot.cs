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
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        //Public Vars
        public static Color[] colors = new Color[] { Color.FromArgb(255, 0, 255, 0), Color.FromArgb(255, 0, 0, 255), Color.FromArgb(255, 0, 254, 0) };
        public static int[] pixelPositions = new int[] { 340, 407, 475, 542 };
        public static char[] chars = new char[] { 'd', 'f', 'j', 'k' };

        public static bool[] lockedKey = new bool[4];
        public static bool[] pushedLong = new bool[4];
        public static bool[] upLong = new bool[4];

        static Stopwatch stopWatch = new Stopwatch();

        //Start Methods
        public static Color[] GetColors()
        {
            Color[] cols = new Color[4];

            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            stopWatch.Start();
            for (int i = 0; i < 4; i++)
            {
                int a = (int)GetPixel(dc, pixelPositions[i], 577);
                cols[i] = Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
            }
            ReleaseDC(desk, dc);
            stopWatch.Stop();

            return cols;
        }

        public static void PushOutputThread(object output)
        {
            Input input = new Input();
            Thread.Sleep(775);
            //Console.WriteLine(output);
            input.KeyPress(output.ToString(), 8);
        }

        public static void PushKeyDown(object output)
        {
            Input input = new Input();
            Thread.Sleep(775);
            Console.WriteLine(output.ToString() + "Down");
            input.KeyDown(output.ToString());
        }
        public static void PushKeyUp(object output)
        {
            Input input = new Input();
            Thread.Sleep(775);
            Console.WriteLine(output.ToString() + "UP");
            input.KeyUp(output.ToString());
        }

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

                for (int i = 0; i < 4; i++)
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

                Thread.Sleep(8);
            }
        }

        static void Main(string[] args)
        {
            //Thread longOutput = new Thread(new ThreadStart(LongOutput));
            //longOutput.Start();
            string output;

            while (true)
            {
                stopWatch.Reset();
                //stopWatch.Start();
                Thread t = new Thread(new ParameterizedThreadStart(PushOutputThread));
                output = "";

                Color[] color = GetColors();
                for (int i = 0; i < 4; i++)
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

                Thread.Sleep(0);
                //stopWatch.Stop();
                Console.WriteLine(stopWatch.ElapsedMilliseconds);
            }
        }
    }
}