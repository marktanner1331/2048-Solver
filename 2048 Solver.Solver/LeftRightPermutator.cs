using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public unsafe class LeftRightPermutator: IDisposable
    {
        public byte* grid;
        byte* collapsedGrid;
        Direction direction;

        int currentRow;
        public int currentColumn;

        int rowDelta;
        int columnDelta;
        int offset;

        bool[] rowNeedsToCollapse = new bool[4];

        public LeftRightPermutator(byte* grid, Direction direction)
        {
            this.grid = GridFunctions.CreateGrid();
            GridFunctions.CloneGrid(grid, this.grid);
            this.direction = direction;

            collapsedGrid = GridFunctions.CreateGrid();
            GridFunctions.CloneGrid(grid, this.collapsedGrid);
            GridFunctions.CollapseGridInPlace(collapsedGrid, direction);

            currentRow = 0;
            currentColumn = 0;

            GridFunctions.GetDeltas(direction, out offset, out rowDelta, out columnDelta);

            bool[] didCollapse = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                didCollapse[i] = !GridFunctions.RowsAreEqual(grid + (i << 2), collapsedGrid + (i << 2), 1);
            }

            for (int i = 0; i < 4; i++)
            {
                rowNeedsToCollapse[i] = !didCollapse[(i + 1) % 4] && !didCollapse[(i + 2) % 4] && !didCollapse[(i + 3) % 4];
            }
        }

        public void Dispose()
        {
            GridFunctions.FreeGrid(grid);
            GridFunctions.FreeGrid(collapsedGrid);
        }

        public bool TryGetPermutation(byte* output, out uint permutationHash, out bool didCollapse)
        {
            /*
             * Rd = 4 Cd = 1  O = 0  (0, 2) = 0 => i * Rd + O
             * Rd = 4 Cd = -1 O = 3  (0, 2) = 3 => i * Rd + O
             * Rd = 1 Cd = 4  O = 0  (0, 2) = 2 => j * Rd + o
             * Rd = 1 Cd = -4 O = 12 (0, 2) = 14 => j * Rd + o
             */ 

            for(; currentRow < 4;currentRow++)
            {
                for(; currentColumn < 4;currentColumn++)
                {
                    if(grid[currentRow * 4 + currentColumn] == 0)
                    {
                        int mappedRowStart = offset + currentRow * rowDelta;

                        GridFunctions.CloneGrid(collapsedGrid, output);
                        ((uint*)output)[currentRow] = ((uint*)grid)[currentRow];
                        //GridFunctions.CloneRow(grid + mappedRowStart, output + mappedRowStart, columnDelta);

                        output[currentRow * 4 + currentColumn] = 1;
                        didCollapse = GridFunctions.collapseRow3(output + mappedRowStart, columnDelta);

                        uint rowHash = GridFunctions.GetRowHash(output + mappedRowStart, columnDelta);
                        rowHash <<= 2;
                        rowHash |= (uint)currentRow;
                        permutationHash = rowHash;

                        if(!didCollapse)
                        {
                            if(rowNeedsToCollapse[currentRow] == false)
                            {
                                didCollapse = true;
                            }
                        }

                        currentColumn++;
                        return true;
                    }
                }

                currentColumn = 0;
            }

            permutationHash = 0;
            didCollapse = false;
            return false;
        }
    }
}
