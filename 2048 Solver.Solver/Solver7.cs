using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public unsafe class Solver7 : ISolver
    {
        private byte* data;
        private byte* current;

        private byte* cache;

        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int UP = 2;
        public const int DOWN = 3;

        public Solver7()
        {
            this.data = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * 8);
            this.cache = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * 128);
        }

        public unsafe Direction Solve(byte* grid)
        {
            this.current = data;

            uint[] scores = new uint[4];
            byte* tempGrid = GridFunctions.CreateGrid();

            for (uint direction = 0; direction < 4; direction++)
            {
                //Console.WriteLine(direction);
                if (GridFunctions.GridCanCollapse(grid, (Direction)direction) == false)
                {
                    //Console.WriteLine("\tCant Collapse");
                    //GridFunctions.printGrid(grid);
                    scores[(int)direction] = 0;
                    continue;
                }

                GridFunctions2.CloneGrid(grid, tempGrid);
                GridFunctions2.CollapseGridInPlace(tempGrid, direction);

                uint permutations = GridFunctions2.CountEmptySquares(grid);
                const int depth = 3;

                //if (permutations > 5)
                //{
                //    scores[direction] = ScoreForGrid(tempGrid, depth);
                //}
                //else if (permutations < 3)
                //{
                //    scores[direction] = ScoreForGrid(tempGrid, depth + 2);
                //}
                //else
                //{
                //    scores[direction] = ScoreForGrid(tempGrid, depth + 1);
                //}

                if (permutations > 5)
                {
                    scores[direction] = Solver7_Fast.ScoreForGrid(tempGrid, current, cache, depth);
                }
                else if (permutations < 5)
                {
                    scores[direction] = Solver7_Fast.ScoreForGrid(tempGrid, current, cache, depth + 2);
                }
                else
                {
                    scores[direction] = Solver7_Fast.ScoreForGrid(tempGrid, current, cache, depth + 1);
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
            if (GridFunctions2.HasEmptySquares(grid) == false)
            {
                return GridFunctions2.SumValuesInGrid(grid);
            }

            uint permutations = GridFunctions2.CountEmptySquares(grid);
            uint[] scores = new uint[permutations];
            current += 16;

            for(uint subDirection  = 0; subDirection < 4;subDirection++)
            {
                int startIndex = 0;

                int cacheCount = 0;
                uint[] cachedScores = new uint[permutations];

                for (int i = 0; i < permutations; i++)
                {
                    GridFunctions2.CloneGrid(grid, current);
                    GridFunctions2.AddPermutation(current, ref startIndex);
                    //if (!success)
                    //{
                    //    GridFunctions.printGrid(grid);
                    //    GridFunctions.CloneGrid(grid, current);
                    //    GridFunctions.printGrid(current);
                    //    GridFunctions.TryAddPermutation(current, ref startIndex);
                    //    throw new Exception();
                    //}

                    if(GridFunctions2.CollapseGridInPlace(current, subDirection) == false)
                    {
                        scores[i] = Math.Max(scores[i], FinalScoreForGrid(current));
                        startIndex++;
                        continue;
                    }

                    uint score;

                    uint index = 0;
                    if (GridFunctions2.IndexOf(cache, cacheCount, current, ref index))
                    {
                        score = cachedScores[index];
                    }
                    else
                    {
                        if (depth == 1)
                        {
                            score = FinalScoreForGrid(current);
                        }
                        else
                        {
                            cache += cacheCount << 4;
                            score = ScoreForGrid(current, depth - 1);
                            cache -= cacheCount << 4;
                        }

                        GridFunctions2.CloneGrid(current, cache + 16 * cacheCount);
                        cachedScores[cacheCount] = score;
                        cacheCount++;
                    }

                    scores[i] = Math.Max(scores[i], score);
                    startIndex++;
                }
            }

            current -= 16;

            return (uint)scores.Average(x => x);
        }

        private unsafe uint FinalScoreForGrid(byte* grid)
        {
            return GridFunctions2.SquareSum(grid);
        }
    }
}
