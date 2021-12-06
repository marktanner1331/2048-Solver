using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2048_Solver.SolverTests
{
    internal unsafe class RowPermutator
    {
        private int i = 0;
        public byte* data;

        public RowPermutator()
        {
            data = (byte*)Marshal.AllocHGlobal(sizeof(byte) * 4);
            *(uint*)data = 0;
        }

        public void CopyTo(byte* dest)
        {
            *(UInt32*)dest = *(UInt32*)data;
        }

        public unsafe bool Next()
        {
            for (; i < 256;)
            {
                for (int j = 0; j < 4; j++)
                {
                    data[j] = (byte)((i >> (2 * j)) & 3);
                }

                //Debug.WriteLine($"{data[0]} {data[1]} {data[2]} {data[3]}");
                i++;
                return true;
            }

            return false;
        }
    }
}
