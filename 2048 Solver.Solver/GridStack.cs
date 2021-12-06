using _2048_Solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_solver.Solver
{
    unsafe class GridStack
    {
        public readonly int maxDepth;
        private byte* data;
        public byte* current { get; private set; }

        public GridStack(int maxDepth)
        {
            this.maxDepth = maxDepth;
            this.data = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 16 * maxDepth);
            current = data;
        }

        /// <summary>
        /// makes a copy of the grid at the top of the stack and pushes it
        /// </summary>
        public void pushCurrent()
        {
            GridFunctions.CloneGrid(current, current + sizeof(byte) * 16);
            current += sizeof(byte) * 16;
        }

        public void pushCurrentWithoutClone()
        {
            current += sizeof(byte) * 16;
        }

        public void pop()
        {
            current -= sizeof(byte) * 16;
        }
    }
}
