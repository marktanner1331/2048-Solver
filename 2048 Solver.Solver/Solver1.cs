using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public class Solver1 : ISolver
    {
        public unsafe Direction Solve(byte* grid)
        {
            byte*[] grids = new byte*[4];
            for (int i = 0; i < 4; i++)
            {
                grids[i] = GridFunctions.CloneGrid(grid);
            }

            Dictionary<Direction, uint> scores = new Dictionary<Direction, uint>();

            int j = 0;
            foreach(Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if(GridFunctions.GridCanCollapse(grid, direction) == false)
                {
                    scores[direction] = 0;
                    continue;
                }

                GridFunctions.CollapseGridInPlace(grids[j], direction);
                scores[direction] = GridFunctions.CountEmptySquares(grids[j]);
            }

            return scores.OrderByDescending(x => x.Value).First().Key;
        }
    }
}
