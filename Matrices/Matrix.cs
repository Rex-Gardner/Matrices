using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Matrices
{
    public class Matrix
    {
        private readonly double[,] values;

        public Matrix(int height, int width)
        {
            values = new double[height, width];
        }

        public Matrix(double[,] values)
        {
            this.values = values;
        }

        public int GetHeight()
        {
            return values.GetLength(0);
        }
        
        public int GetWidth()
        {
            return values.GetLength(1);
        }

        public double GetValue(int row, int col)
        {
            if (row >= GetHeight() || row < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (col >= GetWidth() || col < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(col));
            }

            return values[row, col];
        }
        
        public void SetValue(int row, int col, double value)
        {
            if (row >= GetHeight() || row < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (col >= GetWidth() || col < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(col));
            }

            values[row, col] = value;
        }

        public double this[int row, int col]
        {
            get => GetValue(row, col);
            set => SetValue(row, col, value);
        }

        public void FillRandom()
        {
            var rnd = new Random();
            var height = GetHeight();
            var width = GetWidth();

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    values[i, j] = rnd.NextDouble();
                }
            }
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            var m1Width = m1.GetWidth();
            var m2Height = m2.GetHeight();
            
            if (m1Width != m2Height)
            {
                throw new InvalidDataException($"Число столбцов матрицы {nameof(m1)} не равно числу строк матрицы {nameof(m2)}");
            }

            var m1Height = m1.GetHeight();
            var m2Width = m2.GetWidth();
            var result = new Matrix(m1Height, m2Width);

            for (var i = 0; i < m1Height; i++)
            {
                for (var j = 0; j < m2Width; j++)
                {
                    ComputeAndSetCellValue(m1, m2, i, j, result);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Используется как Action для тасков в ParallelMul
        /// </summary>
        private static void ComputeAndSetCellValue(Matrix m1, Matrix m2, int row, int col, Matrix result)
        {
            var cellValue = 0.0;
            var matrixVectorSize = m2.GetHeight();

            for (var k = 0; k < matrixVectorSize; k++)
            {
                cellValue += m1[row, k] * m2[k, col];
            }
                        
            result[row, col] = cellValue;
        }

        public Matrix ParallelMul(Matrix mat)
        {
            var m1Width = this.GetWidth();
            var m2Height = mat.GetHeight();
            
            if (m1Width != m2Height)
            {
                throw new InvalidDataException($"Число столбцов исходной матрицы не равно числу строк матрицы {nameof(mat)}");
            }

            var m1Height = this.GetHeight();
            var m2Width = mat.GetWidth();
            var result = new Matrix(m1Height, m2Width);

            const int tasksCount = 5;
            var tasks = new Task[tasksCount].Select(t => Task.CompletedTask).ToArray();
            
            for (var i = 0; i < m1Height; i++)
            {
                for (var j = 0; j < m2Width; j++)
                {
                    var row = i;
                    var col = j;
                    var isTaskCreated = false;

                    while (!isTaskCreated)
                    {
                        for (var k = 0; k < tasks.Length; k++)
                        {
                            if (!tasks[k].IsCompleted)
                            {
                                continue;
                            }

                            tasks[k] = Task.Run(() => ComputeAndSetCellValue(this, mat, row, col, result));
                            isTaskCreated = true;
                            break;
                        }
                    }
                }
            }

            Task.WaitAll(tasks);
          
            return result;
        }

        private bool Equals(Matrix other)
        {
            if (this.GetHashCode() != other.GetHashCode())
            {
                return false;
            }

            var height = GetHeight();
            var width = GetWidth();

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (Math.Abs(this[i, j] - other[i, j]) > 0.0001)
                    {
                        return false;
                    }   
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Matrix) obj);
        }

        public override int GetHashCode()
        {
            var height = GetHeight();
            var width = GetWidth();
            var hashCode = height * width;
            
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                hashCode = unchecked(hashCode * 314159 + (int)(values[i,j] * 1000000));
            }
            return hashCode;
        }
    }
}