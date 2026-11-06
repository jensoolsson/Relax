using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Rhino.Display;

namespace Structures
{
    public class Conduit : Rhino.Display.DisplayConduit
    {
        //Attributes
        private Structure _structure;
        private Parameters _p;
        private System.Drawing.Color _baseColor;
        public static Mutex drawingMutex = new Mutex();

        //Constructor
        public Conduit(Structure structure, Parameters p) : base() 
        {
            _structure = structure;
            _p = p;
            _baseColor = System.Drawing.Color.Azure;
        }

        public Structure Structure 
        {
            get { return _structure; }
            set { _structure = value; }
        }

        public Parameters Parameters
        {
            get { return _p; }
            set { _p = value; }
        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            if (!_structure.IsDrawable)
                return;

            Main.mutex.WaitOne();
            try
            {
                _structure.CalcBoundingBox();
                e.BoundingBox.Union(_structure.GetBoundingBox());
            }
            finally
            {
                Main.mutex.ReleaseMutex();
            }
        }

        protected override void PreDrawObjects(DrawEventArgs e)
        {
            base.PreDrawObjects(e);

            if (!_structure.IsDrawable)
                return;

            Main.mutex.WaitOne();
            try
            {
                if (_p.DrawVertexNormals)
                    DrawVertexNormals(e);

                if (_p.DrawFaceNormals)
                    DrawFaceNormals(e);

                if (_p.DrawNodes)
                    DrawNodes(e);

                if (_p.DrawLines)
                    DrawLines(e);

                if (_p.DrawMeshWires)
                    DrawMeshWires(e);

                if (_p.DrawMeshShaded)
                    DrawMeshShaded(e);
                
                if (_p.DrawNodeRing)
                    DrawNodeRing(e);

                if (_p.Draw3dBars)
                {
                    Draw3DSupports(e);
                    Draw3DBars(e);
                }

                if (_p.DrawNodeCS)
                    DrawNodeCS(e);

                if (_p.DrawBarCS)
                    DrawBarCS(e);

                if (_p.DrawVertexID)
                    DrawVertexID(e);

                if (_p.DrawEdgeID)
                    DrawEdgeID(e);

                if (_p.DrawFaceID)
                    DrawFaceID(e);

                if (_p.DrawUtilisation)
                    DrawUtilisation(e);
                
                if (_p.DrawNodeMass)
                    DrawNodeMass(e);

                if (_p.DrawNodeLoad)
                    DrawNodeLoad(e);

                if (_p.DrawLoadArrows)
                    DrawLoadArrows(e);

                if (_p.DrawNodeTS)
                    DrawNodeTS(e);

                if (_p.DrawNodeRS)
                    DrawNodeRS(e);

                if (_p.DrawElementTS)
                    DrawElementTS(e);

                if (_p.DrawElementRS)
                    DrawElementRS(e);
                

                //Debug3(e);

                //Debug4(e);

                //Debug6(e);
            }
            finally
            {
                Main.mutex.ReleaseMutex();
            }
        }

        /** Redraw the views. Safe to call from any thread; the redraw itself runs on the UI thread.
        *
        */
        public void Update()
        {
            RhinoApp.InvokeOnUiThread(new Action(() => Rhino.RhinoDoc.ActiveDoc?.Views.Redraw()));
        }


        //------------------------------- Drawing functions --------------------------------//
        #region

        public void DrawFaceVertexOrder(DrawEventArgs e)
        {
            foreach (Face f in _structure.GetFaces())
            {
                foreach (Vertex v in f.GetVertices())
                {
                    int i = 0;
                    Point3d p = (v.Point + f.MidPoint)/2;
                    i++;
                    e.Display.DrawDot(p, i.ToString());
                }
            }
        }

        public void DrawVertexNormals(DrawEventArgs e)
        {
            foreach (Vertex v in _structure.GetVertices())
                e.Display.DrawArrow(new Line(v.Point, v.Normal, _structure.AvrgEdgeLength * 0.5), System.Drawing.Color.Black);
        }

        public void DrawFaceNormals(DrawEventArgs e) 
        {
            foreach (Face f in _structure.GetFaces())
                e.Display.DrawArrow(new Line(f.MidPoint, f.Normal, _structure.AvrgEdgeLength * 0.5), System.Drawing.Color.Black);         
        }

        public void DrawNodes(DrawEventArgs e)
        {
            foreach (Vertex v in _structure.GetVertices())
            {
                e.Display.DrawPoint(v.Point, _baseColor);
            }
        }

        public void DrawLines(DrawEventArgs e)
        {
            foreach (Edge edge in _structure.GetEdges())
                e.Display.DrawLine(new Line(edge.GetStart().Point, edge.GetEnd().Point), _baseColor);
        }

        public void DrawMeshWires(DrawEventArgs e)
        {
            e.Display.DrawMeshWires(_structure.GetMesh(), _baseColor);
        }

        public void DrawMeshShaded(DrawEventArgs e)
        {
            DisplayMaterial dm = new DisplayMaterial(_baseColor);
            e.Display.DrawMeshShaded(_structure.GetMesh(), dm);
        }

        public void Draw3DBars(DrawEventArgs e)
        {
            DisplayMaterial dm = new DisplayMaterial(_baseColor);

            foreach (Bar b in _structure.GetBars())
            {
                if (b.IsMeshBuilt)
                {   
                    e.Display.DrawMeshFalseColors(b.CrossSection.GetMesh());
                    e.Display.DrawMeshWires(b.CrossSection.GetMesh(), System.Drawing.Color.Black);
                }
            }
        }

        public void Draw3DSupports(DrawEventArgs e)
        {
            DisplayMaterial dm = new DisplayMaterial(_baseColor);

            double h = 0.3;
            double r = 0.15;

            foreach (Node3Dof n in _structure.GetNode3dofs())
            {
                if (n.Constraint == eConstraint.Pined || n.Constraint == eConstraint.Fixed)
                {
                    Cone cone = new Cone(new Plane(new Point3d(n.Point.X, n.Point.Y, (n.Point.Z-(h/2))), new Vector3d(0, 0, -1)), h, r);
                    e.Display.DrawCone(cone, _baseColor);
                }
            }
        }



        public void DrawVertexID(DrawEventArgs e) 
        {
            foreach (Vertex v in _structure.GetVertices())
                e.Display.DrawDot(v.Point, v.ID.ToString());
        }

        public void DrawEdgeID(DrawEventArgs e)
        {
            foreach (Edge edge in _structure.GetEdges())
                e.Display.DrawDot(edge.MidPoint, edge.ID.ToString());
        }

        public void DrawFaceID(DrawEventArgs e)
        {
            foreach (Face f in _structure.GetFaces()) 
                e.Display.DrawDot(f.MidPoint, f.ID.ToString());
        }

        public void DrawNodeRing(DrawEventArgs e) 
        {   
            foreach (Vertex v in _structure.GetVertices()) 
            {   
                if (v.GetEdges().Count > 2)
                {
                    double radius = 0;
                    foreach (Edge edge in v.GetEdges())
                        radius += edge.Length / v.GetEdges().Count;

                    Plane p = new Plane(v.Point, v.Normal);
                    List<Point3d> arcPts = new List<Point3d>(3);

                    for (int i = 0; i < 3; i++)
                    {
                        if (i == 2)
                            i = v.GetEdges().Count - 1;

                        Edge edge = v.GetEdges()[i];
                        Vector3d vec = p.ClosestPoint(edge.MidPoint) - v.Point;
                        vec.Unitize();
                        Point3d pt = v.Point + (radius * 0.2) * vec;
                        arcPts.Add(pt);
                    }

                    Arc a = new Arc(arcPts[0], arcPts[1], arcPts[2]);
                    e.Display.DrawArc(a, _baseColor);

                    Vector3d arrowVec = a.TangentAt(a.ClosestParameter(arcPts[2]));
                    e.Display.DrawArrow(new Line(arcPts[2], arcPts[2] + (0.05 * _structure.AvrgEdgeLength) * arrowVec), _baseColor);
                }
            }
        }

        public void DrawNodeCS(DrawEventArgs e)
        {
            foreach (Node6Dof n in _structure.GetNode6dofs())
            {
                CoordSystem cs = n.CoordSystem;

                Point3d np = n.Point;
                Point3d xp = np + cs.X;
                Point3d yp = np + cs.Y;
                Point3d zp = np + cs.Z;

                e.Display.DrawLine(new Line(np, xp), _baseColor);
                e.Display.DrawLine(new Line(np, yp), _baseColor);
                e.Display.DrawLine(new Line(np, zp), _baseColor);

                e.Display.DrawDot(xp, "x");
                e.Display.DrawDot(yp, "y");
                e.Display.DrawDot(zp, "z");
            }
        }

        public void DrawBarCS(DrawEventArgs e)
        {
            foreach (Beam b in _structure.GetBeams())
            {
                CoordSystem cs1 = b.CoordSystem1;
                CoordSystem cs2 = b.CoordSystem2;

                Point3d midPt = (b.GetStart().Point + b.GetEnd().Point) / 2.0;
                Point3d np1 = (b.GetStart().Point + midPt) / 2.0;
                Point3d np2 = (b.GetEnd().Point + midPt) / 2.0;

                Point3d xp1 = np1 + cs1.X;
                Point3d yp1 = np1 + cs1.Y;
                Point3d zp1 = np1 + cs1.Z;

                Point3d xp2 = np2 + cs2.X;
                Point3d yp2 = np2 + cs2.Y;
                Point3d zp2 = np2 + cs2.Z;

                e.Display.DrawLine(new Line(np1, xp1), _baseColor) ;
                e.Display.DrawLine(new Line(np1, yp1), _baseColor);
                e.Display.DrawLine(new Line(np1, zp1), _baseColor);

                e.Display.DrawLine(new Line(np2, xp2), _baseColor);
                e.Display.DrawLine(new Line(np2, yp2), _baseColor);
                e.Display.DrawLine(new Line(np2, zp2), _baseColor);

                e.Display.DrawDot(xp1, "x1");
                e.Display.DrawDot(yp1, "y1");
                e.Display.DrawDot(zp1, "z1");

                e.Display.DrawDot(xp2, "x2");
                e.Display.DrawDot(yp2, "y2");
                e.Display.DrawDot(zp2, "z2");
            }
        }

        public void Debug1(DrawEventArgs e)
        {
            foreach (Vertex v in _structure.GetVertices())
            {
                List<Edge> edges = v.GetEdges();

                if (edges.Count < 2) { return; }

                Vector3d v1 = Util.GetOtherEnd(edges[0], v).Point - v.Point;
                Vector3d v2 = Util.GetOtherEnd(edges[1], v).Point - v.Point;
                Vector3d crossVec = Vector3d.CrossProduct(v1, v2);
                crossVec.Unitize();
                e.Display.DrawArrow(new Line(v.Point, v.Point + crossVec), _baseColor);
            }
        }

        public void DrawUtilisation(DrawEventArgs e)
        {
            foreach (Bar b in _structure.GetBars())
            {
                double u1 = b.ColorBy[0];
                double u2 = b.ColorBy[1];

                u1 = Math.Round(100 * u1, 0);
                u2 = Math.Round(100 * u2, 0);

                Point3d pt1 = (b.GetStart().Point + b.MidPoint) / 2;
                Point3d pt2 = (b.GetEnd().Point + b.MidPoint) / 2;

                Point3d ptMid = b.MidPoint;// + new Point3d(0,-1,0);

                //e.Display.DrawDot(pt1, u1.ToString() + " %");
                //e.Display.DrawDot(pt2, u2.ToString() + " %");

                double average = (u1 + u2) / 2.0;

                e.Display.DrawDot(ptMid, average.ToString() + " %");

            }
        }


        public void DrawNodeMass(DrawEventArgs e)
        {
            foreach (Node3Dof n in _structure.GetNode3dofs())
            {
                double[] load = n.LoadFromMass;
                double loadMag = Math.Sqrt(load[0] * load[0] + load[1] * load[1] + load[2] * load[2]);
                e.Display.DrawDot(n.Point, Math.Round(loadMag).ToString() + " N" );
            }
        }


        public void DrawNodeLoad(DrawEventArgs e)
        {
            foreach (Node3Dof n in _structure.GetNode3dofs())
            {
                double[] load = n.LoadFromSurface;
                double loadMag = Math.Sqrt(load[0] * load[0] + load[1] * load[1] + load[2] * load[2]);
                e.Display.DrawDot(n.Point, Math.Round(loadMag * _p.SurfaceLoadMagnitude).ToString() + " N");
            }
        }

        public void DrawLoadArrows(DrawEventArgs e)
        {
            double maxLoadNode = _structure.MaxNodeLoad;
            double avrgLength = _structure.AverageLength;

            foreach (Node3Dof n in _structure.GetNode3dofs())
            {
                double[] loadM = n.LoadFromMass;
                double[] loadS = n.LoadFromSurface;
                double[] load = new double[3] { 0, 0, 0 };

                load[0] = loadM[0] + loadS[0];
                load[1] = loadM[1] + loadS[1];
                load[2] = loadM[2] + loadS[2];

                double loadMag = Math.Sqrt(load[0] * load[0] + load[1] * load[1] + load[2] * load[2]);

                double fraction = loadMag / maxLoadNode;
                double sf = fraction * avrgLength;

                Vector3d vec = _p.GravityVector + _p.SurfaceLoadVector;
                vec.Unitize();               
                e.Display.DrawArrow(new Line(n.Point, sf * vec), System.Drawing.Color.Azure);
            }
        }


        public void DrawNodeTS(DrawEventArgs e)
        {
            foreach (Node6Dof n in _structure.GetNode6dofs())
            {
                double[] nts = n.TranslationalStiffness;
                double ntsMag = Math.Sqrt(nts[0] * nts[0] + nts[1] * nts[1] + nts[2] * nts[2]);
                e.Display.DrawDot(n.Point, Math.Round(ntsMag,0).ToString());
            }
        }

        public void DrawNodeRS(DrawEventArgs e)
        {
            foreach (Node6Dof n in _structure.GetNode6dofs())
            {
                double[] nrs = n.RotationalStiffness;
                double nrsMag = Math.Sqrt(nrs[0] * nrs[0] + nrs[1] * nrs[1] + nrs[2] * nrs[2]);
                e.Display.DrawDot(n.Point, Math.Round(nrsMag, 0).ToString());
            }
        }

        public void DrawElementTS(DrawEventArgs e)
        {
            foreach (Beam b in _structure.GetBeams())
            {
                double[] bts = b.TransStiffness;
                double btsMag = Math.Sqrt(bts[0] * bts[0] + bts[1] * bts[1] + bts[2] * bts[2]);
                e.Display.DrawDot(b.MidPoint, Math.Round(btsMag, 0).ToString());
            }
        }

        public void DrawElementRS(DrawEventArgs e)
        {
            foreach (Beam b in _structure.GetBeams())
            {
                double[] brs = b.RotationalStiffness;
                double brsMag = Math.Sqrt(brs[0] * brs[0] + brs[1] * brs[1] + brs[2] * brs[2]);
                e.Display.DrawDot(b.MidPoint, Math.Round(brsMag, 0).ToString());
            }
        }


        public void Debug3(DrawEventArgs e) 
        {
            foreach (SplineB b in _structure.GetSplineBars())
            {
                //double sf = 0.000003;
                //double sf2 = 0.000003;

                Point3d pt1 = (b.GetStart().Point + b.MidPoint) / 2;
                Point3d pt2 = (b.GetEnd().Point + b.MidPoint) / 2;

                //e.Display.DrawArrow(new Line(pt1, pt1 + b.ShearBendStart * sf), _baseColor);
                //e.Display.DrawArrow(new Line(pt2, pt2 + b.ShearBendEnd * sf), _baseColor);

                //e.Display.DrawArrow(new Line(pt1, pt1 + b.ShearTwistStart * sf2), _baseColor);
                //e.Display.DrawArrow(new Line(pt2, pt2 + b.ShearTwistEnd * sf2), _baseColor);

                if (b.PreviousSplineBarIsSet && b.NextSplineBarIsSet)
                    e.Display.DrawDot(b.MidPoint, Util.RadToDeg(b.Angle).ToString());

            }
        }

        //public void Debug4(DrawEventArgs e)
        //{
        //    foreach (SplineB b in _structure.GetSplineBars())
        //    {
        //        if (b.PreviousSplineBarIsSet && b.NextSplineBarIsSet)
        //        {

        //            Point3d pt2 = b.MidPoint1 + _structure.AvrgEdgeLength * 0.5 * b.Normal1;
        //            e.Display.DrawArrow(new Line(b.MidPoint1, pt2), _baseColor);
        //            e.Display.DrawPoint(b.MidPoint1, _baseColor);

        //            Point3d pt3 = b.MidPoint2 + _structure.AvrgEdgeLength * 0.5 * b.Normal2;
        //            e.Display.DrawArrow(new Line(b.MidPoint2, pt3), _baseColor);
        //            e.Display.DrawPoint(b.MidPoint2, _baseColor);
        //        }
        //    }
        //}

        public void Debug5(DrawEventArgs e)
        {
            foreach (SplineB b in _structure.GetSplineBars())
            {
                Point3d pt1 = (b.MidPoint + b.GetStart().Point) / 2;
                Point3d pt2 = (b.MidPoint + b.GetEnd().Point) / 2;

                if (b.PreviousSplineBarIsSet)
                    e.Display.DrawDot(pt1, b.PreviousSplineBar.ID.ToString());

                if (b.NextSplineBarIsSet)
                    e.Display.DrawDot(pt2, b.NextSplineBar.ID.ToString());
            }
        }


        public void Debug6(DrawEventArgs e) 
        {
            foreach (Node3Dof n in _structure.GetNode3dofs()) 
            {
                double sf = 0.00001; 
                double[] sbF = n.ShearBendForce;
                //double[] stF = n.ShearTwistForce;
                
                e.Display.DrawArrow(new Line(n.Point, sf * (new Vector3d(sbF[0], sbF[1], sbF[2]))), System.Drawing.Color.Green);
                //e.Display.DrawArrow(new Line(n.Point, sf * (new Vector3d(stF[0], stF[1], stF[2]))), System.Drawing.Color.Blue);
            }
        }


        #endregion
        //----------------------------------------------------------------------------------//
        

    }
}
