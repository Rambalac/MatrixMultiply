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
    public class Matrix<T> where T: struct
    {
        public int Width { get; }
        public int Height { get; }

        private T[][] m;


        public Matrix(int width, int height)
        {
            Width = width;
            Height = height;
            m = new T[height][];
            for (int row = 0; row < height; row++)
            {
                m[row] = new T[width];
            }
        }

        public Matrix<T> Transpose()
        {
            var result = new Matrix<T>(Height, Width);
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    result.m[col][row] = m[row][col];
                }
            }

            return result;
        }

        public static Matrix<T> Multiply(Matrix<T> a, Matrix<T> b)
        {
            Contract.Requires(a.Width == b.Height && a.Height == b.Width);

            var result = new Matrix<T>(b.Width, a.Height);

            var tempb = b.Transpose();

            var bcolVectors = new Vector<T>[tempb.Height];
            for (int row = 0; row < result.Width; row++)
            {
                bcolVectors[row] = new Vector<T>(tempb.m[row]);
            }

            for (int row = 0; row < result.Height; row++)
            {
                var arow = new Vector<T>(a.m[row]);
                var resultrow = result.m[row];
                for (int col = 0; col < result.Width; col++)
                {
                    var bcol = bcolVectors[col];
                    resultrow[col] = Vector.Dot(arow,bcol);
                }
            }

            return result;
        }

        public static Matrix<T> ParallelMultiply(Matrix<T> a, Matrix<T> b)
        {
            Contract.Requires(a.Width == b.Height && a.Height == b.Width);

            var result = new Matrix<T>(b.Width, a.Height);

            var tempb = b.Transpose();

            var bcolVectors = new Vector<T>[tempb.Height];
            for(int row=0;row<result.Width;row++)
            {
                bcolVectors[row] = new Vector<T>(tempb.m[row]);
            }

            Parallel.For(0, result.Height, (row) =>
            {
                var arow = new Vector<T>(a.m[row]);
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

        public void SetAt(int row, int col, T value)
        {
            m[row][col] = value;
        }

        public T GetAt(int row, int col)
        {
            return m[row][col];
        }

    }

    class Program
    {
        static void AssignRandom(Matrix<float> matrix)
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

        static void AssignRandom(Matrix<double> matrix)
        {
            Random rnd = new Random();
            for (int row = 0; row < matrix.Height; row++)
            {
                for (int col = 0; col < matrix.Width; col++)
                {
                    matrix.SetAt(row, col, (double)rnd.NextDouble());
                }
            }
        }

        static Matrix<float> TestFloat()
        {
            Matrix<float> a = new Matrix<float>(4096, 4096);
            Matrix<float> b = new Matrix<float>(4096, 4096);

            AssignRandom(a);
            AssignRandom(b);

            return Matrix<float>.Multiply(a, b);
        }

        static Matrix<float> ParallelTestFloat()
        {
            var a = new Matrix<float>(4096, 4096);
            var b = new Matrix<float>(4096, 4096);

            AssignRandom(a);
            AssignRandom(b);

            return Matrix<float>.ParallelMultiply(a, b);
        }

        static Matrix<double> TestDouble()
        {
            var a = new Matrix<double>(4096, 4096);
            var b = new Matrix<double>(4096, 4096);

            AssignRandom(a);
            AssignRandom(b);

            return Matrix<double>.Multiply(a, b);
        }

        static Matrix<double> ParallelTestDouble()
        {
            var a = new Matrix<double>(4096, 4096);
            var b = new Matrix<double>(4096, 4096);

            AssignRandom(a);
            AssignRandom(b);

            return Matrix<double>.ParallelMultiply(a, b);
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Vector.IsHardwareAccelerated);
            Console.WriteLine("Starting Single thread multiply float");

            Stopwatch watch = Stopwatch.StartNew();

            var result1=TestFloat();

            Console.WriteLine("Single thread multiply float elapsed seconds: " + watch.Elapsed.TotalSeconds);

            Console.WriteLine("Starting Parallel multiply float");

            watch = Stopwatch.StartNew();

            var result2 = ParallelTestFloat();

            Console.WriteLine("Parallel multiply float elapsed seconds: " + watch.Elapsed.TotalSeconds);

            /////////////

            Console.WriteLine("Starting Single thread multiply double");

            watch = Stopwatch.StartNew();

            var result3 = TestDouble();

            Console.WriteLine("Single thread multiply double elapsed seconds: " + watch.Elapsed.TotalSeconds);

            Console.WriteLine("Starting Parallel multiply double");

            watch = Stopwatch.StartNew();

            var result4 = ParallelTestDouble();

            Console.WriteLine("Parallel multiply double elapsed seconds: " + watch.Elapsed.TotalSeconds);

            //To be sure results are not removed by optimization

            Console.WriteLine(Sum(result1));
            Console.WriteLine(Sum(result2));
            Console.WriteLine(Sum(result3));
            Console.WriteLine(Sum(result4));
        }

        private static float Sum(Matrix<float> matrix)
        {
            float sum = 0;
            for (int row = 0; row < matrix.Height; row++)
            {
                for (int col = 0; col < matrix.Width; col++)
                {
                    sum += matrix.GetAt(row, col);
                }
            }

            return sum;
        }
        private static double Sum(Matrix<double> matrix)
        {
            double sum = 0;
            for (int row = 0; row < matrix.Height; row++)
            {
                for (int col = 0; col < matrix.Width; col++)
                {
                    sum += matrix.GetAt(row, col);
                }
            }

            return sum;
        }
    }
}
