using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public unsafe class Permutator
    {
        public byte* grid;
        byte* collapsedGrid;
        Direction direction;

        int currentRow;
        int currentColumn;

        int rowDelta;
        int columnDelta;
        int offset;

        public Permutator(byte* grid, Direction direction)
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
        }

        public bool TryGetPermutation(byte* output, out uint permutationHash)
        {
            for(; currentRow < 4;currentRow++)
            {
                for(; currentColumn < 4;currentColumn++)
                {
                    if(grid[currentRow * 4 + currentColumn] == 0)
                    {
                        GridFunctions.CloneGrid(collapsedGrid, output);
                        GridFunctions.CloneRow(grid + rowDelta * currentRow + offset, output + rowDelta * currentRow  + offset, columnDelta);

                        output[currentRow * 4 + currentColumn] = 1;
                        GridFunctions.collapseRow(output + rowDelta * currentRow + offset, columnDelta);

                        uint rowHash = GridFunctions.GetRowHash(output + rowDelta * currentRow + offset, columnDelta);
                        rowHash <<= 2;
                        rowHash |= (uint)currentRow;
                        permutationHash = rowHash;

                        currentColumn++;
                        return true;
                    }
                }

                currentColumn = 0;
            }

            permutationHash = 0;
            return false;
        }
    }
}
