using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2048_solver
{
    class WindowsFunctions
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static void hideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);
        }

        public static void performMove(Direction direction)
        {
            switch (direction)
            {
                case Direction.left:
                    SendKeys.SendWait("{LEFT}");
                    break;
                case Direction.right:
                    SendKeys.SendWait("{RIGHT}");
                    break;
                case Direction.up:
                    SendKeys.SendWait("{UP}");
                    break;
                case Direction.down:
                    SendKeys.SendWait("{DOWN}");
                    break;
                default:
                    break;
            }
        }

        public static string GetActiveProcessFileName()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                Process p = Process.GetProcessById((int)pid);

                return p.MainModule.FileName;
            }
            catch
            {
                return "";
            }
        }

        public static Image<Bgr, Byte> captureScreen()
        {
            //annoyingly my screen is retina, which means there are twice as many pixels as reported by Screen.getBounds()
            //and i cant find a reliable way to detect this in c#
            int pixelDensity = 2;

            Rectangle bounds = Screen.GetBounds(Point.Empty);
            bounds.Width *= pixelDensity;
            bounds.Height *= pixelDensity;

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }

                Image<Bgr, Byte> img = new Image<Bgr, Byte>(bitmap);

                if (pixelDensity != 1)
                {
                    //after capturing at a higher density, we scale back down
                    Image<Bgr, byte> resizedImage = img.Resize(img.Width / pixelDensity, img.Height / pixelDensity, Emgu.CV.CvEnum.Inter.Linear);

                    img.Dispose();
                    img = resizedImage;
                }

                return img;
            }
        }

        /// <summary>
        /// returns true if it found a chrome window and displayed it
        /// </summary>
        public static bool BringMainWindowToFront(string processName)
        {
            //chrome usually has lots of processes running, all with the same name, but only one has a window
            Process mainChromeProcess = Process.GetProcessesByName("chrome")
                                               .Where(x => x.MainWindowHandle != IntPtr.Zero)
                                               .FirstOrDefault();

            if (mainChromeProcess == null)
            {
                return false;
            }

            SetForegroundWindow(mainChromeProcess.MainWindowHandle);

            return true;
        }
    }
}
