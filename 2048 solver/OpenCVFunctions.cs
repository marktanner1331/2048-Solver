using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_solver
{
    class OpenCVFunctions
    {
        public static byte[] captureGridFromImage(Image<Bgr, byte> image)
        {
            Rectangle grid = findGridInImage(image);
            
            Rectangle[] squares = getSquares(grid);
            byte[] gridValues = new byte[16];

            int a = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    uint mostCommonPixel = findMostCommonValue(image, squares[a]);
                    byte value = pixelColorToBoxValue((int)mostCommonPixel);

                    gridValues[a] = value;
                    a++;
                }
            }

            return gridValues;
        }

        private static byte pixelColorToBoxValue(int pixel)
        {
            string hexValue = colorToHex(Color.FromArgb(pixel));
            switch (hexValue)
            {
                case "EDC22E":
                    return 11;
                case "EDC53F":
                    return 10;
                case "EDC850":
                    return 9;
                case "EDCC61":
                    return 8;
                case "EDCF72":
                    return 7;
                case "F65E3B":
                    return 6;
                case "F67C5F":
                    return 5;
                case "F59563":
                    return 4;
                case "F2B179":
                    return 3;
                case "EDE0C8":
                    return 2;
                case "EEE4DA":
                    return 1;
                case "CDC1B4":
                    return 0;
                default:
                    throw new Exception("unknown tile");
            }
        }

        private static uint findMostCommonValue(Image<Bgr, byte> image, Rectangle roi)
        {
            Dictionary<uint, uint> counters = new Dictionary<uint, uint>();

            for (int i = roi.Left; i < roi.Right; i++)
            {
                for (int j = roi.Top; j < roi.Bottom; j++)
                {
                    uint blue = image.Data[j, i, 0];
                    uint green = image.Data[j, i, 1];
                    uint red = image.Data[j, i, 2];
                    uint pixel = (((red << 8) | green) << 8) | blue;

                    if (counters.ContainsKey(pixel) == false)
                    {
                        counters.Add(pixel, 0);
                    }

                    counters[pixel]++;
                }
            }

            uint largestCount = 0;
            uint value = 0;

            foreach (KeyValuePair<uint, uint> pair in counters)
            {
                if (pair.Value > largestCount)
                {
                    largestCount = pair.Value;
                    value = pair.Key;
                }
            }

            Color c = Color.FromArgb((int)value);

           /* try { pixelColorToBoxValue((int)value); }
            catch
            {
                Debug.WriteLine("");
            }*/

            return value;
        }

        private static string colorToHex(Color c)
        {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private static Rectangle[] getSquares(Rectangle grid)
        {
            Rectangle[] squares = new Rectangle[16];

            int width = grid.Width / 4;

            int a = 0;
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    squares[a] = new Rectangle(grid.X + i * width, grid.Y + j * width, width, width);
                    a++;
                }
            }

            return squares;
        }

        private static Rectangle findGridInImage(Image<Bgr, byte> image)
        {
            Image<Gray, byte> dest = new Image<Gray, byte>(image.Width, image.Height);
            dest = image.InRange(new Bgr(Color.FromArgb(250, 247, 238)), new Bgr(Color.FromArgb(251, 248, 239)));

            Image<Gray, byte> dest2 = new Image<Gray, byte>(image.Width, image.Height);
            CvInvoke.BitwiseNot(dest, dest2, null);

            VectorOfVectorOfPoint result = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(dest2, result, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            List<Tuple<Rectangle, int>> possibleGrids = new List<Tuple<Rectangle, int>>();

            for (int i = 0; i < result.Size; i++)
            {
                VectorOfPoint vop = result[i];
                double contourArea = CvInvoke.ContourArea(vop, false);

                Rectangle r = CvInvoke.BoundingRectangle(vop);
                int boundingArea = r.Width * r.Height;

                //the bounding area will always be larger than the contour area
                double areaRatio = boundingArea / contourArea;

                if (areaRatio > 1.1)
                    continue;

                if (r.Width > r.Height)
                {
                    if (r.Width / r.Height > 1.1)
                    {
                        continue;
                    }
                }
                else
                {
                    if (r.Height / r.Width > 1.1)
                    {
                        continue;
                    }
                }

                possibleGrids.Add(new Tuple<Rectangle, int>(r, boundingArea));
            }

            possibleGrids = possibleGrids.OrderBy(x => x.Item2).ToList();

            Rectangle grid = possibleGrids.First().Item1;

            Bgr lower = new Bgr(Color.FromArgb(200, 200, 200));
            Bgr upper = new Bgr(Color.FromArgb(255, 255, 255));

            shrinkBoxAroundObject(image, ref grid, lower, upper);
            return grid;
        }

        static void shrinkBoxAroundObject(Image<Bgr, byte> image, ref Rectangle rect, Bgr lower, Bgr upper)
        {
            int left = rect.Left;
            int right = rect.Right;
            int top = rect.Top;
            int bottom = rect.Bottom;

            while (true)
            {
                int start = top;
                bool canMove = true;

                while (start != bottom)
                {
                    byte b = image.Data[start, left, 0];
                    byte g = image.Data[start, left, 1];
                    byte r = image.Data[start, left, 2];

                    if (b <= lower.Blue || b > upper.Blue)
                    {
                        canMove = false;
                        break;
                    }

                    if (g <= lower.Green || g > upper.Green)
                    {
                        canMove = false;
                        break;
                    }

                    if (r <= lower.Red || r > upper.Red)
                    {
                        canMove = false;
                        break;
                    }

                    start++;
                }

                if (canMove)
                {
                    left++;
                    rect.X++;
                    rect.Width--;
                }
                else
                {
                    break;
                }
            }

            while (true)
            {
                int start = left;
                bool canMove = true;

                while (start != right)
                {
                    byte b = image.Data[top, start, 0];
                    byte g = image.Data[top, start, 1];
                    byte r = image.Data[top, start, 2];

                    if (b <= lower.Blue || b > upper.Blue)
                    {
                        canMove = false;
                        break;
                    }

                    if (g <= lower.Green || g > upper.Green)
                    {
                        canMove = false;
                        break;
                    }

                    if (r <= lower.Red || r > upper.Red)
                    {
                        canMove = false;
                        break;
                    }

                    start++;
                }

                if (canMove)
                {
                    top++;
                    rect.Y++;
                    rect.Height--;
                }
                else
                {
                    break;
                }
            }

            while (true)
            {
                int start = top;
                bool canMove = true;

                while (start != bottom)
                {
                    byte b = image.Data[start, right, 0];
                    byte g = image.Data[start, right, 1];
                    byte r = image.Data[start, right, 2];

                    if (b <= lower.Blue || b > upper.Blue)
                    {
                        canMove = false;
                        break;
                    }

                    if (g <= lower.Green || g > upper.Green)
                    {
                        canMove = false;
                        break;
                    }

                    if (r <= lower.Red || r > upper.Red)
                    {
                        canMove = false;
                        break;
                    }

                    start++;
                }

                if (canMove)
                {
                    right--;
                    rect.Width--;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
