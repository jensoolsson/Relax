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
    public class CoordSystem
    {
        private Vector3d _xAxis;
        private Vector3d _yAxis;
        private Vector3d _zAxis;

        public void Initialise()
        {
            _xAxis = new Vector3d();
            _yAxis = new Vector3d();
            _zAxis = new Vector3d();
        }

        public CoordSystem()
        {
            Initialise();
        }

        public CoordSystem(CoordSystem oldCS)
        {
            _xAxis = new Vector3d(oldCS.X);
            _yAxis = new Vector3d(oldCS.Y);
            _zAxis = new Vector3d(oldCS.Z);
            UnitizeVectors();
        }

        public CoordSystem(Vector3d x, Vector3d y, Vector3d z)
        {
            Initialise();
            _xAxis = x;
            _yAxis = y;
            _zAxis = z;
            UnitizeVectors();
        }

        public Vector3d X
        {
            get { return _xAxis; }
            set { _xAxis = value; }
        }


        public Vector3d Y
        {
            get { return _yAxis; }
            set { _yAxis = value; }
        }


        public Vector3d Z
        {
            get { return _zAxis; }
            set { _zAxis = value; }
        }

        public void Clear()
        {
            _xAxis.X = 0;
            _xAxis.Y = 0;
            _xAxis.Z = 0;
            _yAxis.X = 0;
            _yAxis.Y = 0;
            _yAxis.Z = 0;
            _zAxis.X = 0;
            _zAxis.Y = 0;
            _zAxis.Z = 0;
        }

        public void UnitizeVectors()
        {
            _xAxis.Unitize();
            _yAxis.Unitize();
            _zAxis.Unitize();
        }

        /** Calculate the coordinate transformatino matrix based on a rotation vector.
        * 
        */
        public Matrix CalcTransformationMatrix(Vector3d a)
        {
            //a is the rotation axis. And the rotation angle alpha is calculate from the expression: Magn(a) = tan(alpha/2)
            double alpha = 2 * Math.Atan(a.Length);
            Matrix transMatrix = new Matrix(3, 3);
            double a2 = Math.Pow((a.Length), 2);

            transMatrix[0, 0] = Math.Cos(alpha) * 1 + ((1 - Math.Cos(alpha)) / (a2)) * (a.X * a.X) + (Math.Sin(alpha) / a.Length) * 0;
            transMatrix[0, 1] = Math.Cos(alpha) * 0 + ((1 - Math.Cos(alpha)) / (a2)) * (a.X * a.Y) + (Math.Sin(alpha) / a.Length) * -a.Z;
            transMatrix[0, 2] = Math.Cos(alpha) * 0 + ((1 - Math.Cos(alpha)) / (a2)) * (a.X * a.Z) + (Math.Sin(alpha) / a.Length) * a.Y;

            transMatrix[1, 0] = Math.Cos(alpha) * 0 + ((1 - Math.Cos(alpha)) / (a2)) * (a.Y * a.X) + (Math.Sin(alpha) / a.Length) * a.Z;
            transMatrix[1, 1] = Math.Cos(alpha) * 1 + ((1 - Math.Cos(alpha)) / (a2)) * (a.Y * a.Y) + (Math.Sin(alpha) / a.Length) * 0;
            transMatrix[1, 2] = Math.Cos(alpha) * 0 + ((1 - Math.Cos(alpha)) / (a2)) * (a.Y * a.Z) + (Math.Sin(alpha) / a.Length) * -a.X;

            transMatrix[2, 0] = Math.Cos(alpha) * 0 + ((1 - Math.Cos(alpha)) / (a2)) * (a.Z * a.X) + (Math.Sin(alpha) / a.Length) * -a.Y;
            transMatrix[2, 1] = Math.Cos(alpha) * 0 + ((1 - Math.Cos(alpha)) / (a2)) * (a.Z * a.Y) + (Math.Sin(alpha) / a.Length) * a.X;
            transMatrix[2, 2] = Math.Cos(alpha) * 1 + ((1 - Math.Cos(alpha)) / (a2)) * (a.Z * a.Z) + (Math.Sin(alpha) / a.Length) * 0;

            return transMatrix;
        }

        public void Transform(Matrix tM)
        {
            Vector3d x = new Vector3d();
            Vector3d y = new Vector3d();
            Vector3d z = new Vector3d();

            x.X = tM[0, 0] * _xAxis.X + tM[0, 1] * _xAxis.Y + tM[0, 2] * _xAxis.Z;
            x.Y = tM[1, 0] * _xAxis.X + tM[1, 1] * _xAxis.Y + tM[1, 2] * _xAxis.Z;
            x.Z = tM[2, 0] * _xAxis.X + tM[2, 1] * _xAxis.Y + tM[2, 2] * _xAxis.Z;

            y.X = tM[0, 0] * _yAxis.X + tM[0, 1] * _yAxis.Y + tM[0, 2] * _yAxis.Z;
            y.Y = tM[1, 0] * _yAxis.X + tM[1, 1] * _yAxis.Y + tM[1, 2] * _yAxis.Z;
            y.Z = tM[2, 0] * _yAxis.X + tM[2, 1] * _yAxis.Y + tM[2, 2] * _yAxis.Z;

            z.X = tM[0, 0] * _zAxis.X + tM[0, 1] * _zAxis.Y + tM[0, 2] * _zAxis.Z;
            z.Y = tM[1, 0] * _zAxis.X + tM[1, 1] * _zAxis.Y + tM[1, 2] * _zAxis.Z;
            z.Z = tM[2, 0] * _zAxis.X + tM[2, 1] * _zAxis.Y + tM[2, 2] * _zAxis.Z;

            _xAxis = x;
            _yAxis = y;
            _zAxis = z;

            UnitizeVectors();
        }

    }

}
