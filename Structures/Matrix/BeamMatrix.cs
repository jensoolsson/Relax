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
    public class BeamMatrix
    {
        private Beam _beam;
        private Matrix _K;      //Element stiffness matrix
        private Matrix _G;      //Element tranformation matrix
        private Matrix _Kp;     //Transformed stiffness matrix
        private Matrix _C;
        //private double[] mN;
        private double[] mVy;
        private double[] mVz;
        private double[] mT;
        private double[] mMy;
        private double[] mMz;
        private double[] mEvalPts;

        public BeamMatrix(Beam beam)
        {
            _beam = beam;
            Initialise();
        }

        public void Initialise()
        {
            _K = new Matrix(12, 12);
            _G = new Matrix(12, 12);
            _Kp = new Matrix(12, 12);
            _C = new Matrix(12, 12);

            _K.Zero();
            _G.Zero();
            _Kp.Zero();
            _C.Zero();
        }

        public void CalcLocalStiffness()
        {
            double E = _beam.Material.YoungsModulus;
            double A = _beam.CrossSection.Area;
            double L = _beam.Length;
            double Iy = _beam.CrossSection.Iy;
            double Iz = _beam.CrossSection.Ix;
            double Kv = _beam.CrossSection.J;
            double G = _beam.Material.ShearModulus;

            double a = E*A/L;
            double b = 12*E*Iz/Math.Pow(L,3);
            double c = 6 * E * Iz / Math.Pow(L, 2);
            double d = 12 * E * Iy / Math.Pow(L, 3);
            double e = 6 * E * Iy / Math.Pow(L, 2);
            double f = G * Kv / L;
            double g = 2 * E * Iy / L;
            double h = 2 * E * Iz / L;

            /*
            Ke= [ a  0  0  0  0  0 -a  0  0  0  0  0 ;
                  0  b  0  0  0  c  0 -b  0  0  0  c ;
                  0  0  d  0 -e  0  0  0 -d  0 -e  0 ;
                  0  0  0  f  0  0  0  0  0 -f  0  0 ;
                  0  0 -e  0 2*g 0  0  0  e  0  g  0 ;
                  0  c  0  0  0 2*h 0 -c  0  0  0  h ;
                 -a  0  0  0  0  0  a  0  0  0  0  0 ;
                  0 -b  0  0  0 -c  0  b  0  0  0 -c ;
                  0  0 -d  0  e  0  0  0  d  0  e  0 ;
                  0  0  0 -f  0  0  0  0  0  f  0  0 ;
                  0  0 -e  0  g  0  0  0  e  0 2*g 0 ;
                  0  c  0  0  0  h  0 -c  0  0  0 2*h ];
            */

            _K[11, 1] = c;
            _K[11, 5] = h;
            _K[11, 7] = -c;
            _K[11, 11] = 2*h;
            _K[10, 2] = -e;
            _K[10, 4] = g;
            _K[10, 8] = e;
            _K[10, 10] = 2*g;
            _K[9, 3] = -f;
            _K[9, 9] = f;
            _K[8, 2] = -d;
            _K[8, 4] = e;
            _K[8, 8] = d;
            _K[8, 10] = e;
            _K[7, 1] = -b;
            _K[7, 5] = -c;
            _K[7, 7] = b;
            _K[7, 11] = -c;
            _K[6, 0] = -a;
            _K[6, 6] = a;
            _K[5, 1] = c;
            _K[5, 5] = 2*h;
            _K[5, 7] = -c;
            _K[5, 11] = h;
            _K[4, 2] = -e;
            _K[4, 4] = 2*g;
            _K[4, 8] = e;
            _K[4, 10] = g;
            _K[3, 3] = f;
            _K[3, 9] = -f;
            _K[2, 2] = d;
            _K[2, 4] = -e;
            _K[2, 8] = -d;
            _K[2, 10] = -e;
            _K[1, 1] = b;
            _K[1, 5] = c;
            _K[1, 7] = -b;
            _K[1, 11] = c;
            _K[0, 0] = a;
            _K[0, 6] = -a;
        }

        public void CalcTransformationMatrix()
        {
            double L = _beam.Length;
            double[] b = new double[3];
            b[0] = _beam.GetEnd().Point.X - _beam.GetStart().Point.X;
            b[1] = _beam.GetEnd().Point.Y - _beam.GetStart().Point.Y;
            b[2] = _beam.GetEnd().Point.Z - _beam.GetStart().Point.Z;

            double[] n1 = new double[3];
            n1[0] = b[0] / L;
            n1[1] = b[1] / L;
            n1[2] = b[2] / L;

            Vector3d eo = _beam.CoordSystem1.Y + _beam.CoordSystem2.Y;
            eo.Unitize();

            double[] n3 = new double[3];
            n3[0] = eo.X;
            n3[1] = eo.Y;
            n3[2] = eo.Z;

            double[] n2 = new double[3];
            n2[0] = n3[1] * n1[2] - n3[2] * n1[1];
            n2[1] = -n1[2] * n3[0] + n1[0] * n3[2];
            n2[2] = n3[0] * n1[1] - n1[0] * n3[1];

            Matrix An = new Matrix(3, 3);
            An.Zero();
            An[0, 0] = n1[0];
            An[0, 1] = n1[1];
            An[0, 2] = n1[2];
            An[1, 0] = n2[0];
            An[1, 1] = n2[1];
            An[1, 2] = n2[2];
            An[2, 0] = n3[0];
            An[2, 1] = n3[1];
            An[2, 2] = n3[2];

            Util.InsertMatrix(_G, An, 0, 2, 0, 2);
            Util.InsertMatrix(_G, An, 3, 5, 3, 5);
            Util.InsertMatrix(_G, An, 6, 8, 6, 8);
            Util.InsertMatrix(_G, An, 9, 11, 9, 11);
        }

        public void CalcForcesMoment()
        {
            ////Element displacementvector from global coord system.
            //Matrix ed = new Matrix(12, 1);     
            //List<CDof> dofs = beam.GetDofs();
            
            //ed[0,0] = ....

            
            //ed[i,0] = dofs[i].Displacement;

            ////Element local displacements
            //Matrix u = Matrix.Multiply(_G, ed);

            //double L = bar.L;

            ///*C=[0 1   0    0  0 0    0     0   0 0 0 0;
            //     0 0   0    0  0 1    0     0   0 0 0 0;
            //     0 0   0    0  0 0    0     0   0 1 0 0;
            //     0 0   0    0  0 0    0     0   0 0 0 1;
            //     0 0   0    0  0 0    0     0  -1 0 0 0;
            //     0 0   0    0  1 0    0     0   0 0 0 0;
            //     L 1   0    0  0 0    0     0   0 0 0 0;
            //     0 0  L^3  L^2 L 1    0     0   0 0 0 0;
            //     0 0   0    0  0 0   L^3   L^2  L 1 0 0;
            //     0 0   0    0  0 0    0     0   0 0 L 1;
            //     0 0   0    0  0 0 -3*L^2 -2*L -1 0 0 0;
            //     0 0 3*L^2 2*L 1 0    0     0   0 0 0 0];
            //*/

            //mC[0, 1] = 1;
            //mC[1, 5] = 1;
            //mC[2, 9] = 1;
            //mC[3, 11] = 1;
            //mC[4, 8] = 1;
            //mC[5, 4] = 1;
            //mC[6, 0] = L;
            //mC[6, 1] = 1;
            //mC[7, 2] = Math.Pow(L, 3);
            //mC[7, 3] = Math.Pow(L, 2);
            //mC[7, 4] = L;
            //mC[7, 5] = 1;
            //mC[8, 6] = Math.Pow(L, 3);
            //mC[8, 7] = Math.Pow(L, 2);
            //mC[8, 8] = L;
            //mC[8, 9] = 1;
            //mC[9, 10] = L;
            //mC[9, 11] = 1;
            //mC[10, 6] = -3 * Math.Pow(L, 2);
            //mC[10, 7] = -2 * L;
            //mC[10, 8] = -1;
            //mC[11, 2] = 3 * Math.Pow(L, 3);
            //mC[11, 3] = 2 * L;
            //mC[11, 4] = 1;

            //Matrix m = Matrix.Multiply(Matrix.Inverse(mC), u);

            //double EIz = bar.E * bar.Iz;
            //double EIy = bar.E * bar.Iy;
            //double EA = bar.E * bar.A;
            //double GKv = bar.G * bar.Kv;

            ///* p = [EA 0    0      0    0 0     0       0   0 0  0  0;
            //        0  0 -6*EIz    0    0 0     0       0   0 0  0  0;
            //        0  0    0      0    0 0  -6*EIy     0   0 0  0  0;
            //        0  0    0      0    0 0     0       0   0 0 GKv 0;
            //        0  0    0      0    0 0 -6*EIy*x -2*EIy 0 0  0  0;
            //        0  0 6*EIz*x 2*EIz  0 0     0       0   0 0  0  0;]
            //*/

            //Matrix p = new Matrix(6, 12);
            //Utilities.ZeroMatrix(p);
            
            //p[0, 0] = EA;
            //p[1, 2] = -6 * EIz;
            //p[2, 6] = -6 * EIy; ;
            //p[3, 10] = GKv;
            //p[4, 7] = -2 * EIy;
            //p[5, 2] = 2 * EIz;

            //double x = 0;
            //int n = 10;       
            //double dl = L/n;

            //Matrix temp;
            //mN = new double[n + 1];
            //mVy = new double[n + 1];
            //mVz = new double[n + 1];
            //mT = new double[n + 1];
            //mMy = new double[n + 1];
            //mMz = new double[n + 1];
            //mEvalPts = new double[n + 1];

            //for (int i = 0; i <= n; i++)
            //{
            //    x = i * dl;
            //    mEvalPts[i] = x;

            //    p[4, 6] = -6 * EIy * x;
            //    p[5, 2] = 6 * EIz * x;

            //    temp = Matrix.Multiply(p, m);

            //    mN[i] = temp[0, 0];
            //    mVy[i] = temp[1, 0];
            //    mVz[i] = temp[2, 0];
            //    mT[i] = temp[3, 0];
            //    mMy[i] = temp[4, 0];
            //    mMz[i] = temp[5, 0];
            //}
        }

        /** Calculate the local stiffness matrix _K, the transformation matrix _G and 
        *   finally the stiffness matrix _Kp for the element stiffness in global coordinates
        *   
        */
        public void SetUpMatrix()
        {
            _K.Zero();
            _G.Zero();
            _Kp.Zero();

            CalcLocalStiffness();
            CalcTransformationMatrix();
            Matrix GT = _G.Duplicate();
            GT.Transpose();

            _Kp = Util.MatrixMultlipication(GT, _K);
            _Kp = Util.MatrixMultlipication(_Kp, _G);
        }

        public Matrix GetStiffnessMatrix()
        {
            return _Kp;
        }

        //public double[] GetN()
        //{
        //    return mN;
        //}

        public double[] GetVy()
        {
            return mVy;
        }

        public double[] GetVz()
        {
            return mVz;
        }

        public double[] GetT()
        {
            return mT;
        }

        public double[] GetMy()
        {
            return mMy;
        }

        public double[] GetMz()
        {
            return mMz;
        }

        public double[] GetEvalPts()
        {
            return mEvalPts;
        }



    }
}
