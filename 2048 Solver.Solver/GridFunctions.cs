using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public class GridFunctions
    {
        private static Random random = new Random(1);

        public static unsafe bool IsComplete(byte* grid)
        {
            return HasEmptySquares(grid) == false && GridCanCollapse(grid, Direction.left) == false && GridCanCollapse(grid, Direction.up) == false;
        }

        public static unsafe bool GridCanCollapse(byte* grid)
        {
            return HasEmptySquares(grid) || GridCanCollapse(grid, Direction.left) || GridCanCollapse(grid, Direction.up);
        }

        public static unsafe byte* CloneGridMulti(byte* grid, int count)
        {
            byte* newGrids = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * count);
            for (int i = 0; i < count; i++)
            {
                CloneGrid(grid, newGrids + i * 16);
            }

            return newGrids;
        }

        /// <summary>
        /// Adds a '2' in the first free square
        /// </summary>
        /// <param name="grid">The grid to change</param>
        /// <param name="startIndex">The square to start looking from</param>
        /// <param name="setIndex">The index of the square that was changed</param>
        /// <returns>true if there was a free square to update, false otherwise</returns>
        public static unsafe bool TryAddPermutation(byte* grid, ref int startIndex)
        {
            grid += startIndex;
            while (startIndex != 16)
            {
                if (*grid == 0)
                {
                    *grid = 1;
                    return true;
                }

                grid++;
                startIndex++;
            }

            return false;
        }

        public static unsafe void printGrid(byte* grid)
        {
            int[] values = ConvertGridToIntArray(grid);

            int maxDigitSize = 0;
            for (int i = 0; i < 16; i++)
            {
                maxDigitSize = Math.Max(values[i].ToString().Length, maxDigitSize);
            }

            StringBuilder sb = new StringBuilder();

            int a = 0;
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    sb.Append(values[a].ToString().PadLeft(maxDigitSize));
                    sb.Append(" ");
                    a++;
                }

                sb.AppendLine();
            }

            Console.WriteLine(sb.ToString());
        }

        internal static unsafe void FreeGrids(byte* subGrids)
        {
            Marshal.FreeHGlobal((IntPtr)subGrids);
        }

        public static unsafe byte* ConvertIntArrayToGrid(int[] intGrid)
        {
            byte* grid = CreateGrid();
            for (int i = 0; i < 16; i++)
            {
                grid[i] = (byte)Log2(intGrid[i]);
            }

            return grid;
        }

        public static int Log2(int value)
        {
            int i;
            for (i = -1; value != 0; i++)
                value >>= 1;

            return (i == -1) ? 0 : i;
        }

        public static unsafe int[] ConvertGridToIntArray(byte* grid)
        {
            int[] values = new int[16];
            for (int i = 0; i < 16; i++)
            {
                if (grid[i] == 0)
                {
                    values[i] = 0;
                }
                else
                {
                    values[i] = 1 << grid[i];
                }
            }

            return values;
        }

        public static unsafe void AddRandomPermutation(byte* grid)
        {
            int numEmpty = CountEmptySquares(grid);

            int randomIndex = random.Next(0, numEmpty);

            int index = 0;
            for (int i = 0; i < 16; i++)
            {
                if (grid[i] == 0)
                {
                    if (index == randomIndex)
                    {
                        grid[i] = 1;
                        return;
                    }
                    index++;
                }
            }
        }

        public static unsafe bool HasEmptySquares(byte* grid)
        {
            byte* end = grid + 16;
            while (grid != end)
            {
                if (*grid == 0)
                {
                    return true;
                }

                grid++;
            }

            return false;
        }

        public static unsafe int SquareSum(byte* grid)
        {
            int sum = 0;
            byte* end = grid + 16;

            while (grid != end)
            {
                if (*grid != 0)
                {
                    sum += 1 << (*grid << 1);
                }

                grid++;
            }


            return sum;
        }

        public static unsafe int CountEmptySquares(byte* grid)
        {
            int numEmpty = 0;
            byte* end = grid + 16;

            while (grid != end)
            {
                if (*grid == 0)
                {
                    numEmpty++;
                }

                grid++;
            }

            return numEmpty;
        }

        public static unsafe bool GridCanCollapse(byte* grid, Direction direction)
        {
            int delta;
            int delta2;
            int start;

            switch (direction)
            {
                case Direction.left:
                    start = 0;
                    delta = 1;
                    delta2 = 4;
                    break;
                case Direction.right:
                    start = 3;
                    delta = -1;
                    delta2 = 4;
                    break;
                case Direction.up:
                    start = 0;
                    delta = 4;
                    delta2 = 1;
                    break;
                case Direction.down:
                    start = 12;
                    delta = -4;
                    delta2 = 1;
                    break;
                default:
                    throw new Exception();
            }

            for (int i = 0; i < 4; i++)
            {
                if (rowCanCollapse(grid, start, delta))
                {
                    return true;
                }

                start += delta2;
            }

            return false;
        }

        public static unsafe bool rowCanCollapse(byte* grid, int start, int delta)
        {
            int end = start + 4 * delta;

            bool zeroMode = grid[start] == 0;
            start += delta;

            while (start != end)
            {
                if (zeroMode)
                {
                    if (grid[start] != 0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (grid[start] == 0)
                    {
                        zeroMode = true;
                    }
                    else if (grid[start] == grid[start - delta])
                    {
                        return true;
                    }
                }

                start += delta;
            }

            return false;
        }

        public static unsafe bool rowCanCollapse2(byte* grid, int start, int delta)
        {
            for (int i = 0; i < 3; i++)
            {
                if(grid[start] == 0)
                {
                    if(grid[start + i * delta] != 0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (grid[start] == grid[start + i * delta])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static unsafe byte* CreateGrid()
        {
            return (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16);
        }

        public static unsafe byte* CreateEmptyGrid()
        {
            byte* grid = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16);
            ((UInt64*)grid)[0] = 0;
            ((UInt64*)grid)[1] = 0;

            return grid;
        }

        public static unsafe void FreeGrid(byte* grid)
        {
            Marshal.FreeHGlobal((IntPtr)grid);
        }

        public static unsafe int SumValuesInGrid(byte* grid)
        {
            int sum = 0;
            byte* end = grid + 16;

            while (grid != end)
            {
                if (*grid != 0)
                {
                    sum += 1 << *grid;
                }

                grid++;
            }

            return sum;
        }

        public static unsafe int HighestValueInGrid(byte* grid)
        {
            int highest = 0;

            byte* end = grid + 16;

            while (grid != end)
            {
                highest = Math.Max(highest, *grid);
                grid++;
            }

            return highest;
        }

        public static unsafe byte* CloneGrid(byte* grid)
        {
            byte* grid2 = CreateGrid();
            CloneGrid(grid, grid2);
            return grid2;
        }

        public static unsafe void CloneGrid(byte* source, byte* dest)
        {
            *(UInt64*)dest = *(UInt64*)source;
            ((UInt64*)dest)[1] = ((UInt64*)source)[1];

            //memcpy((IntPtr)dest, (IntPtr)source, 16);
        }

        public static void GetDeltas(Direction direction, out int offset, out int rowDelta, out int columnDelta)
        {
            switch (direction)
            {
                case Direction.left:
                    offset = 0;
                    rowDelta = 4;
                    columnDelta = 1;
                    return;
                case Direction.right:
                    offset = 3;
                    rowDelta = 4;
                    columnDelta = -1;
                    return;
                case Direction.up:
                    offset = 0;
                    rowDelta = 1;
                    columnDelta = 4;
                    return;
                case Direction.down:
                    offset = 12;
                    rowDelta = 1;
                    columnDelta = -4;
                    return;
                default:
                    throw new Exception();
            }
        }

        public static unsafe uint GetRowHash(byte* rowStart, int columnDelta)
        {
            uint k = 0;

            k |= *rowStart;

            k <<= 5;
            rowStart += columnDelta;

            k |= *rowStart;

            k <<= 5;
            rowStart += columnDelta;

            k |= *rowStart;

            k <<= 5;
            rowStart += columnDelta;

            k |= *rowStart;

            return k;
        }

        public static unsafe void CloneRow(byte* source, byte* dest, int columnDelta)
        {
            *dest = *source;

            source += columnDelta;
            dest += columnDelta;

            *dest = *source;

            source += columnDelta;
            dest += columnDelta;

            *dest = *source;

            source += columnDelta;
            dest += columnDelta;

            *dest = *source;
        }

        public static unsafe void CollapseGridInPlace(byte* grid, Direction direction)
        {
            switch (direction)
            {
                case Direction.left:
                    collapseRow3(grid, 1);
                    collapseRow3(grid + 4, 1);
                    collapseRow3(grid + 8, 1);
                    collapseRow3(grid + 12, 1);
                    return;
                case Direction.right:
                    collapseRow3(grid + 3, -1);
                    collapseRow3(grid + 7, -1);
                    collapseRow3(grid + 11, -1);
                    collapseRow3(grid + 15, -1);
                    return;
                case Direction.up:
                    collapseRow3(grid, 4);
                    collapseRow3(grid + 1, 4);
                    collapseRow3(grid + 2, 4);
                    collapseRow3(grid + 3, 4);
                    return;
                case Direction.down:
                    collapseRow3(grid + 12, -4);
                    collapseRow3(grid + 13, -4);
                    collapseRow3(grid + 14, -4);
                    collapseRow3(grid + 15, -4);
                    return;
            }
        }

        public static unsafe void collapseRow(byte* grid, int delta)
        {
            byte* start = grid;

            for (int i = 1; i < 4; i++)
            {
                grid += delta;

                //we have found a number we can move
                if (*grid != 0)
                {
                    if (*start == 0)
                    {
                        *start = *grid;
                        *grid = 0;
                    }
                    else if (*start == *grid)
                    {
                        *start += 1;
                        *grid = 0;
                        start += delta;
                    }
                    else
                    {
                        if (start + delta == grid)
                        {
                            start = grid;
                        }
                        else
                        {
                            start += delta;

                            *start = *grid;
                            *grid = 0;
                        }
                    }
                }
            }
            return;
        }

        public static unsafe void collapseRow3(byte* a, int delta)
        {
            byte* b = a + delta;
            byte* c = b + delta;
            byte* d = c + delta;

            if (*a == 0)
            {
                if (*b == 0)
                {
                    if (*c == 0)
                    {
                        if (*d != 0)
                        {
                            *a = *d;
                            *d = 0;
                        }
                    }
                    else
                    {
                        *a = *c;
                        *c = 0;

                        if (*a == *d)
                        {
                            (*a)++;
                            *d = 0;
                        }
                        else if (*d != 0)
                        {
                            *b = *d;
                            *d = 0;
                        }
                    }
                }
                else
                {
                    *a = *b;
                    *b = 0;

                    if (*c == 0)
                    {
                        if (*a == *d)
                        {
                            (*a)++;
                            *d = 0;
                        }
                        else if (*d != 0)
                        {
                            *b = *d;
                            *d = 0;
                        }
                    }
                    else
                    {
                        if (*a == *c)
                        {
                            (*a)++;
                            *c = 0;
                            if (*d != 0)
                            {
                                *b = *d;
                                *d = 0;
                            }
                        }
                        else
                        {
                            *b = *c;
                            *c = 0;

                            if (*b == *d)
                            {
                                (*b)++;
                                *d = 0;
                            }
                            else if (*d != 0)
                            {
                                *c = *d;
                                *d = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                if (*b == 0)
                {
                    if (*c == 0)
                    {
                        if (*a == *d)
                        {
                            (*a)++;
                            *d = 0;
                        }
                        else if (*d != 0)
                        {
                            *b = *d;
                            *d = 0;
                        }
                    }
                    else
                    {
                        if (*d == 0)
                        {
                            if (*a == *c)
                            {
                                (*a)++;
                            }
                            else
                            {
                                *b = *c;
                            }

                            *c = 0;
                        }
                        else
                        {
                            if (*a == *c)
                            {
                                (*a)++;
                                *c = 0;
                                *b = *d;
                            }
                            else
                            {
                                *b = *c;

                                if (*b == *d)
                                {
                                    (*b)++;
                                    *c = 0;
                                }
                                else
                                {
                                    *c = *d;
                                }
                            }

                            *d = 0;
                        }
                    }
                }
                else
                {
                    if (*c == 0)
                    {
                        if (*d == 0)
                        {
                            if (*a == *b)
                            {
                                (*a)++;
                                *b = 0;
                            }
                        }
                        else
                        {
                            if (*a == *b)
                            {
                                (*a)++;
                                *b = *d;
                            }
                            else if (*b == *d)
                            {
                                (*b)++;
                            }
                            else
                            {
                                *c = *d;
                            }

                            *d = 0;
                        }
                    }
                    else
                    {
                        if (*d == 0)
                        {
                            if (*a == *b)
                            {
                                (*a)++;
                                *b = *c;
                                *c = 0;
                            }
                            else if (*b == *c)
                            {
                                (*b)++;
                                *c = 0;
                            }
                        }
                        else
                        {
                            if (*a == *b)
                            {
                                (*a)++;
                                *b = *c;
                                if (*b == *d)
                                {
                                    (*b)++;
                                    *c = 0;
                                }
                                else
                                {
                                    *c = *d;
                                }

                                *d = 0;
                            }
                            else if (*b == *c)
                            {
                                (*b)++;
                                *c = *d;
                                *d = 0;
                            }
                            else if (*c == *d)
                            {
                                (*c)++;
                                *d = 0;
                            }
                        }
                    }
                }
            }
        }
        
        public static unsafe void collapseRow2(byte* grid, int delta)
        {
            byte* start = grid;

            {
                grid += delta;

                //we have found a number we can move
                if (*grid != 0)
                {
                    if (*start == 0)
                    {
                        *start = *grid;
                        *grid = 0;
                    }
                    else if (*start == *grid)
                    {
                        *start += 1;
                        *grid = 0;
                        start += delta;
                    }
                    else
                    {
                        if (start + delta == grid)
                        {
                            start = grid;
                        }
                        else
                        {
                            start += delta;

                            *start = *grid;
                            *grid = 0;
                        }
                    }
                }
            }
            {
                grid += delta;

                //we have found a number we can move
                if (*grid != 0)
                {
                    if (*start == 0)
                    {
                        *start = *grid;
                        *grid = 0;
                    }
                    else if (*start == *grid)
                    {
                        *start += 1;
                        *grid = 0;
                        start += delta;
                    }
                    else
                    {
                        if (start + delta == grid)
                        {
                            start = grid;
                        }
                        else
                        {
                            start += delta;

                            *start = *grid;
                            *grid = 0;
                        }
                    }
                }
            }
            {
                grid += delta;

                //we have found a number we can move
                if (*grid != 0)
                {
                    if (*start == 0)
                    {
                        *start = *grid;
                        *grid = 0;
                    }
                    else if (*start == *grid)
                    {
                        *start += 1;
                        *grid = 0;
                        start += delta;
                    }
                    else
                    {
                        if (start + delta == grid)
                        {
                            start = grid;
                        }
                        else
                        {
                            start += delta;

                            *start = *grid;
                            *grid = 0;
                        }
                    }
                }
            }
        }
    }
}
