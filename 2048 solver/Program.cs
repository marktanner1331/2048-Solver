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
            if (debug == false)
            {
                Console.WriteLine("Press Enter to Start");
                Console.ReadLine();

                WindowsFunctions.BringMainWindowToFront("chrome");
                Thread.Sleep(500);
            }

            GridFunctions.initializeCaches();

            int size = 48000000;

            byte* buffer1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * size);
            byte* end1 = buffer1 + 16 * size;

            byte* buffer2= (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * size);
            byte* end2 = buffer1 + 16 * size;

            while (debug || Path.GetFileNameWithoutExtension(WindowsFunctions.GetActiveProcessFileName()).ToLower() == "chrome")
            {
                byte[] managedValues;
                if (debug)
                {
                    managedValues = new byte[]{ 4, 5, 4, 3, 0, 0, 0, 3, 0, 0, 0, 1, 0, 0, 1, 0 };
                }
                else
                {
                    Console.WriteLine("Capturing Screen");
                    Image<Bgr, byte> screenshot = WindowsFunctions.captureScreen();

                    Console.WriteLine("Finding Grid");
                    managedValues = OpenCVFunctions.captureGridFromImage(screenshot);
                }
                
                Stopwatch sw;
                if (debug)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }

                byte* gridValues = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * size);

                //we copy the first grid into the buffer
                fixed (byte* temp = managedValues)
                {
                    GridFunctions.memcpy((IntPtr)gridValues, (IntPtr)temp, new UIntPtr(16 * sizeof(byte)));
                }

                
                List<KeyValuePair<Direction, double>> scores = new List<KeyValuePair<Direction, double>>();

                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    Console.WriteLine("Calculating: " + direction.ToString());

                    //hint: buffer is not just a pointer to the buffer, its also a pointer to the first grid
                    //in the buffer
                    if (GridFunctions.gridCanCollapse(gridValues, direction) == false)
                    {
                        continue;
                    }

                    byte* srcBuffer = buffer1;
                    byte* srcEnd = buffer1;
                    byte* destBuffer = buffer2;
                    byte* destEnd = buffer2;

                    byte* clone = GridFunctions.cloneGrid(gridValues, ref srcEnd);
                    GridFunctions.collapseGridInPlace(clone, direction);

                    for (int i = 0; i < 4; i++)
                    {
                        //we read permutations from the src, and write them to the dest
                        GridFunctions.add2PermutationsAndCollapseInAllDirections(srcBuffer, srcEnd, ref destEnd);

                        //now we swap round teh src and dest for the next iteration
                        byte* temp = srcBuffer;

                        srcBuffer = destBuffer;
                        srcEnd = destEnd;

                        destBuffer = temp;
                        destEnd = temp;
                    }

                    double averageScore = GridFunctions.getAverageScoreForGrids(srcBuffer, srcEnd, AIFunctions.getScoreForGrid1);
                    Console.WriteLine("Score: " + averageScore);

                    scores.Add(new KeyValuePair<Direction, double>(direction, averageScore));
                }

                //we want to choose the direction with the highest score
                scores = scores.OrderBy(x => x.Value).ToList();
                scores = scores.GroupBy(x => x.Value).Last().ToList();

                //if there are multiple with the same score, then we choose one at random
                Direction d = scores[new Random().Next(0, scores.Count)].Key;
                Console.WriteLine("Performing Move: " + d.ToString());

                if (debug)
                {
                    sw.Stop();
                    Debug.WriteLine(sw.ElapsedMilliseconds);
                }
                else
                {
                    WindowsFunctions.performMove(d);
                    Thread.Sleep(500);
                }

                Marshal.FreeHGlobal((IntPtr)gridValues);
            }

            Marshal.FreeHGlobal((IntPtr)buffer1);
            Marshal.FreeHGlobal((IntPtr)buffer2);
        }
    }
}
