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
    public class Face
    {
        protected List<Vertex> _vertices;
        protected List<Face> _faces;
        protected List<Edge> _edges;
        protected Vector3d _normal;
        protected Point3d _midPt;
        protected bool _isNormalUnified;
        protected double _area;
        protected double _projArea;
        protected int _id;


        public void Initialise()
        {
            _vertices = new List<Vertex>();
            _faces = new List<Face>();
            _edges = new List<Edge>();
            _normal = new Vector3d();
            _midPt = new Point3d();
            _isNormalUnified = false;
            _area = 0;
            _id = -1;

        }


        //------------------------------- Constructors -------------------------------------//
        #region

        public Face() 
        {
            Initialise();
        }

        public Face(int id)
        {
            Initialise();
            _id = id;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------------- Properties --------------------------------------//
        #region

        public List<Edge> GetEdges() 
        {
            return _edges;
        }

        public List<Vertex> GetVertices()
        {
            return _vertices;
        }

        public List<Face> GetFaces()
        {
            return _faces;
        }

        public void AddEdge(Edge e) 
        {
            _edges.Add(e);
        }

        public void AddVertex(Vertex v)
        {
            _vertices.Add(v);
        }

        public void AddVertices(List<Vertex> vs)
        {
            _vertices.AddRange(vs);
        }

        public void AddEdges(List<Edge> es)
        {
            _edges.AddRange(es);
        }

        public void AddFace(Face f)
        {
            _faces.Add(f);
        }

        public void ClearVertices() 
        {
            _vertices.Clear();
        }

        public void ClearEdges()
        {
            _edges.Clear();
        }

        public bool IsNormalUnified 
        {
            get { return _isNormalUnified; }
            set { _isNormalUnified = value; }
        }

        public Vector3d Normal 
        {
            get { return _normal; }
            set { _normal = value; }
        }

        public Point3d MidPoint
        {
            get { return _midPt;}
            set { _midPt = value;}
        }

        public double Area
        {
            get { return _area; }
            set { _area = value; }
        }

        public double ProjectedArea
        {
            get { return _projArea; }
            set { _projArea = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------- Geometry calculations --------------------------------//
        #region


        public void CalcMidPoints() 
        {
            _midPt = new Point3d(0,0,0); 
            foreach (Vertex v in _vertices)
                _midPt += v.Point / _vertices.Count;
        }

        public void CalcArea() 
        {
            Vector3d v1, v2, v3, v4;
            v1 = _vertices[0].Point - _vertices[1].Point;
            v2 = _vertices[2].Point - _vertices[1].Point;

            if (_vertices.Count == 3)
            {
                _area = Vector3d.CrossProduct(v1, v2).Length / 2;
                _normal = Vector3d.CrossProduct(v1, v2);
            }
            else if (_vertices.Count == 4) 
            {
                v3 = _vertices[0].Point - _vertices[3].Point;
                v4 = _vertices[2].Point - _vertices[3].Point;
                _area = (Vector3d.CrossProduct(v1, v2).Length / 2) + (Vector3d.CrossProduct(v3, v4).Length / 2);
                _normal = Vector3d.CrossProduct(v1, v2) + Vector3d.CrossProduct(v3, v4); 
            }

            _normal.Unitize();
        }

        public void CalcProjectedArea()
        {
            //Assuming for the moment that the projection happens to the xy plane.
            Vector3d v1, v2, v3, v4;
            v1 = _vertices[0].Point - _vertices[1].Point;
            v2 = _vertices[2].Point - _vertices[1].Point;

            //Performing the projection by deleting the z component.
            v1.Z = 0;
            v2.Z = 0;

            Vector3d tempNormal = new Vector3d();

            if (_vertices.Count == 3)
            {
                _projArea = Vector3d.CrossProduct(v1, v2).Length / 2;
                tempNormal = Vector3d.CrossProduct(v1, v2);
            }
            else if (_vertices.Count == 4)
            {
                v3 = _vertices[0].Point - _vertices[3].Point;
                v4 = _vertices[2].Point - _vertices[3].Point;
                _projArea = (Vector3d.CrossProduct(v1, v2).Length / 2) + (Vector3d.CrossProduct(v3, v4).Length / 2);
                tempNormal = Vector3d.CrossProduct(v1, v2) + Vector3d.CrossProduct(v3, v4);
            }
        }






        public void CalcNormal()
        {
            Vector3d v1, v2, v3, v4;
            Vector3d n1, n2;
            v1 = _vertices[0].Point - _vertices[1].Point;
            v2 = _vertices[2].Point - _vertices[1].Point;

            if (_vertices.Count == 3)
                _normal = Vector3d.CrossProduct(v1, v2);

            else if (_vertices.Count == 4)
            {
                v3 = _vertices[0].Point - _vertices[3].Point;
                v4 = _vertices[2].Point - _vertices[3].Point;
                n1 = Vector3d.CrossProduct(v1, v2); 
                n2 = Vector3d.CrossProduct(v3, v4);
                if (!Util.IsSameDirection(n1, n2))
                    n2.Reverse();
                _normal = n1 + n2;
            }
            _normal.Unitize();
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------------- Utilities --------------------------------------//
        #region

        public List<int> GetVertexIds()
        {
            List<int> vsIds = new List<int>();
            foreach (Vertex v in _vertices)
                vsIds.Add(v.ID);

            return vsIds;
        }

        public void ReverseNormal()
        {
            _normal.Reverse();
            _vertices.Reverse();
        }

        #endregion
        //----------------------------------------------------------------------------------//

    }
}
