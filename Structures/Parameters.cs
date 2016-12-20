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
    public class Parameters
    {
        //Visualisation parameters
        protected bool _drawVertexNormals;
        protected bool _drawFaceNormals;
        protected bool _drawNodes;
        protected bool _drawLines;
        protected bool _drawMeshWires;
        protected bool _drawMeshShades;
        protected bool _draw3dBars;
        protected bool _drawNodeRing;

        protected bool _drawVertexID;
        protected bool _drawEdgeID;
        protected bool _drawFaceID;
        protected bool _drawDebug;
        protected bool _drawNodeCS;
        protected bool _drawBarCS;

        protected eColorBy _colorBy;
        protected double _showMin;
        protected double _showMax;

        //Analysis parameters
        protected bool _isSelfWeight;
        protected bool _isSurfaceLoad;
        protected Vector3d _gravityVector;
        protected eLoadType _loadType;
        protected double _gravityMag;
        protected Vector3d _srfLoadVector;
        protected double _srfLoadMag;
        protected eSizingMode _sizingMode;
        protected eElementType _elemType;
        protected double _loadSF;

        protected double _dt;                         //Time step
        protected double _vd;                         //Viscous damping
        protected double _tol;                        //Tolerance

        //Parameters feed back from calculations to UI:
        protected double _obf;
        protected double _obm;
        protected double _convergForce;
        protected double _strucWeight;
        protected int _drCounter;
        protected int _sizingCounter;
        protected double _realMin;
        protected double _realMax;

        protected int _nrIterations;

        public void Initialise() 
        {
            _drawVertexNormals = false;
            _drawFaceNormals = false;
            _drawNodes = false;
            _drawLines = false;
            _drawMeshWires = false;
            _drawMeshShades = false;
            _draw3dBars = false;

            _drawVertexID = false;
            _drawEdgeID = false;
            _drawFaceID = false;
            _drawDebug = false;

            _colorBy = eColorBy.Utilisation;
            _showMin = 0;
            _showMax = 100; 

            _isSelfWeight = true;
            _isSurfaceLoad = true;
            _loadType = eLoadType.PointLoad;
            _gravityVector = new Vector3d(0, 0, -1);
            _gravityMag = 9.81;
            _srfLoadVector = new Vector3d(0, 0, -1);
            _srfLoadMag = 0;
            _sizingMode = eSizingMode.Direct;
            _dt = 0.01;
            _vd = 0.95;
            _tol = 0.00001;
            _elemType = eElementType.Beam;
            _loadSF = 1;

            _obf = 0;
            _obm = 0;
            _convergForce = 0;
            _strucWeight = 0;
            _drCounter = 0;
            _sizingCounter = 0;

            _nrIterations = 10;
            
        }

        //----------------------------------------------------------------------------------//
        #region

        public Parameters() 
        {
            Initialise();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------- Visualisation parameters -----------------------------//
        #region

        public bool DrawVertexNormals 
        {
            get { return _drawVertexNormals; }
            set { _drawVertexNormals = value; }
        }

        public bool DrawFaceNormals
        {
            get { return _drawFaceNormals; }
            set { _drawFaceNormals = value; }
        }

        public bool DrawNodes
        {
            get { return _drawNodes; }
            set { _drawNodes = value; }
        }

        public bool DrawLines
        {
            get { return _drawLines; }
            set { _drawLines = value; }
        }

        public bool DrawMeshWires
        {
            get { return _drawMeshWires; }
            set { _drawMeshWires = value; }
        }

        public bool DrawMeshShaded
        {
            get { return _drawMeshShades; }
            set { _drawMeshShades = value; }
        }

        public bool Draw3dBars
        {
            get { return _draw3dBars; }
            set { _draw3dBars = value; }
        }

        public bool DrawNodeRing 
        {
            get { return _drawNodeRing; }
            set { _drawNodeRing = value; }
        }

        public bool DrawNodeCS 
        {
            get { return _drawNodeCS; }
            set { _drawNodeCS = value; }
        }

        public bool DrawBarCS
        {
            get { return _drawBarCS; }
            set { _drawBarCS = value; }
        }

        public bool DrawVertexID 
        {
            get { return _drawVertexID; }
            set { _drawVertexID = value; }
        }

        public bool DrawEdgeID
        {
            get { return _drawEdgeID; }
            set { _drawEdgeID = value; }
        }

        public bool DrawFaceID
        {
            get { return _drawFaceID; }
            set { _drawFaceID = value; }
        }

        public bool DrawDebug
        {
            get { return _drawDebug; }
            set { _drawDebug = value; }
        }

        public eColorBy ColorBy 
        {
            get { return _colorBy; }
            set { _colorBy = value; }
        }

        public double ShowMin
        {
            get { return _showMin; }
            set { _showMin = value; }
        }

        public double ShowMax
        {
            get { return _showMax; }
            set { _showMax = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//



        //------------------------------ Analysis parameters -------------------------------//
        #region

        public bool IsSelfWeight 
        { 
            get{ return _isSelfWeight; } 
            set{ _isSelfWeight = value; }
        }

        public bool IsSurfaceLoad
        {
            get { return _isSurfaceLoad; }
            set { _isSurfaceLoad = value; }
        }

        public eLoadType LoadType
        {
            get { return _loadType; }
            set { _loadType = value; }
        }

        public Vector3d GravityVector
        {
            get { return _gravityVector; }
            set { _gravityVector = value; }
        }

        public double GravityMagnitude
        {
            get { return _gravityMag; }
            set { _gravityMag = value; }
        }

        public Vector3d SurfaceLoadVector
        {
            get { return _srfLoadVector; }
            set { _srfLoadVector = value; }
        }

        public double SurfaceLoadMagnitude
        {
            get { return _srfLoadMag; }
            set { _srfLoadMag = value; }
        }

        public eSizingMode SizingMode
        {
            get { return _sizingMode; }
            set { _sizingMode = value; }
        }

        public double TimeStep 
        {
            get { return _dt; }
            set { _dt = value; }
        }

        public double ViscousDamping
        {
            get { return _vd; }
            set { _vd = value; }
        }

        public double Tolerance
        {
            get { return _tol; }
            set { _tol = value; }
        }

        public eElementType ElementType 
        {
            get { return _elemType; }
            set { _elemType = value; }
        }

        public double LoadScaleFactor
        {
            get { return _loadSF; }
            set { _loadSF = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ From analysis to UI -------------------------------//
        #region

        public double OutOfBalanceForce 
        {
            get { return _obf; }
            set { _obf = value; }
        }

        public double OutOfBalanceMoment
        {
            get { return _obm; }
            set { _obm = value; }
        }

        public double ConvergenceForce
        {
            get { return _convergForce; }
            set { _convergForce = value; }
        }

        public double StructuralWeight
        {
            get { return _strucWeight; }
            set { _strucWeight = value; }
        }

        public int DRCounter
        {
            get { return _drCounter; }
            set { _drCounter = value; }
        }

        public int SizingCounter
        {
            get { return _sizingCounter; }
            set { _sizingCounter = value; }
        }

        public double RealMin 
        {
            get { return _realMin; }
            set { _realMin = value; }
        }

        public double RealMax
        {
            get { return _realMax; }
            set { _realMax = value; }
        }

        public int NrIterations
        {
            get { return _nrIterations; }
            set { _nrIterations = value; }
        }


        #endregion
        //----------------------------------------------------------------------------------//


    }
}
