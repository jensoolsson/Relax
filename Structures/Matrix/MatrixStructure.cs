using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    class MatrixStructure
    {
        private Matrix _K;
        private Matrix _f;
        private Matrix _a;
        private List<Beam> _beams;
        private List<Node6Dof> _nodes;

        public MatrixStructure(List<Beam> beams, List<Node6Dof> nodes)
        {
            _beams = beams;
            _nodes = nodes;

            //Assuming 6 dofs per node.
            _K = new Matrix(_nodes.Count * 6, _nodes.Count * 6);
            _f = new Matrix(_nodes.Count * 6, 1);

            _K.Zero();
            _f.Zero();
        }

        public void PreProcessMatrix()
        {
            List<int> dofs = new List<int>(12);
            _K.Zero();

            foreach (Beam b in _beams)
            {
                //b.SetUpMatrix();
                dofs.Clear();
                dofs.AddRange(b.GetEnd().DOFS);
                dofs.AddRange(b.GetStart().DOFS);
                //Assemble(b.GetMatrix().GetStiffnessMatrix(), dofs);
            }
        }

        /** Assemble local stiffness matrix k in to the global stiffness matrix K.  
        *   
        */
        public void Assemble(Matrix k, List<int> dofs)
        {
            int m, n;
            double temp;
            for (int i = 0; i < k.RowCount; i++)
            {
                for (int j = 0; j < k.ColumnCount; j++)
                {
                    m = dofs[i];
                    n = dofs[j];
                    temp = _K[m, n];
                    temp += k[i, j];
                    _K[m, n] = temp;
                }
            }
            if (!IsSymetric(_K))
            {

            }
        }

        public void ApplyLoad()
        {
            _f.Zero();

            foreach (Node6Dof n in _nodes)
            {
                double x = n.LoadFromSurface[0];
                double y = n.LoadFromSurface[1];
                double z = n.Mass * -9.81 + n.LoadFromSurface[2];

                _f[n.DOFS[0], 0] = x;
                _f[n.DOFS[1], 0] = y;
                _f[n.DOFS[2], 0] = z;
            }
        }

        public bool Solveq()
        {
            List<int> prescribedIndices = new List<int>();

            //Find all predescribed dofs.
            foreach (Node6Dof n in _nodes)
            {
                if (n.Constraint == eConstraint.Fixed)
                    prescribedIndices.AddRange(n.DOFS);
            }

            //Reduce with respect to prescribed dofs.
            Matrix Kff = ReduceMatrix(_K, prescribedIndices);
            Matrix fff = ReduceVector(_f, prescribedIndices);

            //Solve linear system of equations.
            Kff.Invert(0.00001);

            _a = Util.MatrixMultlipication(Kff, fff);

            return true;
        }

        public void JumpToMatrixSolution()
        {
            int[] dofs;
            CoordSystem beamCS;
            foreach (Node6Dof n in _nodes)
            {
                dofs = n.DOFS;
                n.SetPosition(n.Point.X + _a[dofs[0], 0], n.Point.Y + _a[dofs[1], 0], n.Point.Z + _a[dofs[2], 0]);

                foreach (Beam b in n.GetEdges())
                {
                    beamCS = new CoordSystem(b.BaseCoordSystem);
                    RotateCS(beamCS, _a[dofs[3], 0], _a[dofs[4], 0], _a[dofs[5], 0]);
                    SetBeamEndCS(n, b, beamCS);
                }
            }
        }

        public void RotateCS(CoordSystem cs, double a1, double a2, double a3)
        {
            Vector3d x = new Vector3d(1, 0, 0);
            Vector3d y = new Vector3d(0, 1, 0);
            Vector3d z = new Vector3d(0, 0, 1);
            Vector3d rotVec = new Vector3d();
            Matrix tM = new Matrix(3,3);

            try
            {
                rotVec = CalcRotationVector(x, y, z, a1, a2, a3);
            }
            catch (Exception e) 
            {
                Rhino.RhinoApp.WriteLine(e.Message);
            }

            if (!rotVec.IsTiny())
            {
                try
                {
                    tM = cs.CalcTransformationMatrix(rotVec);
                }
                catch (Exception e)
                {
                    Rhino.RhinoApp.WriteLine(e.Message);
                }

                cs.Transform(tM);
            }
        }


        /** Calculate one rotation vector based on rotation around the three axises.
        * 
        */
        public Vector3d CalcRotationVector(Vector3d v1, Vector3d v2, Vector3d v3, double a1, double a2, double a3)
        {
            v1.Unitize();
            v2.Unitize();
            v3.Unitize();

            v1 = v1 * Math.Tan(a1 / 2);
            v2 = v2 * Math.Tan(a2 / 2);
            v3 = v3 * Math.Tan(a3 / 2);

            Vector3d v4, v5;
            v4 = (v1 + v2 + Util.MyOnCrossProduct(v2, v1)) / (1 - Util.MyDotProd(v1, v2));
            v5 = (v3 + v4 + Util.MyOnCrossProduct(v4, v3)) / (1 - Util.MyDotProd(v3, v4));

            return v5;
        }

        public CoordSystem SetBeamEndCS(Node6Dof n, Beam b, CoordSystem newCS)
        {
            if (b.GetStart().Equals(n))
                return b.CoordSystem1 = newCS;
            else
                return b.CoordSystem2 = newCS;
        }

        public Matrix ReduceMatrix(Matrix matrix, List<int> indices)
        {
            int nRows = matrix.RowCount;
            int nCols = matrix.ColumnCount;
            int counter1 = 0;
            int counter2 = 0;
            Matrix newMat = new Matrix(nRows - indices.Count, nCols - indices.Count);
            newMat.Zero();

            //Reduce the matrix in the rows and columns.
            for (int i = 0; i < nRows; i++)
            {
                if (!Util.ContainsInteger(indices, i))
                {
                    for (int j = 0; j < nCols; j++)
                    {
                        if (!Util.ContainsInteger(indices, j))
                        {
                            newMat[counter1, counter2] = matrix[i, j];
                            counter2++;
                        }
                    }
                    counter2 = 0;
                    counter1++;
                }
            }
            return newMat;
        }

        public Matrix ReduceVector(Matrix matrix, List<int> indices)
        {
            int nRows = matrix.RowCount;
            int nCols = matrix.ColumnCount;
            int counter1 = 0;
            Matrix newMat = new Matrix(nRows - indices.Count, nCols);
            newMat.Zero();

            //Reduce the vector, only for the rows.
            for (int i = 0; i < nRows; i++)
            {
                if (!Util.ContainsInteger(indices, i))
                {
                    newMat[counter1, 0] = matrix[i, 0];
                    counter1++;
                }
            }
            return newMat;
        }


        /** Check if a matrix is symmetric.
        * 
        */
        public bool IsSymetric(Matrix matrix)
        {
            Matrix matrixT = matrix.Duplicate();
            matrixT.Transpose();
            Matrix zero = MatrixSubtraction(matrix, matrixT);

            double sum = 0;
            for (int i = 0; i < zero.RowCount; i++)
            {
                for (int j = 0; j < zero.ColumnCount; j++)
                    sum += zero[i, j];
            }
            if (sum < 0.00001)
                return true;

            return false;
        }


        /** Subract matrix2 from matrix1.
        * 
        */
        public Matrix MatrixSubtraction(Matrix mat1, Matrix mat2)
        {
            Matrix resMat = new Matrix(mat1.RowCount, mat1.ColumnCount);
            resMat.Zero();
            for (int i = 0; i < mat1.RowCount; i++)
            {
                for (int j = 0; j < mat1.ColumnCount; j++)
                {
                    double res = mat1[i, j] - mat2[i, j];
                    resMat[i, j] = res;
                }
            }
            return resMat;
        }


    }
}
