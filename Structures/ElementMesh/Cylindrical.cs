using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    class Cylindrical : ElementMesh
    {
        private int _nrRingPts;


        public Cylindrical() : base()
        {
            _nrRingPts = 8;
            _nrSections = 2;
        }

        public int NrRingPoints
        {
            get { return _nrRingPts; }
            set { _nrRingPts = value; }
        }

        public override void BuildMesh(Beam beam)
        {
            BuildMesh((Bar)beam);
        }

        /** Build a cylindrical mesh based on specified nr of sections and ring points. 
        *   TODO: Make dependent on beam coordinate system to capture twist.
        */
        public override void BuildMesh(Bar bar)
        {
            Vector3d vec = bar.GetEnd().Point - bar.GetStart().Point;
            double length = vec.Length;
            Plane secPlane = new Plane(bar.GetStart().Point, vec);
            Circle secCircle = new Circle(secPlane, bar.CrossSection.RadiusOuter);
            Vector3d moveVec = new Vector3d(vec);
            moveVec.Unitize();
            moveVec = moveVec * (length / (_nrSections - 1));    //Scale to be the distance between the sections

            Point3d tempPt;
            NurbsCurve nc = secCircle.ToNurbsCurve();
            
            double t = nc.Domain.Min;
            double dt = (nc.Domain.Max - nc.Domain.Min) / _nrRingPts;
            
            
            int[,] vertIndices = new int[_nrSections, _nrRingPts];
            Point3d[,] points = new Point3d[_nrSections, _nrRingPts];
            int vertCounter = 0;

            _mesh.Faces.Clear();
            _mesh.Vertices.Clear();

            for (int i = 0; i < _nrSections; i++)
            {
                for (int j = 0; j < _nrRingPts; j++)
                {
                    if (i == 0)    //Get the first ring of points from the circle
                    {
                        points[i, j] = nc.PointAt(t);
                        vertIndices[i, j] = vertCounter;
                        t += dt;
                        _mesh.Vertices.Add(points[i, j]);
                    }
                    else        //Then just copy and move those points to create the remaning sections
                    {
                        tempPt = new Point3d(points[i - 1, j]);
                        tempPt += moveVec;
                        points[i, j] = tempPt;
                        vertIndices[i, j] = vertCounter;
                        _mesh.Vertices.Add(points[i, j]);
                    }

                    if (i > 0 && j == _nrRingPts - 1)
                    {
                        _mesh.Faces.AddFace(vertIndices[i, j], vertIndices[i - 1, j], vertIndices[i - 1, j - 1], vertIndices[i, j - 1]);

                        //Close the cylinder by adding the last face starting at j = 0.                        
                        _mesh.Faces.AddFace(vertIndices[i, 0], vertIndices[i - 1, 0], vertIndices[i - 1, j], vertIndices[i, j]);
                    }
                    else if (i > 0 && j > 0)
                    {
                        _mesh.Faces.AddFace(vertIndices[i, j], vertIndices[i - 1, j], vertIndices[i - 1, j - 1], vertIndices[i, j - 1]);   
                    }

                    vertCounter++;
                }
            }
        }

        public override void ColorMesh(Bar bar, double realMin, double realMax, double showMin, double showMax)
        {
            _color1 = Util.ConvertNumberToColor(bar.ColorBy[0], realMin, realMax, showMin, showMax);
            _color2 = Util.ConvertNumberToColor(bar.ColorBy[1], realMin, realMax, showMin, showMax);
            int vertCount = _mesh.Vertices.Count;

            _mesh.VertexColors.Clear();

            for (int i = 0; i < vertCount; i++)
            {
                if (i > _nrRingPts - 1)
                    _mesh.VertexColors.Add(_color2);      //vertex ring 2
                else
                    _mesh.VertexColors.Add(_color1);      //vertex ring 1     
            }
        }


    }
}
