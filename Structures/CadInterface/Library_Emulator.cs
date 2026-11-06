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
    public class Library_Emulator
    {
        List<RhinoObject> _lines;
        List<RhinoObject> _pts;
        List<RhinoObject> _pLines;

        Main _m;
        Parameters _p;
        //RhinoInterface _rI;
        Structure _s;

        public void Initialise() 
        {
            _lines = new List<RhinoObject>();
            _pts = new List<RhinoObject>();
            _pLines = new List<RhinoObject>();
            //_rI = new RhinoInterface();
        }

        public Library_Emulator()
        {
            Initialise();
            SetUp();
        }

        public void SetUp() 
        {
            //_rI.SelectObjects();
            //_rI.SortGeometry();
            GetGeometry();
            GH_Example();
        }

        public void GetGeometry() 
        {
            //_lines = _rI.GetLines();
            //_pts = _rI.GetPoints();
        }


        public void GH_Example() 
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();
            List<ConstraintPoint> cpts = new List<ConstraintPoint>();
            List<Spline> splines = new List<Spline>();
            
            int idN = 0;
            int idB = 0;
            int idC = 0;
            int counter = 0;

            Node3Dof n3s, n3e;
            Node6Dof n6s, n6e;

            //Creating nodes and beams from selected lines
            foreach (RhinoObject ro in _lines) 
            {
                Curve c = ro.Geometry as Curve;

                if (c.IsPolyline()) 
                {
                    Polyline pLine = new Polyline();

                    if (c.TryGetPolyline(out pLine))
                    {
                        Spline spline = new Spline(pLine, eSplineType.BendingTorsionSpline);
                        splines.Add(spline);
                    }
                }

                //if (counter < 50)
                //{
                //    n3s = new Node3Dof(c.PointAtStart);
                //    n3e = new Node3Dof(c.PointAtEnd);
                //    vertices.Add((Vertex)n3s);
                //    vertices.Add((Vertex)n3e);

                //    Cable cable = new Cable(n3s, n3e);
                //    edges.Add((Edge)cable);
                //}
                //else 
                //{
                //    n6s = new Node6Dof(c.PointAtStart);
                //    n6e = new Node6Dof(c.PointAtEnd);
                //    vertices.Add((Vertex)n6s);
                //    vertices.Add((Vertex)n6e);

                //    Beam beam = new Beam(n6s, n6e);
                //    edges.Add((Edge)beam);
                //}

                counter++;
            }

            //Creating constraintpoints from selected points
            foreach (RhinoObject ro in _pts) 
            {
                Point pt = ro.Geometry as Point;
                ConstraintPoint cp = new ConstraintPoint(new Point3d(pt.Location.X, pt.Location.Y, pt.Location.Z), eConstraint.Pined);
                cpts.Add(cp);
            }

            _p = new Parameters();
            _p.GravityMagnitude = 0;
            _p.SurfaceLoadMagnitude = 0;
            _p.Draw3dBars = true;

            _s = new Structure(splines, cpts, _p);

            //_s = new Structure(vertices, edges, cpts, _p);
            _m = new Main(_s, _p);

            _m.RunDR();
            //_m.RunSizing();
        }


    }
}
