using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2048_solver
{
    class Program
    {
        const bool debug = false;

        static unsafe void Main(string[] args)
        {
            Rectangle grid;
            if (debug == false)
            {
                Console.WriteLine("Press Enter to Start");
                Console.ReadLine();

                WindowsFunctions.BringMainWindowToFront("chrome");
                Thread.Sleep(500);

                Console.WriteLine("Capturing Screen");
                Image<Bgr, byte> screenshot = WindowsFunctions.captureScreen();

                Console.WriteLine("Finding Grid");
                grid = OpenCVFunctions.findGridInImage(screenshot);
            }

            //GridFunctions.initializeCaches();

            while (debug || Path.GetFileNameWithoutExtension(WindowsFunctions.GetActiveProcessFileName()).ToLower() == "chrome")
            {
                byte[] managedValues;
                if (debug)
                {
                    //managedValues = new byte[] { 4, 5, 4, 3, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 1, 0 };
                    managedValues = new byte[] {
                        5, 4, 3, 3,
                        0, 0, 0, 3,
                        0, 0, 0, 1,
                        0, 0, 1, 0 };

                    //managedValues = new byte[] {
                    //    0, 1, 2, 2,
                    //    0, 0, 0, 0,
                    //    0, 0, 0, 0,
                    //    0, 0, 0, 0 };
                }
                else
                {
                    Console.WriteLine("Capturing Screen");
                    Image<Bgr, byte> screenshot = WindowsFunctions.captureScreen();

                    Console.WriteLine("Finding Grid");
                    managedValues = OpenCVFunctions.captureGridFromImage(screenshot, grid);
                }

                Stopwatch sw;
                if (debug)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }

                GridStack stack = new GridStack(6);

                //we copy the first grid into the buffer
                //maybe move this into the stack
                fixed (byte* temp = managedValues)
                {
                    GridFunctions.memcpy((IntPtr)stack.current, (IntPtr)temp, new UIntPtr(16 * sizeof(byte)));
                }
                //GridFunctions.printGrid(stack.current);

                var outcome = runPermutation(stack, 1);
                Console.WriteLine(outcome.Key);

                if (debug)
                {
                    sw.Stop();
                    Debug.WriteLine(sw.ElapsedMilliseconds);
                }
                else
                {
                    WindowsFunctions.performMove(outcome.Key);
                    Thread.Sleep(500);
                }

                ///move this to the stack
                //Marshal.FreeHGlobal((IntPtr)gridValues);
            }

            string s = Path.GetFileNameWithoutExtension(WindowsFunctions.GetActiveProcessFileName()).ToLower();
        }

        private static unsafe KeyValuePair<Direction, int> runPermutation(GridStack stack, int ttl)
        {
            //GridFunctions.printGrid(stack.current);
            Dictionary<Direction, int> scores = new Dictionary<Direction, int>();

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                //hint: buffer is not just a pointer to the buffer, its also a pointer to the first grid
                //in the buffer
                if (GridFunctions.gridCanCollapse(stack.current, direction) == false)
                {
                    continue;
                }

                stack.pushCurrent();
                //GridFunctions.printGrid(stack.current);
                GridFunctions.collapseGridInPlace(stack.current, direction);
                //GridFunctions.printGrid(stack.current);
                if (ttl == 0)
                {
                    scores.Add(direction, AIFunctions.getScoreForGrid3((IntPtr)stack.current));
                    stack.pop();
                    continue;
                }

                byte* currentSquare = null;
                List<int> temp = new List<int>();
                while (GridFunctions.tryAdd2Permutation(stack.current, ref currentSquare))
                {
                    var score = runPermutation(stack, ttl - 1);
                    temp.Add(score.Value);
                }

                stack.pop();

                if (temp.Count == 0)
                {
                    scores.Add(direction, 0);
                }
                else
                {
                    ///we choose the permutation that leads to the worst outcome
                    //scores.Add(direction, temp.Sum() / temp.Count);
                    scores.Add(direction, temp.OrderBy(x => x).First());
                }
            }

            //scores.ToList().ForEach(x => Console.WriteLine(x));

            ///we choose the direction that gives the best square
            if(scores.Count == 0)
            {
                return new KeyValuePair<Direction, int>(Direction.left, -1);
            }
            var s = scores.OrderByDescending(x => x.Value).First();
            return s;
        }
    }
}
