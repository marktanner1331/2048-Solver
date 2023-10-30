using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.Solver
{
    public static class Solver7_Fast
    {
        [DllImport("GridFunctions.dll", EntryPoint = "Solver7_ScoreForGrid", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern uint ScoreForGrid(byte* grid, byte* current, byte* cache, int depth);
    }
}
