using _2048_solver.Solver;
using _2048_Solver.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Runner
{
    internal class Program
    {
        private static ISolver solver = new Solver7();

        static unsafe void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            double average = Enumerable
                .Range(0, 10)
                .Select(x => Run(x))
                .Average();

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");

            Console.WriteLine(average);
            Console.ReadLine();
        }

        private static unsafe int Run(int iteration)
        {
            byte* grid = GridFunctions.CreateEmptyGrid();
            GridFunctions.AddRandomPermutation(grid);

            while (true)
            {
                //in theory if a collapse happened in the previous iteration
                //then we will have at least one empty square
                //commented out for efficiency
                //if(GridFunctions.HasEmptySquares(grid) == false)
                //{
                //    throw new Exception();
                //}

                //Console.WriteLine("new");

                GridFunctions.AddRandomPermutation(grid);

                //GridFunctions.printGrid(grid);
                //Console.ReadLine();

                if (GridFunctions.GridCanCollapse(grid) == false)
                {
                    break;
                }

                Direction direction = solver.Solve(grid);
                //if(GridFunctions.GridCanCollapse(grid, direction) == false)
                //{
                //    throw new Exception();
                //}

                GridFunctions.CollapseGridInPlace(grid, direction);
                //Console.WriteLine(GridFunctions.ConvertGridToIntArray(grid).Sum());

                //GridFunctions.printGrid(grid);
                //Console.ReadLine();
            }

            int finalScore = GridFunctions.ConvertGridToIntArray(grid).Sum();

            //GridFunctions.printGrid(grid);
            //Console.WriteLine(finalScore);
            ////Console.ReadLine();
            Console.WriteLine(iteration + ": " + finalScore);

            return finalScore;
        }
    }
}
