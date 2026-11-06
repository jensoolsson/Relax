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
    public class Edge
    {
        protected List<Vertex> _vertices;           //Neighbouring vertices
        protected List<Face> _faces;                //Neighbouring faces    
        protected double _initLength;               //Initial lenght
        protected double _length;                   //Length
        protected double _projLength;                   //Length
        protected double _projArea;                   //Length
        protected int _id;                          //Unique identifier
        protected Point3d _midPt;

        public void Initialise() 
        {
            _faces = new List<Face>();
            _vertices = new List<Vertex>();
            _midPt = new Point3d(0, 0, 0);
            _id = -1;
        }

        //---------------------------------- Constructors ----------------------------------//
        #region

        public Edge() 
        {
            Initialise();
        }

        public Edge(int id)
        {
            Initialise();
            _id = id;
        }

        public Edge(Vertex vertexStart, Vertex vertexEnd)
        {
            Initialise();
            _vertices.Add(vertexStart);
            _vertices.Add(vertexEnd);
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Properties -----------------------------------//
        #region

        public virtual Vertex GetStart() 
        {
            return _vertices[0];
        }

        public virtual void SetStart(Vertex v)
        {
            _vertices[0] = v;
        }

        public virtual Vertex GetEnd()
        {
            return _vertices[1];
        }

        public virtual void SetEnd(Vertex v)
        {
            _vertices[1] = v;
        }

        public virtual List<Vertex> GetVertices()
        {
            return _vertices;
        }

        public void AddFace(Face f) 
        {
            _faces.Add(f);
        }

        public List<Face> GetFaces()
        {
            return _faces;
        }

        public double Length 
        {
            get { return _length; }
            set { _length = value; }
        }

        public double InitialLength 
        {
            get { return _initLength; }
            set { _initLength = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public Point3d MidPoint 
        {
            get { return _midPt; }
            set { _midPt = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Calculations ----------------------------------//
        #region

        public virtual void CalcLength() 
        {
            _length = GetStart().Point.DistanceTo(GetEnd().Point);
        }

        public virtual void CalcInitialLength()
        {
            _initLength = GetStart().Point.DistanceTo(GetEnd().Point);
        }

        public virtual void CalcProjectedLength()
        {
            //Assuming the elment to projected on the xy plane, => zz = 0.
            double xx = GetStart().Point.X - GetEnd().Point.X;
            double yy = GetStart().Point.Y - GetEnd().Point.Y;

            _projLength = Math.Sqrt(xx * xx + yy * yy);
        }

        public virtual void CalcProjectedArea()
        {
            _projArea = 0;
            for (int i = 0; i < _faces.Count; i++)
                _projArea += _faces[i].ProjectedArea / _faces[i].GetEdges().Count;
        }

        
        public virtual void CalcMidPoint() 
        {
            _midPt = (_vertices[0].Point + _vertices[1].Point) / 2;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        public virtual void CompleteFaceTopology() 
        {
            if (_faces.Count == 2)
            {
                _faces[0].AddFace(_faces[1]);
                _faces[1].AddFace(_faces[0]);
            }
        }

        public virtual void Rewind()
        {
            _length = _initLength;
        } 


    }
}
