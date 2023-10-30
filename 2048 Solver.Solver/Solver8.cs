using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    
    public unsafe class Solver8 : ISolver
    {
        delegate bool GetPermutation(byte* grid, out uint hash, out bool didCollapse);

        private byte* data;
        private byte* current;

        private uint* cache;

        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int UP = 2;
        public const int DOWN = 3;

        public Solver8()
        {
            this.data = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * 8);
            this.cache = (uint*)Marshal.AllocHGlobal(sizeof(uint) * 128);
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
                const int depth = 1;

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

            //only iterating for LEFT and UP
            for (int subDirection = 0; subDirection < 4; subDirection++)
            {
                int startIndex = 0;

                int cacheCount = 0;
                uint[] cachedScores = new uint[permutations];

                GetPermutation getPermutation;
                if(subDirection < 2)
                {
                    LeftRightPermutator permutator = new LeftRightPermutator(grid, (Direction)subDirection);
                    getPermutation = permutator.TryGetPermutation;
                }
                else
                {
                    Permutator permutator = new Permutator(grid, (Direction)subDirection);
                    getPermutation = permutator.TryGetPermutation;
                }

                for (int i = 0; i < permutations; i++)
                {
                    GridFunctions.CloneGrid(grid, current);

                    //if (!success)
                    //{
                    //    GridFunctions.printGrid(grid);
                    //    GridFunctions.CloneGrid(grid, current);
                    //    GridFunctions.printGrid(current);
                    //    GridFunctions.TryAddPermutation(current, ref startIndex);
                    //    throw new Exception();
                    //}

                    getPermutation(current, out uint hash, out bool couldCollapse);

                    uint score;
                    if(couldCollapse == false)
                    {
                        score = FinalScoreForGrid(current);
                    }
                    else if (IndexOf(cache, cacheCount, hash, out int index))
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
                            cache += cacheCount;
                            score = ScoreForGrid(current, depth - 1);
                            cache -= cacheCount;
                        }

                        cache[cacheCount] = hash;
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

        private unsafe bool IndexOf(uint* array, int count, uint value, out int index)
        {
            for (int i = 0; i < count; i++)
            {
                if(array[i] == value)
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }

        private unsafe uint FinalScoreForGrid(byte* grid)
        {
            return GridFunctions.SquareSum(grid);
        }
    }
}
