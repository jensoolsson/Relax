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
    public class Vertex
    {
        protected double[] _x;                      //Position
        protected double[] _initX;                  //Initial position
        protected List<Edge> _edges;                //Neighbouring edges
        protected List<Vertex> _vertices;           //Neighbouring vertices
        protected List<Face> _faces;                //Neighbouring faces    
        protected int _valence;                     //Number of edges connected to node
        protected int _id;                          //Unique identiy number for the object
        protected List<double> _edgeAngles;                 //Angles between edges sorted in counter clockwise order
        protected Point3d _pt;                      //Vertex representation as Point3d object.
        protected Vector3d _normal;                 //Normal vector
        protected List<double[]> _positions;        //Saved positions                    
        protected double _connectedEdgeLength;      //The length of edges comming in to the node.
        protected double _area;


        public void Initialise()
        {
            _x = new double[3] { 0, 0, 0 };
            _initX = new double[3] { 0, 0, 0 };
            _edges = new List<Edge>();
            _vertices = new List<Vertex>();
            _faces = new List<Face>();
            _valence = -1;
            _id = -1;
            _edgeAngles = new List<double>();
            _pt = new Point3d();
            _normal = new Vector3d();
            _positions = new List<double[]>();
            _connectedEdgeLength = 0;
            _area = 0;
        }


        //--------------------------------- Constructors -----------------------------------//
        #region

        public Vertex()
        {
            Initialise();
        }

        public Vertex(int id)
        {
            Initialise();
            _id = id;
        }

        public Vertex(Point3d pt)
        {
            Initialise();
            _x[0] = pt.X;
            _x[1] = pt.Y;
            _x[2] = pt.Z;
            _pt = new Point3d(pt);
            _initX[0] = pt.X;
            _initX[1] = pt.Y;
            _initX[2] = pt.Z; 
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------- Properties add and set methods -------------------------//
        #region

        public List<double> EdgeAngles
        {
            get { return _edgeAngles; }
            set { _edgeAngles = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public void AddEdge(Edge e)
        {
            _edges.Add(e);
        }

        public List<Edge> GetEdges()
        {
            return _edges;
        }

        public void AddFace(Face f)
        {
            _faces.Add(f);
        }

        public Vector3d Normal
        {
            get { return _normal; }
            set { _normal = value; }
        }

        public double ConnectedEdgeLength
        {
            get { return _connectedEdgeLength; }
            set { _connectedEdgeLength = value; }
        }

        public double[] GetPosition()
        {
            return _x;
        }

        public double[] GetInitialPosition()
        {
            return _initX;
        }

        public void SetPosition(Point3d pt)
        {
            _x[0] = pt.X;
            _x[1] = pt.Y;
            _x[2] = pt.Z;
            _pt = new Point3d(pt);
        }

        public void SetPosition(double x, double y, double z)
        {
            _x[0] = x;
            _x[1] = y;
            _x[2] = z;
            _pt = new Point3d(x, y, z);
        }

        public void SetPosition(double[] x)
        {
            _x[0] = x[0];
            _x[1] = x[1];
            _x[2] = x[2];
            _pt = new Point3d(x[0], x[1], x[2]);
        }

        public Point3d Point
        {
            get { return _pt; }
            set
            {
                _pt = value;
                _x[0] = value.X;
                _x[1] = value.Y;
                _x[2] = value.Z;
            }
        }

        public double[] InitialPosition
        {
            get { return _initX; }
            set
            {
                _initX[0] = value[0];
                _initX[1] = value[1];
                _initX[2] = value[2];
            }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------- Pre-process geometry -------------------------------//
        #region

        //Functions organized in the order they should be called.

        /** Sort edges in counter clockwise order. 
        * 
        */
        public void SortEdges() 
        {
            _valence = _edges.Count;

            if (_valence < 2) return;

            List<Point3d> projPts = new List<Point3d>();
            List<Vertex> vertexRing = new List<Vertex>();
            Vector3d planeNormal = new Vector3d();
            Point3d projCenterPt = new Point3d();

            List<double> angles = new List<double>(_valence - 1);

            for (int i = 0; i < _valence; i++) 
                vertexRing.Add(Util.GetOtherEnd(_edges[i], this));

            Util.CalcPlaneFromPoints(this, vertexRing, ref projCenterPt, projPts, ref planeNormal);
            Util.CalcAnglesInPlane(projCenterPt, projPts, angles, planeNormal);
            Util.SortEdgesByAngle(_edges, angles);
            _edgeAngles = angles;

            foreach(Edge e in _edges)
                _vertices.Add(Util.GetOtherEnd(e, this));
        }

        public void UnifySorting() 
        {
            if (_edges.Count < 2) { return; }

            int index = Util.GetEdgePairWithSmallestAngles(_edges, _edgeAngles); 

            Vector3d v1 = Util.GetOtherEnd(_edges[index], this).Point - _pt;
            Vector3d v2 = Util.GetOtherEnd(_edges[index+1], this).Point - _pt;

            v1.Unitize();
            v2.Unitize();

            Vector3d crossVec = Vector3d.CrossProduct(v1, v2);

            if(!Util.IsSameDirection(_normal, crossVec))
            {
                _edges.Reverse();
                _edgeAngles.Reverse();
                _vertices.Reverse();
            }
        }


        /** Calculate normal from surrounding face normals.
        * 
        */
        public void CalcNormal()
        {
            Vector3d normal = new Vector3d();
            foreach (Face f in _faces)
                normal += f.Normal;

            normal.Unitize();
            _normal = normal;
        }

        /** Calculates the area surrounding the vertex.
        * 
        */
        public void CalcArea()
        {
            double area = 0;
            foreach (Face face in _faces)
            {
                face.CalcArea();
                if (face.GetVertices().Count == 3)      //Triangle 
                    area += face.Area / 3;
                else if (face.GetVertices().Count == 4) //Quad 
                    area += face.Area / 4;
            }
            _area = area;
        }

        /** Calculate the length of beams that connect to this node.
        *   
        */
        public void CalcConnectedEdgeLength()
        {
            _connectedEdgeLength = 0;
            foreach (Edge e in _edges)
                _connectedEdgeLength += e.InitialLength;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Utilities -------------------------------------//
        #region

        /** Add current location to vector that stores positions.
        * 
        */
        public void SaveState()
        {
            _positions.Add(new double[] { _x[0], _x[1], _x[2] });
        }

        /** Mode node to a position saved in the _positions array.
        * 
        */
        public void ApplyState(int index)
        {
            if (_positions.Count > index)
            {
                _x[0] = _positions[index][0];
                _x[1] = _positions[index][1];
                _x[2] = _positions[index][2];
                SetPosition(_positions[index]);
            }
        }


        public virtual void Rewind() 
        {
            _x = _initX;
            _pt.X = _initX[0];
            _pt.Y = _initX[1];
            _pt.Z = _initX[2];
        }

        #endregion
        //----------------------------------------------------------------------------------//

       

    }
}
