using _2048_solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public unsafe class Solver5 : ISolver
    {
        private byte* data;
        private byte* current;

        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int UP = 2;
        public const int DOWN = 3;

        public Solver5()
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

            foreach (var subDirection in new[] { (Direction.left, Direction.right), (Direction.up, Direction.down) })
            {
                Permutator permutator = new Permutator(grid, subDirection.Item1);
                Permutator permutator2 = new Permutator(grid, subDirection.Item2);
                Dictionary<uint, uint> scoreCache = new Dictionary<uint, uint>();

                int k = 0;
                while (permutator.TryGetPermutation(current, out uint permutationHash, out _))
                {
                    uint score;
                    if (scoreCache.ContainsKey(permutationHash))
                    {
                        score = scoreCache[permutationHash];
                        scores[k] = Math.Max(scores[k], score);
                    }
                    else
                    {
                        if (depth == 1)
                        {
                            score = FinalScoreForGrid(current);
                        }
                        else
                        {
                            score = ScoreForGrid(current, depth - 1);
                        }

                        scores[k] = Math.Max(scores[k], score);

                        throw new Exception("reflections dont work");
                        //GridFunctions.ReflectGrid(current, subDirection.Item2);
                        
                        if (depth == 1)
                        {
                            score = FinalScoreForGrid(current);
                        }
                        else
                        {
                            score = ScoreForGrid(current, depth - 1);
                        }

                        scores[k] = Math.Max(scores[k], score);

                        //scoreCache.Add(permutationHash, scores[k]);
                    }

                    k++;
                }

                //scoreCache.Clear();
                //k = 0;
                //while (permutator2.TryGetPermutation(current, out uint permutationHash2))
                //{
                //    int score;
                //    if (scoreCache.ContainsKey(permutationHash2))
                //    {
                //        score = scoreCache[permutationHash2];
                //    }
                //    else
                //    {
                //        if (depth == 1)
                //        {
                //            score = FinalScoreForGrid(current);
                //        }
                //        else
                //        {
                //            score = ScoreForGrid(current, depth - 1);
                //        }

                //        scoreCache.Add(permutationHash2, score);
                //    }

                //    scores[k] = Math.Max(scores[k], score);

                //    k++;
                //}
            }

            current -= 16;
            //GridFunctions.FreeGrid(tempGrid);

            return (uint)scores.Average(x => x);
        }


        private unsafe uint ScoreForGridOld(byte* grid, int depth)
        {
            if (GridFunctions.HasEmptySquares(grid) == false)
            {
                return GridFunctions.SumValuesInGrid(grid);
            }

            uint permutations = GridFunctions.CountEmptySquares(grid);
            uint[] scores = new uint[permutations];
            current += 16;

            foreach (var subDirection in new[] { (Direction.left, Direction.right), (Direction.up, Direction.down) })
            {
                Permutator permutator = new Permutator(grid, subDirection.Item1);
                Permutator permutator2 = new Permutator(grid, subDirection.Item2);
                Dictionary<uint, uint> scoreCache = new Dictionary<uint, uint>();

                int k = 0;
                while (permutator.TryGetPermutation(current, out uint permutationHash, out _))
                {
                    uint score;
                    if (scoreCache.ContainsKey(permutationHash))
                    {
                        score = scoreCache[permutationHash];
                    }
                    else
                    {
                        uint score1;
                        uint score2;
                        if (depth == 1)
                        {
                            score1 = FinalScoreForGrid(current);
                            permutator2.TryGetPermutation(current, out _, out _);

                            //GridFunctions.ReflectGrid(current, subDirection.Item2);
                            score2 = FinalScoreForGrid(current);
                        }
                        else
                        {
                            score1 = ScoreForGrid(current, depth - 1);
                            permutator2.TryGetPermutation(current, out _, out _);
                            //GridFunctions.ReflectGrid(current, subDirection.Item2);
                            score2 = ScoreForGrid(current, depth - 1);
                        }

                        score = Math.Max(score1, score2);

                        scoreCache.Add(permutationHash, score);
                    }

                    scores[k] = Math.Max(scores[k], score);

                    k++;
                }

                permutator.Dispose();
                permutator2.Dispose();
            }

            current -= 16;
            //GridFunctions.FreeGrid(tempGrid);

            return (uint)scores.Average(x => x);
        }

        private unsafe uint FinalScoreForGrid(byte* grid)
        {
            //Console.WriteLine("\tEmpty: " + GridFunctions.CountEmptySquares(grid));
            //Console.WriteLine("\tsum values: " + GridFunctions.SumValuesInGrid(grid));
            //return (ulong)GridFunctions.CountEmptySquares(grid);
            //return (ulong)(GridFunctions.CountEmptySquares(grid) + GridFunctions.SumValuesInGrid(grid));
            return GridFunctions.SquareSum(grid);

            //int[] snake = new int[]
            //{
            //    0, 1, 2, 3, 7, 6, 5, 4, 8, 9, 10, 11
            //};

            //ulong score = 0;
            //for (int i = 0; i < snake.Length - 1; i++)
            //{
            //    if (grid[snake[i]] < grid[snake[i + 1]])
            //    {
            //        return score;
            //    }

            //    score += (ulong)(1 << grid[snake[i]]) * (ulong)(1 << grid[snake[i]]);
            //}

            //return score;
        }
    }
}
