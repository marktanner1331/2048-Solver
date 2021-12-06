using Microsoft.VisualStudio.TestTools.UnitTesting;
using _2048_Solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2048_solver.Solver;
using _2048_Solver.SolverTests;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace _2048_Solver.Solver.Tests
{
    [TestClass()]
    public class GridFunctionsTests
    {
        [TestMethod()]
        public unsafe void CreateGridTest()
        {
            byte* grid = GridFunctions.CreateEmptyGrid();

            int sum = GridFunctions.SumValuesInGrid(grid);
            Assert.AreEqual(0, sum);

            GridFunctions.FreeGrid(grid);
        }

        [TestMethod()]
        public unsafe void AddPermutationTest()
        {
            byte* grid = GridFunctions.CreateEmptyGrid();
            int startIndex = 0;
            GridFunctions.TryAddPermutation(grid, ref startIndex);

            int sum = GridFunctions.SumValuesInGrid(grid);
            Assert.AreEqual(2, sum);

            GridFunctions.FreeGrid(grid);
        }

        [TestMethod()]
        public unsafe void AddPermutationTest2()
        {
            byte* grid = GridFunctions.CreateEmptyGrid();
            int startIndex = 0;
            GridFunctions.TryAddPermutation(grid, ref startIndex);
            GridFunctions.TryAddPermutation(grid, ref startIndex);

            int sum = GridFunctions.SumValuesInGrid(grid);
            Assert.AreEqual(4, sum);

            GridFunctions.FreeGrid(grid);
        }

        [TestMethod()]
        public unsafe void collapseGridInPlaceTest()
        {
            byte* grid = GridFunctions.CreateEmptyGrid();
            grid[0] = 1;
            grid[1] = 1;

            GridFunctions.CollapseGridInPlace(grid, Direction.left);
            Assert.AreEqual(2, grid[0]);
        }

        [TestMethod()]
        public unsafe void collapseGridInPlaceTest2()
        {
            byte* grid = GridFunctions.CreateEmptyGrid();
            grid[0] = 1;
            grid[1] = 1;

            GridFunctions.CollapseGridInPlace(grid, Direction.up);
            Assert.AreEqual(1, grid[0]);
        }

        [TestMethod()]
        public unsafe void GridCanCollapseTest1()
        {
            int[] intGrid = new int[]
            {
                64,   4,  2,  2,
                128, 32,  4,  8,
                8,   4,  16,  4,
                4,   2,   4,  2
            };

            byte* grid = GridFunctions.ConvertIntArrayToGrid(intGrid);
            Assert.AreEqual(false, GridFunctions.GridCanCollapse(grid, Direction.up));
        }

        [TestMethod()]
        public unsafe void GridCanCollapseTest2()
        {
            int[] intGrid = new int[]
            {
                2, 4, 2, 0,
                2, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            };

            byte* grid = GridFunctions.ConvertIntArrayToGrid(intGrid);
            Assert.AreEqual(true, GridFunctions.GridCanCollapse(grid, Direction.right));
        }

        [TestMethod()]
        public unsafe void GridIsCompleteTest1()
        {
            int[] intGrid = new int[]
            {
                  8,   2,   4,  16,
                 32,  16, 512,   2,
                  4, 128,  32,   8,
                  0,   8,  64,   2
            };

            byte* grid = GridFunctions.ConvertIntArrayToGrid(intGrid);
            Assert.AreEqual(false, GridFunctions.IsComplete(grid));
        }

        [TestMethod()]
        public void Log2Test()
        {
            Assert.AreEqual(0, GridFunctions.Log2(0));
            Assert.AreEqual(1, GridFunctions.Log2(2));
            Assert.AreEqual(2, GridFunctions.Log2(4));
        }

        [TestMethod()]
        public unsafe void collapseRowTest()
        {
            RowPermutator permutator = new RowPermutator();

            byte* row1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);
            byte* row2 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);

            do
            {
                permutator.CopyTo(row1);
                permutator.CopyTo(row2);

                GridFunctions.collapseRow(row1, 1);
                GridFunctions.collapseRow3(row2, 1);

                try
                {
                    Assert.AreEqual(*(uint*)row1, *(uint*)row2);
                }
                catch
                {
                    Debug.WriteLine($"{permutator.data[0]} {permutator.data[1]} {permutator.data[2]} {permutator.data[3]}");
                    Debug.WriteLine($"{row1[0]} {row1[1]} {row1[2]} {row1[3]}");
                    Debug.WriteLine($"{row2[0]} {row2[1]} {row2[2]} {row2[3]}");
                    throw;
                }
            }
            while (permutator.Next());
        }

        public unsafe void PrintCollapser()
        {
            RowPermutator permutator = new RowPermutator();
            byte* row1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);

            do
            {
                permutator.CopyTo(row1);

                Debug.Write($"{row1[0]} {row1[1]} {row1[2]} {row1[3]}: ");
                GridFunctions.collapseRow(row1, 1);
                Debug.WriteLine($"{row1[0]} {row1[1]} {row1[2]} {row1[3]}");
            }
            while (permutator.Next());
        }


        [TestMethod()]
        public unsafe void collapseRowTest2()
        {
            RowPermutator permutator = new RowPermutator();

            byte* row1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);
            row1[0] = 1;
            row1[1] = 1;
            row1[2] = 2;
            row1[3] = 0;

            GridFunctions.collapseRow(row1, 1);

            Assert.IsTrue(
                row1[0] == 2
             && row1[1] == 2
             && row1[2] == 0
             && row1[3] == 0);
        }

        [TestMethod()]
        public unsafe void collapseRowTest4()
        {
            RowPermutator permutator = new RowPermutator();

            byte* row1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);
            row1[0] = 0;
            row1[1] = 1;
            row1[2] = 1;
            row1[3] = 2;

            GridFunctions.collapseRow3(row1, 1);

            Assert.IsTrue(
                row1[0] == 2
             && row1[1] == 2
             && row1[2] == 0
             && row1[3] == 0);
        }

        [TestMethod()]
        public unsafe void collapseRowTest5()
        {
            RowPermutator permutator = new RowPermutator();

            byte* row1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);
            row1[0] = 0;
            row1[1] = 2;
            row1[2] = 1;
            row1[3] = 1;

            GridFunctions.collapseRow3(row1, 1);

            Assert.IsTrue(
                row1[0] == 2
             && row1[1] == 2
             && row1[2] == 0
             && row1[3] == 0);
        }

        [TestMethod]
        public unsafe void SquareTest()
        {
            for (int i = 1; i < 10; i++)
            {
                int j = 1 << i;
                int sum1 = j * j;

                int sum2 = 1 << (i << 1);
                Assert.AreEqual(sum1, sum2);
            }
        }

        [TestMethod()]
        public unsafe void rowCanCollapseTest()
        {
            RowPermutator permutator = new RowPermutator();

            byte* row1 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);
            byte* row2 = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);

            do
            {
                permutator.CopyTo(row1);
                permutator.CopyTo(row2);

                GridFunctions.collapseRow(row1, 1);
                GridFunctions.collapseRow3(row2, 1);

                try
                {
                    Assert.AreEqual(GridFunctions.rowCanCollapse(row1, 0, 1), GridFunctions.rowCanCollapse(row2, 0, 1));
                }
                catch
                {
                    throw;
                }
            }
            while (permutator.Next());

        }
    }
}