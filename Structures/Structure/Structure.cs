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
    public class Structure : GeomStructure
    {
        protected double _structWeight;           //Total weight of the structure 
        protected List<ConstraintPoint> _consPts;

        protected double _maxNodeLoad;
        protected double _minNodeLoad;

        protected bool _isAnalysable;

        public new void Initialise()
        {
            _consPts = new List<ConstraintPoint>();
            _structWeight = 0;
            _isAnalysable = false;
        }

        //--------------------------------- Constructors -----------------------------------//
        #region
        
        public Structure() : base()
        {
            Initialise();
        }

        //Constructor used in the GH component.
        public Structure(List<Line> lines, List<Point3d> pts) 
        {
            base.Initialise();
            Initialise();

            CreateCablesFromLines(lines);

            CreateConstraintObjects(pts);
        }

        public Structure(List<Node3Dof> nodes, List<Bar> bars, List<ConstraintPoint> cpts, Parameters p)
        {
            base.Initialise();
            Initialise();
            StichExternalObjects(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), Util.CastToSuperclass<Edge, Bar>(bars), cpts, p);
        }

        public Structure(List<Node6Dof> nodes, List<Beam> beams, List<ConstraintPoint> cpts, Parameters p)
        {
            base.Initialise();
            Initialise();
            StichExternalObjects(Util.CastToSuperclass<Vertex, Node6Dof>(nodes), Util.CastToSuperclass<Edge, Beam>(beams), cpts, p);
        }

        public Structure(List<Vertex> vertices, List<Edge> edges, List<ConstraintPoint> cpts, Parameters p)
        {
            base.Initialise();
            Initialise();
            StichExternalObjects(vertices, edges, cpts, p);
        }

        public Structure(List<Vertex> vertices, List<Edge> edges, List<Spline> splines, List<ConstraintPoint> cpts, Parameters p)
        {
            base.Initialise();
            Initialise();

            //Create objects from spline
            foreach (Spline spline in splines)
            {
                spline.CreateObjects();
                vertices.AddRange(spline.GetVertices());
                edges.AddRange(spline.GetEdges());
            }

            StichExternalObjects(vertices, edges, cpts, p);
        }

        public Structure(List<Spline> splines, List<ConstraintPoint> cpts, Parameters p)
        {
            base.Initialise();
            Initialise();

            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();

            //Create objects from spline
            foreach (Spline spline in splines)
            {
                spline.CreateObjects();
                vertices.AddRange(spline.GetVertices());
                edges.AddRange(spline.GetEdges());
            }

            StichExternalObjects(vertices, edges, cpts, p);
        }

        public Structure(List<Vertex> vetrices, List<Edge> edges, List<Face> faces) : base(vetrices, edges, faces)
        {
            Initialise();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------------- Properties -------------------------------------//
        #region

        public List<Element> GetElements()
        {
            return Util.CastToSubclass<Element, Edge>(_edges);
        }

        public List<Bar> GetBars()
        {
            return Util.CastToSubclass<Bar, Edge>(_edges);
        }

        public List<Beam> GetBeams()
        {
            return Util.CastToSubclass<Beam, Edge>(_edges);
        }

        public List<Cable> GetCables()
        {
            return Util.CastToSubclass<Cable, Edge>(_edges);
        }

        public List<SplineB> GetSplineBars()
        {
            return Util.CastToSubclass<SplineB, Edge>(_edges);
        }

        public List<Plate> GetPlates()
        {
            return Util.CastToSubclass<Plate, Face>(_faces);
        }

        public List<Node> GetNodes()
        {
            return Util.CastToSubclass<Node, Vertex>(_vertices);
        }

        public List<Node3Dof> GetNode3dofs()
        {
            return Util.CastToSubclass<Node3Dof, Vertex>(_vertices);
        }

        public List<Node6Dof> GetNode6dofs()
        {
            return Util.CastToSubclass<Node6Dof, Vertex>(_vertices);
        }

        public double StructuralWeight 
        {
            get { return _structWeight; }
            set { _structWeight = value; } 
        }

        public bool IsAnalysable 
        {
            get { return _isAnalysable; }
            set { _isAnalysable = value; }
        }

        public double MaxNodeLoad
        {
            get { return _maxNodeLoad; }
            set { _maxNodeLoad = value; }
        }

        public double MinNodeLoad
        {
            get { return _minNodeLoad; }
            set { _minNodeLoad = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Creating objects ----------------------------------//
        #region

        /* Beams */
        public void CreateBeamsFromLines(List<RhinoObject> selLine)
        {
            List<Node6Dof> nodes = new List<Node6Dof>();
            List<Beam> beams = new List<Beam>();
            int nodeCounter = 0;
            for (int i = 0; i < selLine.Count; i++)
            {
                int index = -1;
                Curve crv = selLine[i].Geometry as Curve;
                Point3d ptS = crv.PointAtStart;
                Point3d ptE = crv.PointAtEnd;
                Node6Dof ns, ne;

                //Check if there is a vertex at the start point
                if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node6Dof>(nodes), ptS, out index))
                {
                    ns = new Node6Dof(ptS);
                    ns.ID = nodeCounter;
                    nodeCounter++;
                    nodes.Add(ns);
                }
                else
                    ns = nodes[index];

                //Check if there is a vertex at the edn point
                if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node6Dof>(nodes), ptE, out index))
                {
                    ne = new Node6Dof(ptE);
                    ne.ID = nodeCounter;
                    nodeCounter++;
                    nodes.Add(ne);
                }
                else
                    ne = nodes[index];

                Beam e = new Beam(ns, ne);
                e.ID = i;
                ns.AddEdge(e);
                ne.AddEdge(e);
                beams.Add(e);
            }
            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node6Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, Beam>(beams));
        }
        
        /* Cables */
        public void CreateCablesFromLines(List<RhinoObject> selLine)
        {
            List<Node3Dof> nodes = new List<Node3Dof>();
            List<Cable> cables = new List<Cable>();
            int nodeCounter = 0;
            for (int i = 0; i < selLine.Count; i++)
            {
                int index = -1;
                Curve crv = selLine[i].Geometry as Curve;
                Point3d ptS = crv.PointAtStart;
                Point3d ptE = crv.PointAtEnd;
                Node3Dof ns, ne;

                //Check if there is a vertex at the start point
                if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptS, out index))
                {
                    ns = new Node3Dof(ptS);
                    ns.ID = nodeCounter;
                    nodeCounter++;
                    nodes.Add(ns);
                }
                else
                    ns = nodes[index];

                //Check if there is a vertex at the edn point
                if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptE, out index))
                {
                    ne = new Node3Dof(ptE);
                    ne.ID = nodeCounter;
                    nodeCounter++;
                    nodes.Add(ne);
                }
                else
                    ne = nodes[index];

                Cable c = new Cable(ns, ne);
                c.ID = i;
                ns.AddEdge(c);
                ne.AddEdge(c);
                cables.Add(c);
            }
            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node3Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, Cable>(cables));
        }

        public void CreateCablesFromLines(List<Line> lines)
        {
            List<Node3Dof> nodes = new List<Node3Dof>();
            List<Cable> cables = new List<Cable>();
            int nodeCounter = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 0.001)
                {
                    int index = -1;
                    Point3d ptS = lines[i].From;
                    Point3d ptE = lines[i].To;
                    Node3Dof ns, ne;

                    //Check if there is a vertex at the start point
                    if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptS, out index))
                    {
                        ns = new Node3Dof(ptS);
                        ns.ID = nodeCounter;
                        nodeCounter++;
                        nodes.Add(ns);
                    }
                    else
                        ns = nodes[index];

                    //Check if there is a vertex at the edn point
                    if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptE, out index))
                    {
                        ne = new Node3Dof(ptE);
                        ne.ID = nodeCounter;
                        nodeCounter++;
                        nodes.Add(ne);
                    }
                    else
                        ne = nodes[index];

                    Cable c = new Cable(ns, ne);
                    c.ID = i;
                    ns.AddEdge(c);
                    ne.AddEdge(c);
                    cables.Add(c);
                }
            }
            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node3Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, Cable>(cables));
        }
        
        /* Bars */
        public void CreateBarsFromLines(List<RhinoObject> selLine)
        {
            List<Node3Dof> nodes = new List<Node3Dof>();
            List<Bar> bars = new List<Bar>();
            int nodeCounter = 0;
            for (int i = 0; i < selLine.Count; i++)
            {
                int index = -1;
                Curve crv = selLine[i].Geometry as Curve;
                Point3d ptS = crv.PointAtStart;
                Point3d ptE = crv.PointAtEnd;
                Node3Dof ns, ne;

                //Check if there is a vertex at the start point
                if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptS, out index))
                {
                    ns = new Node3Dof(ptS);
                    ns.ID = nodeCounter;
                    nodeCounter++;
                    nodes.Add(ns);
                }
                else
                    ns = nodes[index];

                //Check if there is a vertex at the edn point
                if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptE, out index))
                {
                    ne = new Node3Dof(ptE);
                    ne.ID = nodeCounter;
                    nodeCounter++;
                    nodes.Add(ne);
                }
                else
                    ne = nodes[index];

                Bar c = new Bar(ns, ne);
                c.ID = i;
                ns.AddEdge(c);
                ne.AddEdge(c);
                bars.Add(c);
            }
            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node3Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, Bar>(bars));
        }
        
        /* Bend-Splines */
        public void CreateBSplinesFromLines(List<RhinoObject> selLine)
        {
            List<Node3Dof> nodes = new List<Node3Dof>();
            List<SplineB> sBars = new List<SplineB>();
            int nodeCounter = 0;
            int sbarCounter = 0;
            Line line0, line1, line2;
            SplineB sb0, sb1, sb2;

            foreach (RhinoObject ro in selLine)
            {
                List<Line> lines = SplitPolylineToLines(ro);

                for (int i = 0; i < lines.Count; i++)
                {
                    if (i == 0 && lines.Count > 1) 
                    {
                        line1 = lines[i];
                        line2 = lines[i + 1];
                        
                        sb1 = CreateOrGetBSpline(line1, nodes, sBars, ref nodeCounter, ref sbarCounter);
                        sb2 = CreateOrGetBSpline(line2, nodes, sBars, ref nodeCounter, ref sbarCounter);

                        sb1.NextSplineBar = sb2;
                        sb1.NextSplineBarIsSet = true;
                    }
                    else if (i < (lines.Count - 1))
                    {
                        line0 = lines[i-1];
                        line1 = lines[i];
                        line2 = lines[i + 1];

                        sb0 = CreateOrGetBSpline(line0, nodes, sBars, ref nodeCounter, ref sbarCounter);
                        sb1 = CreateOrGetBSpline(line1, nodes, sBars, ref nodeCounter, ref sbarCounter);
                        sb2 = CreateOrGetBSpline(line2, nodes, sBars, ref nodeCounter, ref sbarCounter);

                        sb1.PreviousSplineBar = sb0;
                        sb1.NextSplineBar = sb2;
                        sb1.PreviousSplineBarIsSet = true;
                        sb1.NextSplineBarIsSet = true;
                    }
                    else if (i == (lines.Count - 1)) 
                    {
                        line0 = lines[i - 1];
                        line1 = lines[i];

                        sb0 = CreateOrGetBSpline(line0, nodes, sBars, ref nodeCounter, ref sbarCounter);
                        sb1 = CreateOrGetBSpline(line1, nodes, sBars, ref nodeCounter, ref sbarCounter);
               
                        sb1.PreviousSplineBar = sb0;
                        sb1.PreviousSplineBarIsSet = true;
                    }
                }
            }
            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node3Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, SplineB>(sBars));
        }

        /* Bend-Twist-Splines */
        public void CreateBTSplinesFromLines(List<RhinoObject> selLine)
        {
            List<Node3Dof> nodes = new List<Node3Dof>();
            List<SplineBT> bendTwistSplines = new List<SplineBT>();
            int nodeCounter = 0;
            int sbarCounter = 0;
            Line line0, line1, line2;
            SplineB sb0, sb1, sb2;

            foreach (RhinoObject ro in selLine)
            {
                List<Line> lines = SplitPolylineToLines(ro);

                for (int i = 0; i < lines.Count; i++)
                {
                    if (i == 0 && lines.Count > 1)
                    {
                        line1 = lines[i];
                        line2 = lines[i + 1];

                        sb1 = CreateOrGetBTSpline(line1, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);
                        sb2 = CreateOrGetBTSpline(line2, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);

                        sb1.NextSplineBar = sb2;
                        sb1.NextSplineBarIsSet = true;
                    }
                    else if (i < (lines.Count - 1))
                    {
                        line0 = lines[i - 1];
                        line1 = lines[i];
                        line2 = lines[i + 1];

                        sb0 = CreateOrGetBTSpline(line0, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);
                        sb1 = CreateOrGetBTSpline(line1, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);
                        sb2 = CreateOrGetBTSpline(line2, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);

                        sb1.PreviousSplineBar = sb0;
                        sb1.NextSplineBar = sb2;
                        sb1.PreviousSplineBarIsSet = true;
                        sb1.NextSplineBarIsSet = true;
                    }
                    else if (i == (lines.Count - 1))
                    {
                        line0 = lines[i - 1];
                        line1 = lines[i];

                        sb0 = CreateOrGetBTSpline(line0, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);
                        sb1 = CreateOrGetBTSpline(line1, nodes, bendTwistSplines, ref nodeCounter, ref sbarCounter);

                        sb1.PreviousSplineBar = sb0;
                        sb1.PreviousSplineBarIsSet = true;
                    }
                }
            }
            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node3Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, SplineBT>(bendTwistSplines));
        }

        public List<Line> SplitPolylineToLines(RhinoObject ro)
        {
            List<Line> lines = new List<Line>();
            
            Curve c = ro.Geometry as Curve;
           
            if (c.IsPolyline())
            {
                Polyline pl = new Polyline();
                c.TryGetPolyline(out pl);
                if (pl != null)
                    for (int i = 0; i < pl.Count - 1; i++)
                        lines.Add(new Line(pl[i], pl[i + 1]));
            }
            return lines;
        }

        public List<Line> SplitPolylinesToLines(List<RhinoObject> selPLines)
        {
            List<Line> lines = new List<Line>();
            foreach (RhinoObject ro in selPLines)
            {
                Curve c = ro.Geometry as Curve;
                Polyline pl = new Polyline();

                if (c.IsPolyline())
                {
                    c.TryGetPolyline(out pl);
                    if (pl != null)
                    {
                        for (int i = 0; i < pl.Count - 1; i++)
                            lines.Add(new Line(pl[i], pl[i + 1]));
                    }
                }
            }
            return lines;
        }

        public SplineB CreateOrGetBSpline(Line line, List<Node3Dof> nodes, List<SplineB> sBars, ref int nodeCounter, ref int sbarCounter) 
        {
            Point3d ptS = line.From;
            Point3d ptE = line.To;
            Node3Dof ns, ne;
            SplineB sb;
            int index = -1;

            if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptS, out index))
            {
                ns = new Node3Dof(ptS);
                ns.ID = nodeCounter;
                nodeCounter++;
                nodes.Add(ns);
            }
            else
                ns = nodes[index];

            if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptE, out index))
            {
                ne = new Node3Dof(ptE);
                ne.ID = nodeCounter;
                nodeCounter++;
                nodes.Add(ne);
            }
            else
                ne = nodes[index];

            index = -1;

            if (!Util.ContainsEdge(Util.CastToSuperclass<Edge, SplineB>(sBars), (Vertex)ns, (Vertex)ne, out index))
            {
                sb = new SplineB(ns, ne);
                sb.ID = sbarCounter;
                ns.AddEdge(sb);
                ne.AddEdge(sb);
                sbarCounter++;
                sBars.Add(sb);
            }
            else
                sb = sBars[index];

            return sb;
        }

        public SplineB CreateOrGetBTSpline(Line line, List<Node3Dof> nodes, List<SplineBT> sBars, ref int nodeCounter, ref int sbarCounter)
        {
            Point3d ptS = line.From;
            Point3d ptE = line.To;
            Node3Dof ns, ne;
            SplineBT sb;
            int index = -1;

            if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptS, out index))
            {
                ns = new Node3Dof(ptS);
                ns.ID = nodeCounter;
                nodeCounter++;
                nodes.Add(ns);
            }
            else
                ns = nodes[index];

            if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptE, out index))
            {
                ne = new Node3Dof(ptE);
                ne.ID = nodeCounter;
                nodeCounter++;
                nodes.Add(ne);
            }
            else
                ne = nodes[index];

            index = -1;

            if (!Util.ContainsEdge(Util.CastToSuperclass<Edge, SplineBT>(sBars), (Vertex)ns, (Vertex)ne, out index))
            {
                sb = new SplineBT(ns, ne);
                sb.ID = sbarCounter;
                ns.AddEdge(sb);
                ne.AddEdge(sb);
                sbarCounter++;
                sBars.Add(sb);
            }
            else
                sb = sBars[index];

            return sb;
        }

        public void CreateConstraintObjects(List<RhinoObject> selPts)
        {
            int cpCounter = 0;

            for (int i = 0; i < selPts.Count; i++)
            {
                Point pt = selPts[i].Geometry as Point;
                Point3d ptDup = new Point3d(pt.Location.X, pt.Location.Y, pt.Location.Z);

                if (!Util.ContainsConstPt(_consPts, ptDup))
                {
                    _consPts.Add(new ConstraintPoint(ptDup, cpCounter));
                    cpCounter++;
                }
            }
        }

        public void CreateConstraintObjects(List<Point3d> pts)
        {
            int cpCounter = 0;

            for (int i = 0; i < pts.Count; i++)
            {
                Point3d pt = pts[i];
                Point3d ptDup = new Point3d(pt.X, pt.Y, pt.Z);

                if (!Util.ContainsConstPt(_consPts, ptDup))
                {
                    _consPts.Add(new ConstraintPoint(ptDup, cpCounter));
                    cpCounter++;
                }
            }
        }

        public void AssignConstPtsToVertices()
        {
            foreach (Node n in GetNodes())
            {
                foreach (ConstraintPoint cp in _consPts)
                {
                    if (n.Point.DistanceTo(cp.Point) < 0.00001)
                    {
                        n.Constraint = cp.Type;
                    }
                }
            }
        }

        /** Creates Plates, relying on that bars are sorted counter clockwise around the nodes.  
        * 
        */
        public override bool CreateFacesFromEdges()
        {
            if (_vertices.Count < 1) { return false; }

            Edge e1, e2;
            Vertex v1, v2;
            Vertex commonVertex;
            List<Edge> pEdges = new List<Edge>(4);
            List<Vertex> pVertices = new List<Vertex>(4);
            List<Edge> missingEdges = new List<Edge>();
            int plateCounter = 0;
            double angle;

            foreach (Vertex v in _vertices)
            {
                List<Edge> edges = v.GetEdges();
                List<double> edgeAngles = v.EdgeAngles;

                for (int i = 0; i < edges.Count; i++)
                {
                    if (v.GetEdges().Count < 3) break;

                    //Check angle, if greater than 180, no face should be created.
                    if (i == 0)
                        angle = edgeAngles[0];
                    else if (i == edges.Count - 1)
                        angle = 360 - edgeAngles[i - 1];
                    else
                        angle = Math.Abs(edgeAngles[i] - edgeAngles[i - 1]);
                    if (angle > 180) break;

                    if (i == (edges.Count - 1))
                    {
                        e1 = edges[i];
                        e2 = edges[0];
                    }
                    else
                    {
                        e1 = edges[i];
                        e2 = edges[i + 1];
                    }

                    v1 = Util.GetOtherEnd(e1, v);
                    v2 = Util.GetOtherEnd(e2, v);

                    pEdges.Clear();
                    pVertices.Clear();
                    missingEdges.Clear();

                    pVertices.Add(v);
                    pVertices.Add(v1);
                    pEdges.Add(e1);

                    commonVertex = new Vertex(-1);

                    if (FindMissingEdge(v, v1, v2, missingEdges, ref commonVertex))
                    {
                        if (commonVertex.ID != -1 && missingEdges.Count == 2)
                        {   //Quad
                            pVertices.Add(commonVertex);
                            pVertices.Add(v2);
                            pEdges.Add(missingEdges[0]);
                            pEdges.Add(missingEdges[1]);
                            pEdges.Add(e2);
                        }
                        else
                        {   //Triangle 
                            pVertices.Add(v2);
                            pEdges.Add(missingEdges[0]);
                            pEdges.Add(e2);
                        }

                        if (!Util.ContainsFace(_faces, pVertices))
                        {
                            //Create face object, assign all topology, and add to list of faces.
                            _faces.Add((Face)CreatePlate(pVertices, pEdges, plateCounter));
                            plateCounter++;
                        }
                    }
                }
            }
            return true;
        }

        private Face CreatePlate(List<Vertex> fVertices, List<Edge> fEdges, int id)
        {
            Plate plate = new Plate(id);
            plate.AddVertices(fVertices);
            plate.AddEdges(fEdges);

            foreach (Edge e in fEdges)
                e.AddFace((Face)plate);

            foreach (Vertex v in fVertices)
                v.AddFace((Face)plate);

            return plate;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        public void StichExternalObjects(List<Vertex> vs, List<Edge> es, List<ConstraintPoint> cpts, Parameters p)
        { 
            base.StichExternalObjects(vs, es);

            CheckExternalConstPts(cpts);

            AssignConstPtsToVertices();

            PreprocessStructure(p);
        }

        public void CheckExternalConstPts(List<ConstraintPoint> cpts) 
        {
            int id = 0;
            foreach (ConstraintPoint cp in cpts) 
            {   
                //Avoid duplicated constpts. 
                if(!Util.ContainsConstPt(_consPts, cp.Point))
                {
                    cp.ID = id;
                    _consPts.Add(cp);
                    id++;
                }
            }
        }


        //---------------------------- Initial Calculations --------------------------------//
        #region


        public void PreprocessStructure(Parameters p)
        {
            InitialiseCoordinateSystems();

            InitialiseNodeAndElementProperties(p);

            CalcMass(p);

            CalcLoadFromMass(p);

            CalcLoadFromSurface(p);

            ApplyLoad(p);

            CalcMaxMinLoadPerNode();

            CalcTotalStructuralWeight();

            SetColorBy(p);

            _isDarwable = true;

            _isAnalysable = true;
        }


        public void CalcTotalStructuralWeight()
        {
            _structWeight = 0;
            foreach (Bar b in this.GetBars())
                _structWeight += b.Mass;

            _structWeight /= 1000; //tons
        }

        private void InitialiseCoordinateSystems()
        {
            foreach (Node n in GetNodes())
                n.InitialiseCoordSystem();
            
            foreach (Element e in GetElements())
                e.InitialiseCoordSystem();
        }

        private void InitialiseNodeAndElementProperties(Parameters p)
        {
            foreach (Element e in GetElements())
            {
                e.CalcInitialTwist();
                e.CalcSectionProperties();
                e.InitialiseMesh();
            }
            foreach (Node n in GetNodes())
            {
                n.CalcFictiousMass(p.TimeStep, p.TranslationalStiffnessScaleFactor, p.RotationalStiffnessScaleFactor);
                n.CalcStabilityLimit();
            }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------------- Calculations ------------------------------------//
        #region

        /** Mass/Loading calculations
        * 
        */
        public void CalcMass(Parameters p)
        {
            foreach (Element e in GetElements())
                e.CalcMass();
        }

        public void CalcLoadFromMass(Parameters p)
        {
            foreach (Element e in GetElements())
                if (p.IsSelfWeight)
                    e.CalcLoadFromMass(p.GravityVector, p.GravityMagnitude, p.LoadType);
        }

        public void CalcLoadFromSurface(Parameters p)
        {
            foreach (Element e in GetElements())
                if (p.IsSurfaceLoad)
                    e.CalcLoadFromSurface(p.SurfaceLoadVector, 1, p.LoadType);   //Setting the magnitude to be one, such that when the application happens the load can be scaled with a magnitude SF.
        }

        public void ApplyLoad(Parameters p)
        {
            foreach (Element e in GetElements())
            {
                if (p.IsSelfWeight)
                    e.ApplyLoadFromMass();

                if (p.IsSurfaceLoad)
                    e.ApplyLoadFromSurface();
            }
        }

        public void ClearNodeLoad() 
        {
            foreach (Node n in GetNodes())
                n.ClearLoad();
        }

        /** Relaxation
        * 
        */
        public void ResetForcesMoments() 
        {
            foreach (Node n in GetNodes())
                n.Clear();
        }

        public void CalcFictiousMass(Parameters p) 
        {
            foreach (Element e in GetElements())
            {
                e.CalcNodeStiffness();
                e.ApplyNodeStiffness();
            }
            foreach (Node n in GetNodes())
                n.CalcFictiousMass(p.TimeStep, p.TranslationalStiffnessScaleFactor, p.RotationalStiffnessScaleFactor);
        }

        public void CalcStrain(Parameters p) 
        {
            foreach(Element e in GetElements())
                e.ClearForce();

            foreach (Element e in GetElements()) 
            {
                e.CalcLength();
                e.CalcDeformation();
                e.CalcStrain();
            }
        }

        /** Calculate forces and moments in beams.
        *
        */
        public void CalcForceAndMoments(Parameters p)
        {
            foreach (Element e in GetElements())
                e.ClearForce();

            foreach (Beam b in GetBeams())
            {
                //Calculate the beam length due to the moved nodes in the last iteration
                b.CalcLength();
                b.CalcDeformation();

                b.ScaleFactorEA = 1;
                b.ScaleFactorEI = 1;

                //ELASTIC
                if (p.BeamMode == eBeamMode.PropertyControl)
                {
                    b.ScaleFactorEA = p.AxialSF;
                    b.ScaleFactorEI = p.BendingSF;
                }

                b.CalcAxialForce();
                b.CalcMoment();

                //PLASTIC
                if (p.BeamMode == eBeamMode.LimitControl)
                {
                    b.ScaleMomentAndForcePlastic(p.AxialSF, p.BendingSF);
                }
                //FORCE/MOMENT
                else if (p.BeamMode == eBeamMode.ForceMomentControl)
                {
                    b.ScaleMomentAndForce(p.AxialSF, p.BendingSF);
                }
            }
        }



        public void ApplyStrainToNodes() 
        {
            foreach (Element e in GetElements())
            {
                e.ApplyStrainToNodes();
            }
        }

        public void TransformNodes(Parameters p) 
        {
            foreach (Node3Dof n in GetNodes())
            {
                n.TransformDueToStrain(p.TimeStep, p.ViscousDamping, p.LoadScaleFactor, p.IsSelfWeight, p.SurfaceLoadMagnitude);
            }
        }

        /** Sizing
        * 
        */
        public void Sizing(Parameters p)
        {
            foreach (Element e in GetElements())
            {
                if (p.SizingMode == eSizingMode.Direct)
                    e.DirectSizing();
                else
                    e.IncrementalSizing();
            }
        }

        
        
        public void CalcCrossSectionProperties() 
        {
            foreach (Element e in GetElements()) 
                e.CalcSectionProperties();
        }



        public void CalcMaxMinLoadPerNode()
        {
            double maxLoadPerNode = 0;
            double minLoadPerNode = double.MaxValue;

            foreach (Node3Dof n in GetNode3dofs())
            {
                double[] loadM = n.LoadFromMass;
                double[] loadS = n.LoadFromSurface;
                double[] load = new double[3] { 0, 0, 0 };

                load[0] = loadM[0] + loadS[0];
                load[1] = loadM[1] + loadS[1];
                load[2] = loadM[2] + loadS[2];

                double loadMag = Math.Sqrt(load[0] * load[0] + load[1] * load[1] + load[2] * load[2]);

                if (loadMag > maxLoadPerNode)
                    maxLoadPerNode = loadMag;

                if (loadMag < minLoadPerNode)
                    minLoadPerNode = loadMag;
            }
            if (minLoadPerNode == double.MaxValue)     //No nodes
                minLoadPerNode = 0;

            _maxNodeLoad = maxLoadPerNode;
            _minNodeLoad = minLoadPerNode;
        }












        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------- Update/Postprocess ---------------------------------//
        #region

        public override void UpdateGeometry(Parameters p)
        {
            base.UpdateGeometry(p);
            
            SetColorBy(p);

            CalcMinMaxVisValue(p);

            UpdateElementMesh(p);
        }

        public void Rewind() 
        {
            //Rewind is virtual, so each node and element type rewinds its own state.
            //Bar and cable models have Node3Dof vertices, so these must not be cast to Node6Dof.
            foreach (Vertex v in _vertices)
                v.Rewind();

            foreach (Edge e in _edges)
                e.Rewind();
        }

        public void PostProcess(Parameters p)
        {
            foreach (Element e in GetElements())
            {
                e.PostProcess();
            }
            CalcMinMaxVisValue(p);
        }

        private void UpdateElementMesh(Parameters p)
        {
            foreach (Element e in GetElements())
            {
                e.BuildMesh();

                //HACK
                if(p.ColorBy == eColorBy.None)
                    e.ColorMesh(0, 1, 0, 1);
                else
                    e.ColorMesh(p.RealMin, p.RealMax, p.ShowMin, p.ShowMax);
            }

            this.BuildMesh();
        }

        private void CalcMinMaxVisValue(Parameters p)
        {
            double _minColorByValue = 10000000000;
            double _maxColorByValue = -10000000000;
            foreach (Element e in GetElements())
            {
                for (int i = 0; i < 2; i++)
                {
                    if (e.ColorBy[i] < _minColorByValue)
                        _minColorByValue = e.ColorBy[i];

                    if (e.ColorBy[i] > _maxColorByValue)
                        _maxColorByValue = e.ColorBy[i];
                }
            }
            p.RealMin = _minColorByValue;
            p.RealMax = _maxColorByValue;
        }

        private void SetColorBy(Parameters p) 
        {
            foreach (Bar b in GetBars())
            {
                b.SetColorBy(p);
            }
        }


        #endregion
        //----------------------------------------------------------------------------------//
        

        //--------------------------------- Utilities --------------------------------------//
        #region

        public void DebugBake()
        {
            foreach (Element e in GetElements())
            {
                Rhino.RhinoDoc.ActiveDoc.Objects.Add(e.GetMesh());
            }

            foreach (Element e in GetElements())
            {
                Mesh mesh = e.GetMesh();

                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    MeshFace mf = mesh.Faces[i];
                    Brep brep;

                    int index1 = mf.A;
                    int index2 = mf.B;
                    int index3 = mf.C;
                    int index4 = mf.D;

                    Point3d p1 = mesh.Vertices[index1];
                    Point3d p2 = mesh.Vertices[index1];
                    Point3d p3 = mesh.Vertices[index1];
                    Point3d p4 = mesh.Vertices[index1];

                    brep = Brep.CreateFromCornerPoints(p1, p2, p3, p4, 0.01);

                    Rhino.RhinoDoc.ActiveDoc.Objects.Add(brep);
                }
            }
        }

        public void ExportToCsv()
        {
            // Simple CSV export  
            CsvExport sectionExport = new CsvExport();

            foreach (Bar b in GetBars())
            {
                //Section properties
                sectionExport.AddRow();
                sectionExport["Section #"] = "Section " + (b.ID + 1);
                sectionExport["Material"] = "Steel";
                sectionExport["Description"] = "STD CHS(m) " + (b.CrossSection.RadiusOuter * 2) + " " + (b.CrossSection.RadiusOuter - b.CrossSection.RadiusInner);
                sectionExport["Axes for Stiffness Calculation"] = "yz axes";
                sectionExport["Area"] = "" + b.CrossSection.Area;
                sectionExport["Iyy"] = "" + b.CrossSection.Ix;
                sectionExport["Izz"] = "" + b.CrossSection.Iy;
                sectionExport["J"] = "" + b.CrossSection.J;
            }

            //Then you can do any of the following three output options: 
            string mySectionCsv = sectionExport.Export();
            sectionExport.ExportToFile("CrossSections.csv");
            byte[] mySectionCsvData = sectionExport.ExportToBytes();


            // Simple CSV export  
            CsvExport topologyExport = new CsvExport();

            foreach (Edge e in _edges)
            {
                //Section properties
                topologyExport.AddRow();
                topologyExport["Type"] = "Beam";
                topologyExport["Property"] = "" + (e.ID + 1);
                topologyExport["Group"] = "1";
                topologyExport["Orientation node"] = "None";
                topologyExport["Orientation angle"] = "0";
                topologyExport["Topology 1"] = "" + (_vertices.IndexOf(e.GetStart()) + 1);
                topologyExport["Topology 2"] = "" + (_vertices.IndexOf(e.GetEnd()) + 1);
            }

            //Then you can do any of the following three output options: 
            string myBeamCsv = topologyExport.Export();
            topologyExport.ExportToFile("TopologyExport.csv");
            byte[] myBeamCsvData = topologyExport.ExportToBytes();

            //And for the nodes..
            CsvExport nodeExport = new CsvExport();
            int i = 1;

            foreach (Node n in GetNodes())
            {
                //Node coordinates
                nodeExport.AddRow();
                nodeExport["Node #"] = "" + i;
                nodeExport["X"] = "" + n.GetInitialPosition()[0];
                nodeExport["Y"] = "" + n.GetInitialPosition()[1];
                nodeExport["Z"] = "" + n.GetInitialPosition()[2];

                i++;
            }

            //Then you can do any of the following three output options: 
            string myNodeCsv = nodeExport.Export();
            nodeExport.ExportToFile("Nodes.csv");
            byte[] myNodeCsvData = nodeExport.ExportToBytes();
        }

        public List<Line> GetLines() 
        {
            List<Line> lines = new List<Line>();
            foreach (Bar b in _edges) 
                lines.Add(new Line(b.GetStart().Point, b.GetEnd().Point));

            return lines;
        }

        public List<Mesh> Get3dBars()
        {
            List<Mesh> barMesh = new List<Mesh>();
            foreach (Bar b in _edges)
                barMesh.Add(b.CrossSection.GetMesh());

            return barMesh;
        }

        public List<Point3d> GetPoints()
        {
            List<Point3d> pts = new List<Point3d>();
            foreach (Vertex v in _vertices)
                pts.Add(v.Point);

            return pts;
        }




        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------- Old algorithm ------------------------------------//
        #region

        /** Set forces and moments in the beams to zeros, also reset the _newRot variable. 
        *
        */
        //public void ResetForcesMoments()
        //{
        //    foreach (NodeBeam nb in _vertices)
        //    {
        //        nb.ResetForcesMomentsRotations();
        //    }
        //}

        /** Calculate the mass vector for the node. 
        *
        */
        //public void CalcTranslationMass(double dt)
        //{
        //    foreach (NodeBeam nb in _vertices)
        //    {
        //        nb.CalcMass4(dt);
        //    }
        //}

        /** Calculate forces and moments in beams.
        *
        */
        //public void CalcForceAndMoments(RelaxParameters aParams)
        //{
        //    foreach (BeamRelax b in _links)
        //    {
        //        //Calculate the beam length due to the moved nodes in the last iteration
        //        b.CalcLength();

        //        //Calculate the displacements based on the moved and rotaded nodes.
        //        if (b.ElementType == ElementType.beam || b.ElementType == ElementType.beamCable)
        //            b.ClacEndRotationAndTwist();

        //        b.CalcElongation();

        //        //ELASTIC
        //        if (b.ModeControl == false || b.AnalysisControl == AnalysisControl.elastic)
        //        {
        //            b.CalcAxialForce();
        //            if (b.ElementType == ElementType.beam || b.ElementType == ElementType.beamCable)
        //                b.CalcMoment();
        //        }
        //        //PLASTIC
        //        else if (b.ModeControl == true && b.AnalysisControl == AnalysisControl.plastic)
        //        {
        //            b.CalcAxialForce();

        //            //Beam
        //            if (b.ElementType == ElementType.beam || b.ElementType == ElementType.beamCable)
        //            {
        //                b.CalcMoment();
        //                //If the utilization is separated into axial and bending (custom control) or not.
        //                if (aParams.CustomControl)
        //                    b.ScaleMomentAndForcePlastic(aParams.AxialUtilization, aParams.BendingUtilization);
        //                else
        //                    b.ScaleMomentAndForcePlastic();
        //            }
        //            //Bar/Cable
        //            else
        //            {
        //                if (aParams.CustomControl)
        //                    b.ScaleForceFromUtilization(aParams.AxialUtilization);
        //                else
        //                    b.ScaleMomentAndForcePlastic();
        //            }
        //        }
        //        //FORCE/MOMENT
        //        else if (b.ModeControl == true && b.AnalysisControl == AnalysisControl.forceMoment)
        //        {
        //            b.CalcAxialForce();

        //            //Beam
        //            if (b.ElementType == ElementType.beam || b.ElementType == ElementType.beamCable)
        //            {
        //                b.CalcMoment();
        //                //If the utilization is separated into axial and bending (custom control) or not.
        //                if (aParams.CustomControl)
        //                    b.ScaleMomentAndForce(aParams.AxialUtilization, aParams.BendingUtilization);
        //                else
        //                    b.ScaleMomentAndForce();
        //            }

        //        }

        //        //Apply releases. Not working with line loads!!!
        //        if (b.ReleaseType != ReleaseType.none)
        //            b.ApplyReleases();

        //        //Convert the forces and moments to nodal coordinate system and adds them to the node.
        //        b.ApplyForceToNodes();

        //        if (b.ElementType == ElementType.beam || b.ElementType == ElementType.beamCable)
        //            b.ApplyMomentsToNodes();
        //    }
        //}


        /** Based on the out of balance force and out of balance moment translate and rotate nodes.
        *
        */
        //public void MoveNodes(double dt, double cf)
        //{
        //    foreach (NodeBeam nb in _vertices)
        //    {
        //        nb.CalcTranslation(dt, cf);
        //        nb.CalcRotation(dt, cf);
        //    }
        //}

        /** Rotate the coordinate system for the node and the associated beam ends.
        *
        */
        //public void UpdateCS()
        //{
        //    foreach (NodeBeam nb in _vertices)
        //    {
        //        nb.RotateCS();
        //    }
        //    foreach (BeamRelax br in _links)
        //    {
        //        br.UpdateBaseCS();
        //    }
        //}
    
        #endregion
        //----------------------------------------------------------------------------------//

    }
}
