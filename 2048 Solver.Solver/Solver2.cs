using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public class Solver2 : ISolver
    {
        public unsafe Direction Solve(byte* grid)
        {
            GridStack stack = new GridStack(128);
            GridFunctions.CloneGrid(grid, stack.current);

            Dictionary<Direction, uint> scores = new Dictionary<Direction, uint>();

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                //Console.WriteLine(direction);
                if (GridFunctions.GridCanCollapse(grid, direction) == false)
                {
                    //Console.WriteLine("\tCant Collapse");
                    continue;
                }

                stack.pushCurrent();
                GridFunctions.CollapseGridInPlace(stack.current, direction);

                scores[direction] = ScoreForGrid(stack, 1);

                stack.pop();
                //Console.WriteLine("\tScore: " + scores[direction]);
            }

            return scores.OrderByDescending(x => x.Value).First().Key;
        }

        private unsafe uint ScoreForGrid(GridStack stack, int depth)
        {
            uint permutations = GridFunctions.CountEmptySquares(stack.current);
            if(permutations == 0)
            {
                return 0;
            }

            int startIndex = 0;
            uint score = int.MaxValue;
            for (int i = 0; i < permutations; i++)
            {
                stack.pushCurrent();
                GridFunctions.AddPermutation(stack.current, ref startIndex);
                //if(!success)
                //{
                //    //GridFunctions.printGrid(grid);
                //    //GridFunctions.printGrid((grids + i * 16));
                //    //GridFunctions.TryAddPermutation((grids + i * 16), startIndex, out startIndex);
                //    throw new Exception();
                //}

                startIndex++;

                uint subScore = 0;
                foreach (Direction subDirection in Enum.GetValues(typeof(Direction)))
                {
                    stack.pushCurrent();
                    GridFunctions.CollapseGridInPlace(stack.current, subDirection);

                    if(depth == 1)
                    {
                        subScore = Math.Max(subScore, FinalScoreForGrid(stack.current));
                    }
                    else
                    {
                        subScore = Math.Max(subScore, ScoreForGrid(stack, depth - 1));
                    }

                    stack.pop();
                }

                stack.pop();
                score = Math.Min(score, subScore);
            }

            return score;
        }

        private unsafe uint FinalScoreForGrid(byte* grid)
        {
            //Console.WriteLine("\tEmpty: " + GridFunctions.CountEmptySquares(grid));
            //Console.WriteLine("\tsum values: " + GridFunctions.SumValuesInGrid(grid));
            //return GridFunctions.CountEmptySquares(grid);
            //return GridFunctions.CountEmptySquares(grid) + GridFunctions.SumValuesInGrid(grid);
            return GridFunctions.SquareSum(grid);
        }
    }
}
