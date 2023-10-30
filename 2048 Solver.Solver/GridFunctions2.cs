using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public static class GridFunctions2
    {
        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_HasEmptySquares", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern bool HasEmptySquares(byte* grid);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_SquareSum", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern uint SquareSum(byte* grid);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_SumValuesInGrid", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern uint SumValuesInGrid(byte* grid);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_CountEmptySquares", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern uint CountEmptySquares(byte* grid);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_CloneGrid", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern void CloneGrid(byte* source, byte* dest);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_AddPermutation", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern void AddPermutation(byte* grid, ref int startIndex);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_GridsAreEqual", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern bool GridsAreEqual(byte* a, byte* b);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_IndexOf", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern bool IndexOf(byte* cache, int cacheCount, byte* current, ref uint index);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_collapseRow3", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern bool collapseRow3(byte* a, int delta);

        [DllImport("GridFunctions.dll", EntryPoint = "GridFunctions_CollapseGridInPlace", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern bool CollapseGridInPlace(byte* grid, uint direction);
    }
}
