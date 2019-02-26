using System.Diagnostics;
using NUnit.Framework;

namespace Matrices
{
    [TestFixture]
    public class MatrixTests
    {
        [Test]
        public void MatrixUsualMul_ShouldBeCorrect()
        {
            var m1 = new Matrix(new double[,] {{1,2,3}, {4,5,6}, {7,8,9}});
            var m2 = new Matrix(new double[,] {{9,8,7}, {6,5,4}, {3,2,1}});

            var actual = m1 * m2;
            var expected = new Matrix(new double[,] {{30,24,18}, {84,69,54}, {138,114,90}});
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void MatrixParralelMul_ShouldBeCorrect()
        {
            var m1 = new Matrix(new double[,] {{1,2,3}, {4,5,6}, {7,8,9}});
            var m2 = new Matrix(new double[,] {{9,8,7}, {6,5,4}, {3,2,1}});

            var actual = m1.ParallelMul(m2);
            var expected = new Matrix(new double[,] {{30,24,18}, {84,69,54}, {138,114,90}});
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void MatrixSpeedComparison_ParallelMulShouldBeFasterThanUsualMul()
        {
            const int matSize = 300;
            var m1 = new Matrix(matSize, matSize);
            m1.FillRandom();
            var m2 = new Matrix(matSize, matSize);
            m2.FillRandom();

            var usualMulTime = new Stopwatch();
            usualMulTime.Start();
            var r1 = m1 * m2;
            usualMulTime.Stop();

            var parallelMulTime = new Stopwatch();
            parallelMulTime.Start();
            var r2 = m1.ParallelMul(m2);
            parallelMulTime.Stop();
            
            Assert.IsTrue(parallelMulTime.ElapsedMilliseconds < usualMulTime.ElapsedMilliseconds);
        }
    }
}