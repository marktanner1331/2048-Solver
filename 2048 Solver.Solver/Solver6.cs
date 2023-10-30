using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public unsafe class Solver6 : ISolver
    {
        private byte* data;
        private byte* current;

        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int UP = 2;
        public const int DOWN = 3;

        public Solver6()
        {
            this.data = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * 8);
        }

        public unsafe Direction Solve(byte* grid)
        {
            this.current = data;

            uint[] scores = new uint[4];
            byte* tempGrid = GridFunctions.CreateGrid();

            for (int direction = 0; direction < 4; direction++)
            {
                //Console.WriteLine(direction);
                if (GridFunctions.GridCanCollapse(grid, (Direction)direction) == false)
                {
                    //Console.WriteLine("\tCant Collapse");
                    //GridFunctions.printGrid(grid);
                    scores[(int)direction] = 0;
                    continue;
                }

                GridFunctions.CloneGrid(grid, tempGrid);
                GridFunctions.CollapseGridInPlace(tempGrid, (Direction)direction);

                uint permutations = GridFunctions.CountEmptySquares(grid);
                const int depth = 2;

                if (permutations > 5)
                {
                    scores[direction] = ScoreForGrid(tempGrid, depth);
                }
                else if (permutations < 3)
                {
                    scores[direction] = ScoreForGrid(tempGrid, depth + 2);
                }
                else
                {
                    scores[direction] = ScoreForGrid(tempGrid, depth + 1);
                }

                //Console.WriteLine("\tScore: " + scores[direction]);
            }

            GridFunctions.FreeGrid(tempGrid);

            uint max = 0;
            Direction maxDir = Direction.left;

            for (int i = 0; i < 4; i++)
            {
                if (scores[i] > max)
                {
                    max = scores[i];
                    maxDir = (Direction)i;
                }
            }

            //if(max == 0)
            //{
            //    GridFunctions.printGrid(grid);
            //    throw new Exception();
            //}

            return maxDir;
            //return scores.OrderByDescending(x => x.Value).First().Key;
        }

        private unsafe uint ScoreForGrid(byte* grid, int depth)
        {
            if (GridFunctions.HasEmptySquares(grid) == false)
            {
                return GridFunctions.SumValuesInGrid(grid);
            }

            uint permutations = GridFunctions.CountEmptySquares(grid);
            uint[] scores = new uint[permutations];
            current += 16;

            foreach (Direction subDirection in Enum.GetValues(typeof(Direction)))
            {
                int startIndex = 0;

                for (int i = 0; i < permutations; i++)
                {
                    GridFunctions.CloneGrid(grid, current);
                    GridFunctions.AddPermutation(current, ref startIndex);
                    //if (!success)
                    //{
                    //    GridFunctions.printGrid(grid);
                    //    GridFunctions.CloneGrid(grid, current);
                    //    GridFunctions.printGrid(current);
                    //    GridFunctions.TryAddPermutation(current, ref startIndex);
                    //    throw new Exception();
                    //}


                    if(GridFunctions.GridCanCollapse(current, subDirection) == false)
                    {
                        scores[i] = Math.Max(scores[i], FinalScoreForGrid(current));
                        continue;
                    }

                    GridFunctions.CollapseGridInPlace(current, subDirection);

                    if (depth == 1)
                    {
                        scores[i] = Math.Max(scores[i], FinalScoreForGrid(current));
                    }
                    else
                    {
                        scores[i] = Math.Max(scores[i], ScoreForGrid(current, depth - 1));
                    }

                    startIndex++;
                }
            }

            current -= 16;

            return (uint)scores.Average(x => x);
        }


        private unsafe uint FinalScoreForGrid(byte* grid)
        {
            return GridFunctions.SquareSum(grid);
        }
    }
}