using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MatrixMultiply
{
    public class Matrix
    {
        public int Width { get; }
        public int Height { get; }

        private float[][] m;


        public Matrix(int width, int height)
        {
            Width = width;
            Height = height;
            m = new float[height][];
            for (int row = 0; row < height; row++)
            {
                m[row] = new float[width];
            }
        }

        public Matrix Transpose()
        {
            var result = new Matrix(Height, Width);
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    result.m[col][row] = m[row][col];
                }
            }

            return result;
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            Contract.Requires(a.Width == b.Height && a.Height == b.Width);

            var result = new Matrix(b.Width, a.Height);

            var tempb = b.Transpose();

            var bcolVectors = new Vector<float>[tempb.Height];
            for (int row = 0; row < result.Width; row++)
            {
                bcolVectors[row] = new Vector<float>(tempb.m[row]);
            }

            for (int row = 0; row < result.Height; row++)
            {
                var arow = new Vector<float>(a.m[row]);
                var resultrow = result.m[row];
                for (int col = 0; col < result.Width; col++)
                {
                    var bcol = bcolVectors[col];
                    resultrow[col] = Vector.Dot(arow,bcol);
                }
            }

            return result;
        }

        public static Matrix ParallelMultiply(Matrix a, Matrix b)
        {
            Contract.Requires(a.Width == b.Height && a.Height == b.Width);

            var result = new Matrix(b.Width, a.Height);

            var tempb = b.Transpose();

            var bcolVectors = new Vector<float>[tempb.Height];
            for(int row=0;row<result.Width;row++)
            {
                bcolVectors[row] = new Vector<float>(tempb.m[row]);
            }

            Parallel.For(0, result.Height, (row) =>
            {
                var arow = new Vector<float>(a.m[row]);
                //Console.WriteLine(row);
                var resultrow = result.m[row];
                for (int col = 0; col < result.Width; col++)
                {
                    var bcol = bcolVectors[col];
                    resultrow[col] = Vector.Dot(arow, bcol);
                }
            });

            return result;
        }

        public void SetAt(int row, int col, float value)
        {
            m[row][col] = value;
        }

        public float GetAt(int row, int col)
        {
            return m[row][col];
        }

    }

    class Program
    {
        static void AssignRandom(Matrix matrix)
        {
            Random rnd = new Random();
            for (int row = 0; row < matrix.Height; row++)
            {
                for (int col = 0; col < matrix.Width; col++)
                {
                    matrix.SetAt(row, col, (float)rnd.NextDouble());
                }
            }

        }

        static void Test()
        {
            Matrix a = new Matrix(4096, 4096);
            Matrix b = new Matrix(4096, 4096);

            AssignRandom(a);
            AssignRandom(b);

            Matrix result = Matrix.Multiply(a, b);
        }

        static void ParallelTest()
        {
            Matrix a = new Matrix(4096, 4096);
            Matrix b = new Matrix(4096, 4096);

            AssignRandom(a);
            AssignRandom(b);

            Matrix result = Matrix.ParallelMultiply(a, b);
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Vector.IsHardwareAccelerated);
            Console.WriteLine("Starting Single thread multiply");

            Stopwatch watch = Stopwatch.StartNew();

            Test();

            Console.WriteLine("Single thread multiply elapsed seconds: " + watch.Elapsed.TotalSeconds);

            Console.WriteLine("Starting Parallel multiply");

            watch = Stopwatch.StartNew();

            ParallelTest();

            Console.WriteLine("Parallel multiply elapsed seconds: " + watch.Elapsed.TotalSeconds);

        }
    }
}
