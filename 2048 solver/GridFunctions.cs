using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_solver
{
    static class GridFunctions
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        static UIntPtr gridSize = new UIntPtr(16 * sizeof(byte));
        static UIntPtr doubleGridSize = new UIntPtr(32 * sizeof(byte));

        static Dictionary<int, int> leftCache = new Dictionary<int, int>();
        static Dictionary<int, int> rightCache = new Dictionary<int, int>();

        public static unsafe void initializeCaches()
        {
            leftCache.Add(0, 0);
            rightCache.Add(0, 0);

            byte[] buffer = new byte[4];
            fixed(byte* b = buffer)
            {
                while (buffer[3] != 16)
                {
                    buffer[0]++;
                    for (int i = 0; i < 3; i++)
                    {
                        if (buffer[i] == 16)
                        {
                            buffer[i] = 0;
                            buffer[i + 1]++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int a = *(int*)b;
                    
                    collapseRow2(b, 1);
                    leftCache.Add(a, *(int*)b);
                    int* temp = (int*)b;
                    *temp = a;
                }
            }

            buffer = new byte[4];
            fixed (byte* b = buffer)
            {
                while (buffer[3] != 16)
                {
                    buffer[0]++;
                    for (int i = 0; i < 3; i++)
                    {
                        if (buffer[i] == 16)
                        {
                            buffer[i] = 0;
                            buffer[i + 1]++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int a = *(int*)b;

                    collapseRow2(b, -1);
                    rightCache.Add(a, *(int*)b);
                    int* temp = (int*)b;
                    *temp = a;
                }
            }
        }

        public static unsafe void add2PermutationsAndCollapseInAllDirections(byte* src, byte* srcEnd, ref byte* dest)
        {
            while(src != srcEnd)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (src[i] == 0)
                    {
                        //we create a clone of our grid
                        byte* clone = dest;
                        memcpy((IntPtr)dest, (IntPtr)src, gridSize);
                        dest += 16;

                        //and put the '2' in the right place
                        clone[i] = 1;

                        //then make two copies of it,
                        //the second copy is a double copy
                        //so we end up with 4 grids all the same as the src with the '2' set
                        memcpy((IntPtr)dest, (IntPtr)clone, gridSize);
                        dest += 16;

                        memcpy((IntPtr)dest, (IntPtr)clone, doubleGridSize);
                        dest += 32;

                        //the buffer between 'clone' and 'dest' contains 4 grids with the '2' set in the right place

                        if (gridCanCollapse(src, Direction.left))
                        {
                            collapseGridInPlace(clone, Direction.left);
                            clone += 16;
                        }

                        if (gridCanCollapse(src, Direction.right))
                        {
                            collapseGridInPlace(clone, Direction.right);
                            clone += 16;
                        }

                        if (gridCanCollapse(src, Direction.up))
                        {
                            collapseGridInPlace(clone, Direction.up);
                            clone += 16;
                        }

                        if (gridCanCollapse(src, Direction.down))
                        {
                            collapseGridInPlace(clone, Direction.down);
                            clone += 16;
                        }

                        //just in case we couldnt collapse in all 4 directions, we set dest back to the next clone
                        dest = clone;
                    }
                }

                src += 16;
            }
        }

        public static unsafe double getAverageScoreForGrids(byte* src, byte* srcEnd, Func<IntPtr, int> scorer)
        {
            double score = 0;
            int counter = 0;

            while(src != srcEnd)
            {
                score += scorer((IntPtr)src);
                counter++;
                src += 16;
            }
            
            return score / counter;
        }

        public static string printGridAsArrayLiteral(int[] grid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            for (int i = 0; i < 15; i++)
            {
                sb.Append(grid[i] + ", ");
            }

            sb.Append(grid[15]);
            sb.Append("};");

            return sb.ToString();
        }

        public static unsafe void printGrid(byte* grid)
        {
            int[] values = new int[16];
            for (int i = 0; i < 16; i++)
            {
                if(grid[i] == 0)
                {
                    values[i] = 0;
                }
                else
                {
                    values[i] = 1 << grid[i];
                }
            }

            int maxDigitSize = 0;
            for (int i = 0; i < 16; i++)
            {
                maxDigitSize = Math.Max(values[i].ToString().Length, maxDigitSize);
            }

            string format = "";
            for (int i = 0; i < maxDigitSize; i++)
            {
                format += "0";
            }

            StringBuilder sb = new StringBuilder();

            int a = 0;
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    sb.Append(values[a].ToString(format));
                    sb.Append(" ");
                    a++;
                }

                sb.AppendLine();
            }

            Debug.WriteLine(sb.ToString());
        }

        public static unsafe byte* cloneGrid(byte* src, ref byte* free)
        {
            memcpy((IntPtr)free, (IntPtr)src, gridSize);
            free += 16;

            return free - 16;
        }

        public static unsafe bool gridCanCollapse(byte* grid, Direction direction)
        {
            if(direction == Direction.left)
            {
                int* temp = (int*)grid;
                return leftCache[temp[0]] == temp[0] ||
                       leftCache[temp[1]] == temp[1] ||
                       leftCache[temp[2]] == temp[2] ||
                       leftCache[temp[3]] == temp[3];
            }

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
                /*if(rowCanCollapse(grid, start, delta) != rowCanCollapse2(grid, start, delta))
                {
                    rowCanCollapse2(grid, start, delta);
                    throw new Exception();
                }
                //*/

                if (rowCanCollapse2(grid, start, delta))
                {
                    return true;
                }

                start += delta2;
            }

            return false;
        }

        public static unsafe void collapseGridInPlace(byte* grid, Direction direction)
        {
            if(direction == Direction.left)
            {
                int* row = (int*)grid;
                *row = leftCache[*row];
                row++;
                *row = leftCache[*row];
                row++;
                *row = leftCache[*row];
                row++;
                *row = leftCache[*row];
                row++;

                return;
            }
            else if(direction == Direction.right)
            {
                int* row = (int*)grid;
                *row = rightCache[*row];
                row++;
                *row = rightCache[*row];
                row++;
                *row = rightCache[*row];
                row++;
                *row = rightCache[*row];
                row++;

                return;
            }

            switch (direction)
            {
                case Direction.left:
                    collapseRow2(grid, 1);
                    collapseRow2(grid + 4, 1);
                    collapseRow2(grid + 8, 1);
                    collapseRow2(grid + 12, 1);
                    return;
                case Direction.right:
                    collapseRow2(grid + 3, -1);
                    collapseRow2(grid + 7, -1);
                    collapseRow2(grid + 11, -1);
                    collapseRow2(grid + 15, -1);
                    return;
                case Direction.up:
                    collapseRow2(grid, 4);
                    collapseRow2(grid + 1, 4);
                    collapseRow2(grid + 2, 4);
                    collapseRow2(grid + 3, 4);
                    return;
                case Direction.down:
                    collapseRow2(grid + 12, -4);
                    collapseRow2(grid + 13, -4);
                    collapseRow2(grid + 14, -4);
                    collapseRow2(grid + 15, -4);
                    return;
            }
        }

        private static unsafe void collapseRow3(byte* grid, int delta)
        {
            byte* a = grid;
            byte* b = a + delta;
            byte* c = b + delta;
            byte* d = c + delta;

            if(*a == 0)
            {
                if(*b == 0)
                {
                    if(*c == 0)
                    {
                        if(*d == 0)
                        {
                            return;
                        }
                        else
                        {
                            *a = *d;
                            *d = 0;
                            return;
                        }
                    }
                    else
                    {
                        *a = *c;
                        *c = 0;
                        if(*d == 0)
                        {
                            return;
                        }
                        else if(*a == *d)
                        {
                            (*a)++;
                            *d = 0;
                            return;
                        }
                        else
                        {
                            *b = *d;
                            *d = 0;
                            return;
                        }
                    }
                }
                else
                {
                    *a = *b;
                    if(*c == 0)
                    {
                        if(*d == 0)
                        {
                            *b = 0;
                            return;
                        }
                        else if(*a == *d)
                        {
                            (*a)++;
                            *b = 0;
                            *d = 0;
                        }
                        else
                        {
                            *b = *d;
                            *d = 0;
                        }
                    }
                    else if(*a == *c)
                    {

                    }
                    else
                    {
                        
                        *b = 0;

                    }
                    if(*a == *b)
                    {
                        (*a)++;
                        if(*c == 0)
                        {
                            if(*d == 0)
                            {
                                *b = 0;
                                return;
                            }
                            else
                            {
                                *b = *d;
                                *d = 0;
                                return;
                            }
                        }
                        else
                        {
                            if(*d == 0)
                            {
                                *b = *c;
                                *c = 0;
                            }
                            else if(*b == *d)
                            {
                                *b = (byte)(*c + 1);
                                *c = 0;
                                *d = 0;
                            }
                            else
                            {
                                *b = *c;
                                *c = *d;
                                *d = 0;
                            }
                        }
                    }
                    else
                    {

                    }
                }
            }
            else
            {

            }
        }
        
        private static unsafe void collapseRow2(byte* grid, int delta)
        {
            //we dont ever move the first tile
            grid += delta;

            //loop through each tile in the row
            //'start' is the index of the current tile
            for (int i = 1; i < 4; i++)
            {
                //we cant move '0' tiles
                if(*grid == 0)
                {
                    grid += delta;
                    continue;
                }

                //find the first non zero tile that the start tile will move to
                int a = i - 1;
                byte* aStart = grid - delta;

                while (a > -1)
                {
                    if (*aStart != 0)
                    {
                        break;
                    }

                    aStart -= delta;
                    a--;
                }

                //if we hit here it means that the start tile will move all the way to the edge of the grid
                //or it means we hit a tile (index of that tile is aStart)
                //if the start tile and aStart tile have the same value, they merge
                //otherwise start will sit next to aStart
                if (a == -1 || *aStart != *grid)
                {
                    aStart += delta;
                }
                
                //aStart now contains the offset that we want to move the value to
                //the += covers either doubling if its the same value in both
                //or if the aStart is 0
                if (aStart != grid)
                {
                    if(*aStart == 0)
                    {
                        *aStart = *grid;
                    }
                    else
                    {
                        (*aStart)++;
                    }
                    
                    *grid = 0;
                }

                grid += delta;
            }
        }

        private static unsafe bool rowCanCollapse2(byte* grid, int start, int delta)
        {
            int end  = start + 4 * delta;

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
    }
}
