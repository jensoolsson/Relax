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
    public class Beam : Bar
    {
        private CoordSystem _cs1;                       //Local coordinate system end 1
        private CoordSystem _cs2;                       //Local coordinate system end 2
        private double[] _rot1;                         //Rotations of end 1 around x,y,z axis.
        private double[] _rot2;                         //Rotations of end 2 around x,y,z axis.
        private double[] _iM1;                          //Internal moment around local x and y axes for end 1.
        private double[] _iM2;                          //Internal Moment around local x and y axes for end 2.
        private double[] _tM1;                          //Totla moment around local x and y axes for end 1. _tM1 = _iM1 + _M_LoadFromMass + _M_LoadFromSurface
        private double[] _tM2;                          //Internal Moment around local x and y axes for end 2.
        private double[] _V;                            //Shear force
        private double[] _U;                            //Total utilization for the two ends of the element.    
        private double[] _Ub;                           //Utilization bending for the two ends of the element.    
        private double[] _Ua;                           //Utilization axial for the two ends of the element.    
        

        private double _twist;                          //Twist angle.
        private double _initTwist;                      //Initial twist due to allready twisted member.
        private double _eb;                             //Elongation due to bowing
        private double _MT;                             //Twisting moment
        
        private double _utA;                            //Utilization axial, value between 0-100%, from the UI.
        private double _utB;                            //Utilization bendgin, value between 0-100%, from the UI.
        private double _sfEI;                           //Bending stiffness scale factor
        private double _maxAllowedM;                    //If utilized only in bending, this is the max moment it can take.
        private double _maxAllowedF;                    //If utilized only in axial force, this is the max force it can take.
        private double _maxM;                           //Max moment along the member.
        private double _avrgDisp;                        //Average displacement
        
        private double _sizingUtil;                     //The leve to which the sizing is carried out
        private double _effectiveLength;                //Effective length of beam, needed in euro code 3 equations, used here for utilization calculations
        private double _effectiveLengthScaleFactor;     //Due to difficuly to calculate actuall effective length, this scale factor was intorduced.

        private eRelease _release;

        private double[] _M_LoadFromMass;               //Moment from self weight
        private double[] _M_LoadFromSurface;            //Moment from line load
        private double _loadArea;                       //Area  from surface load
        private Line _loadLine;                         //Visualising surface load applied as line load by drawing the projected area as line.
        private Polyline _deflectedShape;               //Approximate shape of defelected member
        private Polyline _loadAreaPolyline;             //Visualising surface load applied as area load by drawing the projected area as polyline. 
        private Point3d _projectedCentroid;             //Element center projected to load surface.
        private BeamMatrix _matrix;

        private double[] _rotStiffness;                 //Bendingstiffness-contribution from this element to the neighbouring nodes.
        private double _nrs_Sf;                         //Node rotations stiffness scale factor.


        new public void Initialise()
        {
            _cs1 = new CoordSystem();
            _cs2 = new CoordSystem();
            _rot1 = new double[3] { 0, 0, 0 };
            _rot2 = new double[3] { 0, 0, 0 };
            _iM1 = new double[2] { 0, 0 };
            _iM2 = new double[2] { 0, 0 };
            _tM1 = new double[2] { 0, 0 };
            _tM2 = new double[2] { 0, 0 };

            _V = new double[2];
            _U = new double[2];
            _Ub = new double[2];
            _Ua = new double[2];

            _twist = 0;
            _initTwist = 0;
            _eb = 0;
            _N = 0;
            _MT = 0;

            _utA = 0;
            _utB = 0;
            _sfEI = 1;
            _maxAllowedM = 0;
            _maxAllowedF = 0;
            _maxM = 0;
            _avrgDisp = 0;

            _sizingUtil = 0.7;                      //Meaning sizing to 70 % of capacity
            _effectiveLength = 0;
            _effectiveLengthScaleFactor = 1;
            _release = eRelease.none;

            _loadFromMass = new double[3] { 0, 0, 0 };
            _loadFromSurface = new double[3] { 0, 0, 0 };
            _M_LoadFromMass = new double[4] { 0, 0, 0, 0 };
            _M_LoadFromSurface = new double[4] { 0, 0, 0, 0 };
            _loadArea = 0;
            _loadLine = new Line();
            _deflectedShape = new Polyline();
            _loadAreaPolyline = new Polyline();
            _projectedCentroid = new Point3d();

            _matrix = new BeamMatrix(this);

            _rotStiffness = new double[] { 0, 0, 0 };

            _nts_Sf = 1;      //Scale factor for translational stiffness.
            _nrs_Sf = 10;       //Scale factor for rotational stiffness.
        }

        //--------------------------------- Constructors -----------------------------------//
        #region

        public Beam() : base()
        {
            Initialise();
        }

        public Beam(Edge egde) : base()
        {
            Initialise();
        }

        public Beam(Node6Dof nodeStart, Node6Dof nodeEnd) : base((Node3Dof)nodeStart, (Node3Dof)nodeEnd)
        {
            Initialise();

            _crossSection = new RectangularHollow();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Properties -----------------------------------//
        #region

        public CoordSystem BaseCoordSystem
        {
            get { return _baseCS; }
            set { _baseCS = value; }
        }

        public CoordSystem CoordSystem1
        {
            get { return _cs1; }
            set { _cs1 = value; }
        }

        public CoordSystem CoordSystem2
        {
            get { return _cs2; }
            set { _cs2 = value; }
        }

        public double[] EndRotations1
        {
            get { return _rot1; }
            set { _rot1 = value; }
        }

        public double[] EndRotations2
        {
            get { return _rot2; }
            set { _rot2 = value; }
        }

        public double Twist
        {
            get { return _twist; }
            private set { _twist = value; }
        }

        public double ElongationAxial
        {
            get { return _ea; }
            private set { _ea = value; }
        }

        public double ElongationBowing
        {
            get { return _eb; }
            private set { _eb = value; }
        }

        public double NormalForce
        {
            get { return _N; }
            private set { _N = value; }
        }

        public double[] Moment1
        {
            get { return _iM1; }
            private set { _iM1 = value; }
        }

        public double[] Moment2
        {
            get { return _iM2; }
            private set { _iM2 = value; }
        }

        public double MomentTorsion
        {
            get { return _MT; }
            private set { _MT = value; }
        }

        public double[] MomentDeadLoad
        {
            get { return _M_LoadFromMass; }
            set { _M_LoadFromMass = value; }
        }

        public double[] MomentLineLoad
        {
            get { return _M_LoadFromSurface; }
            set { _M_LoadFromSurface = value; }
        }

        public double[] ShearForce
        {
            get { return _V; }
            set { _V = value; }
        }

        public double[] Utilization 
        {
            get { return _U; }
            set { _U = value; }
        }

        public double UtilizationAxial
        {
            get { return _utA; }
            set { _utA = value; }
        }

        public double UtilizationBending
        {
            get { return _utB; }
            set { _utB = value; }
        }

        public double ScaleFactorEA
        {
            get { return _sfEA; }
            set { _sfEA = value; }
        }

        public double ScaleFactorEI
        {
            get { return _sfEI; }
            set { _sfEI = value; }
        }

        public double MaxAllowedMoment 
        {
            get { return _maxAllowedM; }
            set { _maxAllowedM = value; }
        }

        public double MaxAllowedForce
        {
            get { return _maxAllowedF; }
            set { _maxAllowedF = value; }
        }

        public double MaxMoment 
        {
            get { return _maxM; }
            set { _maxM = value; }
        }

        public double SizingUtilization 
        {
            get {return _sizingUtil;}
            set {_sizingUtil = value;}
        }

        public double EffectiveLengthScaleFactor 
        {
            get { return _effectiveLengthScaleFactor; }
            set { _effectiveLengthScaleFactor = value; }
        }

        public Line LoadLine 
        {
            get { return _loadLine; }
            set { _loadLine = value; }
        }

        public Polyline DeflectedShape 
        {
            get { return _deflectedShape; }
            set { _deflectedShape = value; }
        }

        public double LoadArea 
        {
            get { return _loadArea; }
            set { _loadArea = value; }
        }

        public Polyline LoadAreaPolyline
        {
            get { return _loadAreaPolyline; }
            set { _loadAreaPolyline = value; }
        }

        public Point3d ProjectedCentroid
        {
            get { return _projectedCentroid; }
            set { _projectedCentroid = value; }
        }

        public new Node6Dof GetStart() 
        {
            return (Node6Dof)_vertices[0];
        }

        public new Node6Dof GetEnd()
        {
            return (Node6Dof)_vertices[1];
        }

        public double AverageDisplacement
        {
            get { return _avrgDisp; }
            set { _avrgDisp = value; }
        }

        public double[] NodeRotationalStiffness 
        {
            get { return _rotStiffness; }
            set { _rotStiffness = value; }
        }

        public double NodeRotationalStiffnessScaleFactor 
        {
            get { return _nrs_Sf; }
            set { _nrs_Sf = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------- Clear and reset funtions ---------------------------//
        #region


        public void ClearForce() 
        {
            _f[0] = 0;
            _f[1] = 0;
            _f[2] = 0;
        }

        /** Set rotations to zero. 
        * 
        */
        public void ClearRotations() 
        {
            _rot1[0] = 0;
            _rot1[1] = 0;
            _rot2[0] = 0;
            _rot2[1] = 0;
            _twist = 0;
            _initTwist = 0;
            _eb = 0;
        }

        /** Set moments to zero. 
        * 
        */
        public void ClearMoments() 
        {
            _iM1[0] = 0;
            _iM1[1] = 0;
            _iM2[0] = 0;
            _iM2[1] = 0;
            _MT = 0;
        }

        /** Set section force to zero. 
        * 
        */
        public void ClearSectionForce() 
        {
            _V[0] = 0;
            _V[0] = 0;
        }

        /** Clear end coordinate systems for the beam
        * 
        */
        public void ClearCoordinatSystem() 
        {
            base.ClearCoodinateSystem();
            _cs1.Clear();
            _cs2.Clear();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------- Initialisation functions ---------------------------//
        #region

        public override void InitialiseCoordSystem()
        {
            if (_orientation == eOrientation.vertical)
                InitialiseVerticalCoordSystem();
            else if (_orientation == eOrientation.normal)
                InitialiseNormalCoordSystem();
        }


        //The coordinate system for the beam is orientated to follow the convension used in the
        //dissertation "Stressed spline structures" by Sigrid Adriaenssens. That means the z-axis
        //is aglined with the beam itself, the y axis is the "normal" to the beam, and the x axis
        //as the cross product between the two other axes Y x Z. So for bending the stiffer 
        //bending-axis would usually for be the x-axis and twisting happens around the z-axis. 

        /** Initialise a coordinate system where the y-axis is orientated "sort of" vertiacally
        * 
        */
        public override void InitialiseVerticalCoordSystem()
        {
            Vector3d zVec = _vertices[1].Point - _vertices[0].Point;
            Vector3d temp = new Vector3d(0, 0, 1);

            if (zVec.X + zVec.Y == 0) //Member orientated along global z-axis.
                temp = new Vector3d(1, 0, 0);

            Vector3d xVec = Vector3d.CrossProduct(temp, zVec);
            Vector3d yVec = Vector3d.CrossProduct(zVec, xVec);

            //Set the calculated coord system.
            _baseCS.X = xVec;
            _baseCS.Y = yVec;
            _baseCS.Z = zVec;

            _cs1.X = xVec;
            _cs1.Y = yVec;
            _cs1.Z = zVec;
            _cs2.X = xVec;
            _cs2.Y = yVec;
            _cs2.Z = zVec;

            _baseCS.UnitizeVectors();
            _cs1.UnitizeVectors();
            _cs2.UnitizeVectors();
        }

        /** Initialise a coordinate system where the y-axis is orientated "sort of" normaly
        * 
        */
        public override void InitialiseNormalCoordSystem()
        {
            Vector3d avrgNormal = (_vertices[0].Normal + _vertices[1].Normal) / 2;
            avrgNormal.Unitize();
            Vector3d zVec = _vertices[1].Point - _vertices[0].Point;
            Vector3d xVec = Vector3d.CrossProduct(avrgNormal, zVec);
            Vector3d yVec = Vector3d.CrossProduct(xVec, zVec);

            //Set the calculated coord system.
            _baseCS.X = xVec;
            _baseCS.Y = yVec;
            _baseCS.Z = zVec;

            _cs1.X = xVec;
            _cs1.Y = yVec;
            _cs1.Z = zVec;
            _cs2.X = xVec;
            _cs2.Y = yVec;
            _cs2.Z = zVec;

            _baseCS.UnitizeVectors();
            _cs1.UnitizeVectors();
            _cs2.UnitizeVectors();
        }

        /** Calculates the initial twist before the relaxation has started. 
        * 
        */
        public override void CalcInitialTwist()
        {
            _initTwist = (Util.MyDotProd(_cs1.X, _cs2.Y) - Util.MyDotProd(_cs2.X, _cs1.Y)) / 2;
        }


        /** Sets the base coordinate system to be the average of the two end coordinate 
        *   systems. 
        */
        public void UpdateBaseCS()
        {
            _baseCS.X = (_cs1.X + _cs2.X) / 2;
            _baseCS.Y = (_cs1.Y + _cs2.Y) / 2;
            _baseCS.Z = (_cs1.Z + _cs2.Z) / 2;
            _baseCS.UnitizeVectors();
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Calculations ----------------------------------//
        #region

        public override void CalcDeformation() 
        {
            CalcElongation();

            CalcEndRotationAndTwist();
        }

        public override void CalcStrain()
        {
            CalcAxialForce();

            CalcMoment();
        }

        public override void ApplyStrainToNodes()
        {
            ApplyForceToNodes();

            ApplyMomentsToNodes();
        }


        /** Calculate elongation due to axial and rotational displacements.
        * 
        */
        public override void CalcElongation()
        {
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            //Elongation due to axial displacement
            _ea = (Math.Pow(_length, 2) - Math.Pow(iL, 2)) / (2 * iL);

            double x1 = _rot1[0];
            double y1 = _rot1[1];
            double x2 = _rot2[0];
            double y2 = _rot2[1];

            //Elongation due to bowing
            _eb = (iL / 60) * (x1 * (4 * x1 - x2) + x2 * (-x1 + 4 * x2)) + (iL / 60) * (y1 * (4 * y1 - y2) + y2 * (-y1 + 4 * y2));
        }

        /** Calculate rotatinal displacements based on the rotated coordinate system. 
        * 
        */
        private void CalcEndRotationAndTwist()
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;

            //According to eqn 2.35 in "Stressed spline structures" Sigrid Adriaenssens.
            _rot1[0] = Util.MyDotProd(_cs1.Y, p) / p.Length;       //Rotation around x-axis.
            _rot1[1] = -Util.MyDotProd(_cs1.X, p) / p.Length;     //Rotation around y-axis.

            _rot2[0] = Util.MyDotProd(_cs2.Y, p) / p.Length;       //Rotation around x-axis.
            _rot2[1] = -Util.MyDotProd(_cs2.X, p) / p.Length;     //Rotation around y-axis.

            //Twist angle
            //_twist = (Utilities.MyDotProd(_cs1.X, _cs2.Y) - Utilities.MyDotProd(_cs2.X, _cs1.Y)) / 2;
            double currentTwist = (Util.MyDotProd(_cs1.X, _cs2.Y) - Util.MyDotProd(_cs2.X, _cs1.Y)) / 2;
            _twist = currentTwist - _initTwist;
        }

        /** Calculate axial force in the beam, used in elastic control.
        * 
        */
        public override void CalcAxialForce()
        {
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;
            _N = (_sfEA * _material.YoungsModulus * _crossSection.Area / _initLength) * (_ea + _eb);

            AddPreStress();
        }

        /** Calculate local moment in the element at the two ends, used in elastic control
        * 
        */
        private void CalcMoment()
        {
            double x1 = _rot1[0];
            double y1 = _rot1[1];
            double x2 = _rot2[0];
            double y2 = _rot2[1];
            double E = _material.YoungsModulus;
            double Ix = _crossSection.Ix;
            double Iy = _crossSection.Iy;
            double G = _material.ShearModulus;
            double J = _crossSection.J;
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            //Moment around x-axis at end 1.
            _iM1[0] = _sfEI * (_N * iL / 30) * (4 * x1 - x2) + (_sfEI * E * Ix / iL) * (4 * x1 + 2 * x2);
            //Moment around y-axis at end 1.
            _iM1[1] = _sfEI * (_N * iL / 30) * (4 * y1 - y2) + (_sfEI * E * Iy / iL) * (4 * y1 + 2 * y2);

            //Moment around x-axis at end 2.
            _iM2[0] = _sfEI * (_N * iL / 30) * (-x1 + 4 * x2) + (_sfEI * E * Ix / iL) * (2 * x1 + 4 * x2);
            //Moment around y-axis at end 2.
            _iM2[1] = _sfEI * (_N * iL / 30) * (-y1 + 4 * y2) + (_sfEI * E * Iy / iL) * (2 * y1 + 4 * y2);

            //Twisting moment for the element
            _MT = (G * J / iL) * _twist;
        }

        /** Calculate this beams contribution to axial and bending stiffness
        * 
        */
        public override void CalcNodeStiffness()
        {
            base.CalcNodeTransStiffness();

            CalcNodeBendingStiffness();
        }

        public void CalcNodeBendingStiffness()
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            //To make the code readable
            double E = _material.YoungsModulus;
            double G = _material.ShearModulus;
            double Ix = _crossSection.Ix;
            double Iy = _crossSection.Iy;
            double J = _crossSection.J;

            //Average the end coordinate systems
            Vector3d csX = (_cs1.X + _cs2.X);
            Vector3d csY = (_cs1.Y + _cs2.Y);
            Vector3d csZ = (_cs1.Z + _cs2.Z);

            csX.Unitize();
            csY.Unitize();
            csZ.Unitize();

            //Bending stiffness
            _rotStiffness[0] = (1 / iL) * (E * Ix * Math.Abs(csX.X) + E * Iy * Math.Abs(csY.X) + G * J * Math.Abs(csZ.X));
            _rotStiffness[1] = (1 / iL) * (E * Ix * Math.Abs(csX.Y) + E * Iy * Math.Abs(csY.Y) + G * J * Math.Abs(csZ.Y));
            _rotStiffness[2] = (1 / iL) * (E * Ix * Math.Abs(csX.Z) + E * Iy * Math.Abs(csY.Z) + G * J * Math.Abs(csZ.Z));
        }

        public override void ApplyNodeStiffness() 
        {
            base.ApplyNodeStiffness();

            GetStart().RotationalStiffness[0] += _nrs_Sf * _rotStiffness[0];
            GetStart().RotationalStiffness[1] += _nrs_Sf * _rotStiffness[1];
            GetStart().RotationalStiffness[2] += _nrs_Sf * _rotStiffness[2];

            GetEnd().RotationalStiffness[0] += _nrs_Sf * _rotStiffness[0];
            GetEnd().RotationalStiffness[1] += _nrs_Sf * _rotStiffness[1];
            GetEnd().RotationalStiffness[2] += _nrs_Sf * _rotStiffness[2];
        }


        /** Set moments according to the releases
        * 
        */
        private void CalcMomentRelases(double[] rM1, double[] rM2, double rMT)
        {
            switch (_release)
            {
                case eRelease.none:
                    return;

                case eRelease.rot_X:
                    //If released for x-rot, the moment applied on the node from this beam is 0 around the x-axis is:
                    rM1[0] = 0;
                    rM2[0] = 0;
                    break;

                case eRelease.rot_Y:
                    //If released for y-rot, the moment applied on the node from this beam is 0 around the y-axis is:
                    rM1[1] = 0;
                    rM2[1] = 0;
                    break;

                case eRelease.rot_X_and_Torsion:
                    //If released for y-rot, the moment applied on the node from this beam is 0 around the y-axis is:
                    rM1[0] = 0;
                    rM2[0] = 0;
                    rMT = 0;
                    break;

                case eRelease.rot_Y_and_Torsion:
                    //If released for y-rot, the moment applied on the node from this beam is 0 around the y-axis is:
                    rM1[1] = 0;
                    rM2[1] = 0;
                    rMT = 0;
                    break;

                case eRelease.torsion:
                    rMT = 0;
                    break;
            }
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------- Apply calculation results ----------------------------//
        #region

        /** Convert axial force to nodal coordiantes.
        * 
        */
        public override void ApplyForceToNodes()
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;
            double[] _nForce = new double[3];
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            _nForce[0] = (1 / iL) * (_N * p.X + _iM1[0] * _cs1.Y.X - _iM1[1] * _cs1.X.X + _iM2[0] * _cs2.Y.X - _iM2[1] * _cs2.X.X);
            _nForce[1] = (1 / iL) * (_N * p.Y + _iM1[0] * _cs1.Y.Y - _iM1[1] * _cs1.X.Y + _iM2[0] * _cs2.Y.Y - _iM2[1] * _cs2.X.Y);
            _nForce[2] = (1 / iL) * (_N * p.Z + _iM1[0] * _cs1.Y.Z - _iM1[1] * _cs1.X.Z + _iM2[0] * _cs2.Y.Z - _iM2[1] * _cs2.X.Z);

            GetStart().ForceAdd(_nForce[0], _nForce[1], _nForce[2]);
            GetEnd().ForceAdd(-_nForce[0], -_nForce[1], -_nForce[2]);
        }

        /** Convert moments to nodal coordiantes. See eqn 2.39 in in "Stressed spline structures" Sigrid Adriaenssens for details.
        * 
        */
        private void ApplyMomentsToNodes()
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;
            double[] _nM1 = new double[3];                                  //Moment for node 1 in nodal coordinates.
            double[] _nM2 = new double[3];                                  //Moment for node 2 in nodal coordiantes.
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            //Totlat = internal + from mass + from load suface
            _tM1[0] = _iM1[0] + _M_LoadFromMass[0] + _M_LoadFromSurface[0];
            _tM2[0] = _iM2[0] + _M_LoadFromMass[1] + _M_LoadFromSurface[1];
            _tM1[1] = _iM1[1] + _M_LoadFromMass[2] + _M_LoadFromSurface[2];
            _tM2[1] = _iM2[1] + _M_LoadFromMass[3] + _M_LoadFromSurface[3];

            //Moment for node 1:
            _nM1[0] = -1 * (_tM1[0] * (p.Z * _cs1.Y.Y / iL) - _tM1[1] * (p.Z * _cs1.X.Y / iL) + _MT * ((_cs1.X.Y * _cs2.Y.Z - _cs1.Y.Y * _cs2.X.Z) / 2));
            _nM1[0] += 1 * (_tM1[0] * (p.Y * _cs1.Y.Z / iL) - _tM1[1] * (p.Y * _cs1.X.Z / iL) + _MT * ((_cs1.X.Z * _cs2.Y.Y - _cs1.Y.Z * _cs2.X.Y) / 2));
            _nM1[1] = -1 * (_tM1[0] * (p.X * _cs1.Y.Z / iL) - _tM1[1] * (p.X * _cs1.X.Z / iL) + _MT * ((_cs1.X.Z * _cs2.Y.X - _cs1.Y.Z * _cs2.X.X) / 2));
            _nM1[1] += 1 * (_tM1[0] * (p.Z * _cs1.Y.X / iL) - _tM1[1] * (p.Z * _cs1.X.X / iL) + _MT * ((_cs1.X.X * _cs2.Y.Z - _cs1.Y.X * _cs2.X.Z) / 2));
            _nM1[2] = -1 * (_tM1[0] * (p.Y * _cs1.Y.X / iL) - _tM1[1] * (p.Y * _cs1.X.X / iL) + _MT * ((_cs1.X.X * _cs2.Y.Y - _cs1.Y.X * _cs2.X.Y) / 2));
            _nM1[2] += 1 * (_tM1[0] * (p.X * _cs1.Y.Y / iL) - _tM1[1] * (p.X * _cs1.X.Y / iL) + _MT * ((_cs1.X.Y * _cs2.Y.X - _cs1.Y.Y * _cs2.X.X) / 2));

            //Moment for node 2:
            _nM2[0] = -1 * (_tM2[0] * (p.Z * _cs2.Y.Y / iL) - _tM2[1] * (p.Z * _cs2.X.Y / iL) - _MT * ((_cs1.X.Y * _cs2.Y.Z - _cs1.Y.Y * _cs2.X.Z) / 2));
            _nM2[0] += 1 * (_tM2[0] * (p.Y * _cs2.Y.Z / iL) - _tM2[1] * (p.Y * _cs2.X.Z / iL) - _MT * ((_cs1.X.Z * _cs2.Y.Y - _cs1.Y.Z * _cs2.X.Y) / 2));
            _nM2[1] = -1 * (_tM2[0] * (p.X * _cs2.Y.Z / iL) - _tM2[1] * (p.X * _cs2.X.Z / iL) - _MT * ((_cs1.X.Z * _cs2.Y.X - _cs1.Y.Z * _cs2.X.X) / 2));
            _nM2[1] += 1 * (_tM2[0] * (p.Z * _cs2.Y.X / iL) - _tM2[1] * (p.Z * _cs2.X.X / iL) - _MT * ((_cs1.X.X * _cs2.Y.Z - _cs1.Y.X * _cs2.X.Z) / 2));
            _nM2[2] = -1 * (_tM2[0] * (p.Y * _cs2.Y.X / iL) - _tM2[1] * (p.Y * _cs2.X.X / iL) - _MT * ((_cs1.X.X * _cs2.Y.Y - _cs1.Y.X * _cs2.X.Y) / 2));
            _nM2[2] += 1 * (_tM2[0] * (p.X * _cs2.Y.Y / iL) - _tM2[1] * (p.X * _cs2.X.Y / iL) - _MT * ((_cs1.X.Y * _cs2.Y.X - _cs1.Y.Y * _cs2.X.X) / 2));

            GetStart().AddMoment(_nM1[0], _nM1[1], _nM1[2]);
            GetEnd().AddMoment(_nM2[0], _nM2[1], _nM2[2]);
        }


        /** Apply release in the joint to an other element.
        * 
        */
        public override void ApplyReleases()
        {
            if (_release == eRelease.none)
                return;

            else if (_release == eRelease.rot_X || _release == eRelease.rot_X_and_Torsion)
            {
                //The only moments on the element comes from distributed loads.
                _iM1[0] = _M_LoadFromMass[0] + _M_LoadFromSurface[0];
                _iM2[0] = _M_LoadFromMass[1] + _M_LoadFromSurface[1];
            }
            else if (_release == eRelease.rot_Y || _release == eRelease.rot_Y_and_Torsion)
            {
                //The only moments on the element comes from distributed loads.
                _iM1[1] = _M_LoadFromMass[2] + _M_LoadFromSurface[2];
                _iM2[1] = _M_LoadFromMass[3] + _M_LoadFromSurface[3];
            }
            else if (_release == eRelease.rot_X_and_Torsion || _release == eRelease.rot_Y_and_Torsion || _release == eRelease.torsion)
            {
                _MT = 0;
            }
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------- Post-Calculations --------------------------------//
        #region

        public override void PostProcess()
        {
            CalcMaxUtil();

            CalcAxialAndBendUtil();
            
            CalcDeflectedShape();
        }


        /** Assuming the moment to vary with a cubic shape function. See page 183 in "Stressed spline structures" Sigrid Adriaenssens for details.
        *   Parameter interval: -1/2, u, 1/2.       
        */
        private void CalcMomentAtParameter(double u, ref double Mxx, ref double Myy)
        {
            //Calculating the curvature.
            double kxx = (_rot1[0] * (6 * u + 1) + _rot2[0] * (6 * u - 1)) / _length;
            double kyy = -(_rot1[1] * (6 * u + 1) + _rot2[1] * (6 * u - 1)) / _length;

            //And then the moments
            Mxx = -_material.YoungsModulus * _crossSection.Ix * kxx;
            Myy = -_material.YoungsModulus * _crossSection.Iy * kyy;
        }


        /** Derivative of the curvature times EI, see page 183 in "Stressed spline structures" Sigrid Adriaenssens.
        *   for the curvature equation.
        */
        private void CalcShearForce()
        {
            _V[0] = _material.YoungsModulus * _crossSection.Ix * (_rot1[0] * 6 + _rot2[0] * 6) / _length;
            _V[1] = _material.YoungsModulus * _crossSection.Iy * -(_rot1[1] * 6 + _rot2[1] * 6) / _length;
        }

        /** Calculate and return average displacements for the beam. 
        * 
        */
        private void CalcAvrgDisplacement()
        {
            double dx, dy, dz;
            Node6Dof n1 = GetStart();
            Node6Dof n2 = GetEnd();

            n1.CalcDisplacement();
            n2.CalcDisplacement();

            dx = (Math.Abs(n1.Displacement[0]) + Math.Abs(n2.Displacement[0])) / 2.0;
            dy = (Math.Abs(n1.Displacement[1]) + Math.Abs(n2.Displacement[1])) / 2.0;
            dz = (Math.Abs(n1.Displacement[2]) + Math.Abs(n2.Displacement[2])) / 2.0;

            _avrgDisp = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /** Find the max moment in the member. 
        * 
        */
        private void CalcMaxMoment()
        {
            double[] ab = new double[4];
            ab[0] = _iM1[0];
            ab[1] = _iM1[1];
            ab[2] = _iM2[0];
            ab[3] = _iM2[1];
            double max = 0;

            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(ab[i]) > max)
                    max = Math.Abs(ab[i]);
            }
            _maxM = max;
        }

        /** Calculates the moment from 100% utilization
        * 
        */
        private void CalcMaxAllowedMoment(double a, double b)
        {
            double sfacMax1 = _utB * _material.YieldStrength / (_iM1[0] * a + _iM1[1] * b);
            double sfacMax2 = _utB * _material.YieldStrength / (_iM2[0] * a + _iM2[1] * b);

            double[] tempMaxM = new double[4];
            tempMaxM[0] = sfacMax1 * _iM1[0];
            tempMaxM[1] = sfacMax1 * _iM1[1];
            tempMaxM[2] = sfacMax2 * _iM2[0];
            tempMaxM[3] = sfacMax2 * _iM2[1];

            double max = 0;
            for (int i = 0; i < 4; i++)
            {
                if (Math.Abs(tempMaxM[i]) > max)
                    max = Math.Abs(tempMaxM[i]);
            }
            _maxAllowedM = max;
        }

        /** Calculates the deflected shape of the element based on a qubic shape function 
        * 
        */
        public void CalcDeflectedShape()
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;
            Point3d r1, r2, r;
            double L = p.Length;
            int n = 10;
            Polyline tempPline = new Polyline();
            double u = -0.5;
            double uStep = 1 / ((double)(n - 1));
            Vector3d z2 = _cs1.Z;
            Vector3d z1 = _cs2.Z;

            for (int i = 0; i < n; i++)
            {
                r1 = _vertices[0].Point;
                r2 = _vertices[1].Point;
                r = (r1 + r2) / 2 + p * u + ((z1 * L * L / Util.DotProduct(z1, p)) - p) * (u - 0.5) * (u + 0.5) * (u + 0.5) + ((z2 * L * L / Util.DotProduct(z2, p)) - p) * (u - 0.5) * (u - 0.5) * (u + 0.5);
                tempPline.Add(r);
                u += uStep;
            }
            _deflectedShape = tempPline;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------------- Sizing functions -------------------------------//
        #region

        /** Sizing the member based on incremently increasing or decreasing the section scale factor.
        *   
        */
        public override void IncrementalSizing() 
        {
            double util1 = 0, util2 = 0;
            CalcTotalUtilization(ref util1, ref util2);

            if (Math.Abs(util1) >= _sizingUtil || Math.Abs(util2) >= _sizingUtil)
            {
                //Member is over utilized and has to be sized up.
                _crossSection.IncrementSize(1.05);
                _crossSection.CalcSectionProperties();
            }
            else
            {
                //Member not utilizing its full capacity and could potentially be sized down but the new 
                //size has to be checked before its applied such that it does not lead to over utilization.
                _crossSection.IncrementSize(0.95);
                _crossSection.CalcSectionProperties();
                util1 = 0;
                util2 = 0;
                CalcTotalUtilization(ref util1, ref util2);

                if (Math.Abs(util1) >= 1 || Math.Abs(util2) >= 1)
                { 
                    //Over utilized so go back to previous size.
                    _crossSection.ApplyPreviousSection();
                }
            }

            //Increase the capacity based on the over size scale factor from the UI to cope with global buckling!
            //double sFac = Math.Pow(1 + _utilReduction, (1.0 / 3.0));
            //_section.IncrementSize(sFac);
            //_section.CalcSectionProperties();
        }

        /** Calculates a section scale factor that will directly give close to 100% utilization.
        *   
        */
        public override void DirectSizing()
        {   
            //Calculate the current max utilization
            double maxUtil = Math.Abs(CalcMaxUtil());

            //Based on that calculation guess a value for the section scalefactor.
            double sfGuess = Math.Pow(maxUtil, (1.0 / 3.0));

            //Based on the guess recursively narrow down the section scalefactor until the value is
            //sufficiently close to _sizeUtil set from the UI.
            int counter = 0;

            RecursiveCalcScaleFactor(sfGuess, maxUtil, ref counter);

            CheckLowerLimit(0.001);
        }

        /** Recursive function used to calculate the scale factor needed to find the size that gives a certain level of utilization.
        *   
        */
        private void RecursiveCalcScaleFactor(double sFac, double maxUtil, ref int counter)
        {
            if (Math.Abs(Math.Abs(maxUtil) - _sizingUtil) > 0.01 && counter < 20 && sFac > 0)
            {
                _crossSection.IncrementSize(sFac);
                _crossSection.CalcSectionProperties();

                maxUtil = CalcMaxUtil();
                sFac = Math.Pow(Math.Abs(maxUtil) + (1 - _sizingUtil), (1.0 / 3.0));
                counter++;
                RecursiveCalcScaleFactor(sFac, maxUtil, ref counter);
            }
        }

        /** Recursive function used to calculate the scale factor needed to find the size that gives a certain level of utilization.
        *   
        */
        private void CheckLowerLimit(double limitArea)
        {
            double sFac = 0;
            if (_crossSection.Area < limitArea)
            {
                sFac = limitArea / _crossSection.Area;
                _crossSection.IncrementSize(sFac);
            }
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Utilization ----------------------------------//
        #region

        /** Calls functions to calculates the maximum utilization.
        *   
        */
        public double CalcMaxUtil() 
        {
            double util1 = 0;
            double util2 = 0;
            CalcTotalUtilization(ref util1, ref util2);
            _U[0] = Math.Abs(util1);
            _U[1] = Math.Abs(util2);
            return Util.LargestAbsValue(util1, util2);   
        }

        public void CalcAxialAndBendUtil()
        {
            double axUtil = 0;
            double mUtil1 = 0;
            double mUtil2 = 0;
            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);
            _Ub[0] = mUtil1;
            _Ub[1] = mUtil2;
            _Ua[0] = axUtil;
            _Ua[1] = axUtil;
        }

        /** Calculates the bending utilization of a member due to axial force and moments.
        *   
        */
        public void CalcBendUtil()
        {
            double axUtil = 0;
            double mUtil1 = 0;
            double mUtil2 = 0;
            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);
            _U[0] = mUtil1;
            _U[1] = mUtil2;
        }

        /** Calculates the axial utilization of a member due to axial force and moments.
        *   
        */
        public void CalcAxialUtil()
        {
            double axUtil = 0;
            double mUtil1 = 0;
            double mUtil2 = 0;
            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);
            _U[0] = axUtil;
            _U[1] = axUtil;
        }

        /** Calculates the utilization of a member due to axial force and moments.
        *   
        */
        public void CalcSeparateUtilization(ref double axUtil, ref double mUtil1, ref double mUtil2)
        {
            double Nrk, Mxrk, Myrk;
            double Wx, Wy;
            double Xx = 0, Xy = 0, XLT = 0;
            double Ned, Mxed1, Mxed2, dMxed, Myed1, Myed2, dMyed;
            double gammaM1 = 1;
            double x = 0;
            double y = 0;
            _crossSection.GetXY(ref x, ref y);

            //Set to be the axail maximum capacity.
            Nrk = _material.YieldStrength * _crossSection.Area;

            //Calculating the bending strength.
            Wx = _crossSection.Ix / y;
            Wy = _crossSection.Iy / x;

            //Calculating moment resistance
            Mxrk = _material.YieldStrength * Wx;
            Myrk = _material.YieldStrength * Wy;

            //Not applying any safty factor 
            Ned = Math.Abs(_N);
            Mxed1 = Math.Abs(_iM1[0]);
            Mxed2 = Math.Abs(_iM2[0]);
            Myed1 = Math.Abs(_iM1[1]);
            Myed2 = Math.Abs(_iM2[1]);

            //These are non zero only for sections in calss 4, ok assumtion?
            dMxed = 0;
            dMyed = 0;

            double ux, uy, u1x, u1y, u2x, u2y;

            if (_N > 0) //Tension, capacity not reduced due to buckling
            {
                ux = Ned / (Nrk / gammaM1);
                uy = ux;
            }
            else        //Compression, buckling reduction factor in Xx, and Xy.
            {
                CalcBucklingReductionFactors(ref Xx, ref Xy, ref XLT);
                ux = Ned / (Xx * Nrk / gammaM1);
                uy = Ned / (Xy * Nrk / gammaM1);
            }
            //Interaction factors kxx,kyy,kxy,kyx are NOT used since axial and bening util are separated.
            axUtil = Util.LargestValue(ux, uy);
            u1x = (Mxed1 + dMxed) / (Mxrk / gammaM1);
            u1y = (Myed1 + dMyed) / (Myrk / gammaM1);
            u2x = (Mxed2 + dMxed) / (Mxrk / gammaM1);
            u2y = (Myed2 + dMyed) / (Myrk / gammaM1);

            //Summarize util for each bending direction at each end of the beam.
            mUtil1 = u1x + u1y;
            mUtil2 = u2x + u2y;
        }

        /** Calculates the utilization of a member due to axial force and moments.
        *   
        */
        private void CalcTotalUtilization(ref double util1, ref double util2)
        {
            double Nrk, Mxrk, Myrk;
            double Wx, Wy;
            double Xx = 0, Xy = 0, XLT = 0;
            double kxx, kxy, kyx, kyy;
            double Ned, Mxed1, Mxed2, dMxed, Myed1, Myed2, dMyed;
            double gammaM1 = 1;
            double x = 0;
            double y = 0;
            _crossSection.GetXY(ref x, ref y);

            //Set to be the axail maximum capacity.
            Nrk = _material.YieldStrength * _crossSection.Area;

            //Calculating the bending resistance.
            Wx = _crossSection.Ix / y;
            Wy = _crossSection.Iy / x;
            Mxrk = _material.YieldStrength * Wx;
            Myrk = _material.YieldStrength * Wy;

            CalcBucklingReductionFactors(ref Xx, ref Xy, ref XLT);

            //For simplicity setting the interaction factors to 1, seems to be a pain to calculate!
            kxx = 1;
            kxy = 1;
            kyx = 1;
            kyy = 1;

            //Not applying any safty factor 
            Ned = Math.Abs(_N);
            Mxed1 = Math.Abs(_iM1[0]);
            Mxed2 = Math.Abs(_iM2[0]);
            Myed1 = Math.Abs(_iM1[1]);
            Myed2 = Math.Abs(_iM2[1]);

            dMxed = 0;      //These are non zero only for sections in class 4.
            dMyed = 0;

            //Util1 = utilization of member at end1, Util2 = utilization of member at end2.
            double util1x, util1y, util2x, util2y;

            if (_N > 0) //Tension, capacity not reduced due to buckling
            {
                util1x = Ned / (Nrk / gammaM1) + kxx * (Mxed1 + dMxed) / (XLT * Mxrk / gammaM1) + kxy * (Myed1 + dMyed) / (Myrk / gammaM1);
                util1y = Ned / (Nrk / gammaM1) + kyx * (Mxed1 + dMxed) / (XLT * Mxrk / gammaM1) + kyy * (Myed1 + dMyed) / (Myrk / gammaM1);
                util2x = Ned / (Nrk / gammaM1) + kxx * (Mxed2 + dMxed) / (XLT * Mxrk / gammaM1) + kxy * (Myed2 + dMyed) / (Myrk / gammaM1);
                util2y = Ned / (Nrk / gammaM1) + kyx * (Mxed2 + dMxed) / (XLT * Mxrk / gammaM1) + kyy * (Myed2 + dMyed) / (Myrk / gammaM1);
            }
            else        //Compression, buckling reduction factor in Xx, and Xy.
            {
                util1x = Ned / (Xx * Nrk / gammaM1) + kxx * (Mxed1 + dMxed) / (XLT * Mxrk / gammaM1) + kxy * (Myed1 + dMyed) / (Myrk / gammaM1);
                util1y = Ned / (Xy * Nrk / gammaM1) + kyx * (Mxed1 + dMxed) / (XLT * Mxrk / gammaM1) + kyy * (Myed1 + dMyed) / (Myrk / gammaM1);
                util2x = Ned / (Xx * Nrk / gammaM1) + kxx * (Mxed2 + dMxed) / (XLT * Mxrk / gammaM1) + kxy * (Myed2 + dMyed) / (Myrk / gammaM1);
                util2y = Ned / (Xy * Nrk / gammaM1) + kyx * (Mxed2 + dMxed) / (XLT * Mxrk / gammaM1) + kyy * (Myed2 + dMyed) / (Myrk / gammaM1);
            }
            util1 = Util.LargestValue(Math.Abs(util1x), Math.Abs(util1y));
            util2 = Util.LargestValue(Math.Abs(util2x), Math.Abs(util2y));
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------------- Eurocode 3.0 stuff ------------------------------//
        #region

        /** Calculating buckling reduction factors according to Euro code 3.
        *   
        */
        private void CalcBucklingReductionFactors(ref double Xx, ref double Xy, ref double XLT) 
        {
            double a = 0.49;    //Is an imperfection factor, chosen conservatively based on buckling curve c.
            double lambda;      //Non-dimensional slenderness.
            double phi;         //Some other shit-factor.
            double Ncr;         //Is the elastic critical force for the relevant buckling mode based on the gross cross sectional properties.

            CalcEffectiveLength();

            double A = _crossSection.Area;
            double fy = _material.YieldStrength;
            double E = _material.YoungsModulus;
            double Leff = _effectiveLength;
            double I;

            //Calculation for Xx
            I = _crossSection.Ix;
            Ncr = Math.PI * Math.PI * E * I / (Leff * Leff);
            lambda = Math.Sqrt(A * fy / Ncr);
            phi = 0.5 * (1 + a * (lambda - 0.2) + lambda * lambda);
            Xx = 1 / (phi + Math.Sqrt(phi*phi - lambda*lambda));
            if(Xx > 1)
                Xx = 1;

            //Calculation for Xy
            I = _crossSection.Iy;
            Ncr = Math.PI * Math.PI * E * I / (Leff * Leff);
            lambda = Math.Sqrt(A * fy / Ncr);
            phi = 0.5 * (1 + a * (lambda - 0.2) + lambda * lambda);
            Xy = 1 / (phi + Math.Sqrt(phi*phi - lambda*lambda));
            if(Xy > 1)
                Xy = 1;

            //For simplicity XLT is just set to 1.
            XLT = 1;

        }

        /** Calculating effective length of member, TODO: Take into account for different releases i.e. buckling modes.
        *   
        */
        private void CalcEffectiveLength() 
        {   
            _effectiveLength = _effectiveLengthScaleFactor * _initLength;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------------- Loading ------------------------------------//
        #region

        /** Calculate load from surface 
        * 
        */
        public override void CalcLoadFromSurface(Vector3d dirVec, double gravity, eLoadType loadType)
        {
            base.CalcLoadFromSurface(dirVec, gravity, loadType);
        
            //if(loadType == Structures.eLoadType.LineLoad)
            //    CalcLineLoadFromSurface(double projLength, Vector3d loadDir, double loadPerMeter)
            //else if (loadType == Structures.eLoadType.AreaLoad)
            //    CalcAreaLoadFromSurface(double projLength, Vector3d loadDir, double loadPerMeter)
        }

        public override void CalcPointLoadFromSurface(Vector3d loadDir, double magnitude)
        {
            base.CalcPointLoadFromSurface( loadDir, magnitude);
        }

        public override void CalcLineLoadFromSurface(double projLength, Vector3d loadDir, double loadPerMeter)
        {
            base.CalcLineLoadFromSurface(projLength, loadDir, loadPerMeter);
         
            _M_LoadFromSurface = CalcMomentsFromDistributedLoad(loadDir, loadPerMeter);
        }

        public override void CalcAreaLoadFromSurface(double area, Vector3d loadDir, double loadPerSquareMeter)
        {
            base.CalcAreaLoadFromSurface(area, loadDir, loadPerSquareMeter);

            double loadPerMeter = (loadPerSquareMeter * area / _initLength);

            _M_LoadFromSurface = CalcMomentsFromDistributedLoad(loadDir, loadPerMeter);
        }

        /** Calculate load from mass (i.e. self weight).
        * 
        */
        public override void CalcLoadFromMass(Vector3d dirVec, double gravity, eLoadType loadType)
        {
            base.CalcLoadFromMass(dirVec, gravity, loadType);

            double loadPerMeter = (_loadFromMass[0] + _loadFromMass[1] + _loadFromMass[2]) / _initLength;      //Load N/m

            if (loadType == eLoadType.LineLoad)
                _M_LoadFromMass = CalcMomentsFromDistributedLoad(dirVec, loadPerMeter);
            else
            {
                _M_LoadFromMass[0] = 0;
                _M_LoadFromMass[1] = 0;
                _M_LoadFromMass[2] = 0;
                _M_LoadFromMass[3] = 0;
            }
        }

        /** Calculate moments caused by a line load on the element.
        * 
        */
        private double[] CalcMomentsFromDistributedLoad(Vector3d loadDir, double loadPerMeter)
        {
            //Calculate the component of the load in the local x and y direction of the beam.
            //Dot product
            double xComp = _baseCS.X.X * loadDir.X + _baseCS.X.Y * loadDir.Y + _baseCS.X.Z * loadDir.Z;
            double yComp = _baseCS.Y.X * loadDir.X + _baseCS.Y.Y * loadDir.Y + _baseCS.Y.Z * loadDir.Z;
            double zComp = _baseCS.Z.X * loadDir.X + _baseCS.Z.Y * loadDir.Y + _baseCS.Z.Z * loadDir.Z;

            //LoadMag is N/m hence the multlipication with the length.
            double yLoad = yComp * loadPerMeter;
            double xLoad = xComp * loadPerMeter;

            double z0 = 0;
            double zL = _initLength;
            double L = _initLength;
            double m1x = 0, m2x = 0, m1y = 0, m2y = 0;
            double[] moment = new double[4] { 0, 0, 0, 0 };

            //The y-component of the load causes rotations around the x-axis and vice versa.
            if (_release == eRelease.none)
            {
                m1x = yLoad * ((z0 * z0) / 2.0 - (L * z0 / 2.0) + (L * L) / 12.0);
                m2x = yLoad * ((zL * zL) / 2.0 - (L * zL / 2.0) + (L * L) / 12.0);
                m1y = xLoad * ((z0 * z0) / 2.0 - (L * z0 / 2.0) + (L * L) / 12.0);
                m2y = xLoad * ((zL * zL) / 2.0 - (L * zL / 2.0) + (L * L) / 12.0);
            }
            else if (_release == eRelease.rot_X || _release == eRelease.rot_X_and_Torsion)
            {
                m1y = xLoad * ((z0 * z0) / 2.0 - (L * z0 / 2.0) + (L * L) / 12.0);
                m2y = xLoad * ((zL * zL) / 2.0 - (L * zL / 2.0) + (L * L) / 12.0);
            }
            else if (_release == eRelease.rot_Y || _release == eRelease.rot_Y_and_Torsion)
            {
                m1x = yLoad * ((z0 * z0) / 2.0 - (L * z0 / 2.0) + (L * L) / 12.0);
                m2x = yLoad * ((zL * zL) / 2.0 - (L * zL / 2.0) + (L * L) / 12.0);
            }

            moment[0] = m1x;
            moment[1] = -m2x;
            moment[2] = m1y;
            moment[3] = -m2y;

            //moment[0] = -m1x;
            //moment[1] = m2x;
            //moment[2] = -m1y;
            //moment[3] = m2y;

            return moment;
        }


        /** Calculate shear force from the distributed load.
        * 
        */
        private double[] CalcShearForceFromDistrubutedLoad(double projLength, Vector3d loadDir, double loadMag) 
        {
            //Calculate the component of the load in the local x and y direction of the beam.
            //Dot product
            double yComp = _baseCS.Y.X * loadDir.X + _baseCS.Y.Y * loadDir.Y + _baseCS.Y.Z * loadDir.Z;
            double xComp = _baseCS.X.X * loadDir.X + _baseCS.X.Y * loadDir.Y + _baseCS.X.Z * loadDir.Z;

            //LoadMag is N/m hence the multlipication with the length.
            double yLoad = yComp * loadMag * projLength;
            double xLoad = xComp * loadMag * projLength;

            double x0 = 0;
            double xL = _initLength;
            double L = _initLength;
            double v1x, v2x, v1y, v2y;
            double[] shearForce = new double[4] { 0, 0, 0, 0 };

            //Not sure about the shear force...
            v1x = -xLoad * (x0 - L / 2);
            v2x = xLoad * (x0 - L / 2);
            v1y = -yLoad * (x0 - L / 2);
            v2y = yLoad * (x0 - L / 2);

            shearForce[0] = v1x;
            shearForce[1] = v2x;
            shearForce[2] = v1y;
            shearForce[3] = v2y;

            return shearForce;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------Plastic control funtcions---------------------------//
        #region

        /** Scale forces and moments for separate utilization, axial and bending. Used in plastic analysis custom control.
        * 
        */
        private void ScaleMomentAndForcePlastic(double ua, double ub)
        {
            ScaleForceFromUtilization(ua);
            ScaleMomentFromUtilization(ub);

            double u1 = 0, u2 = 0;
            CalcTotalUtilization(ref u1, ref u2);
        }

        /** Scale forces and moments to obtain 100% utilization. Used in plastic control when custom control is deactivated.
        * 
        */
        private void ScaleMomentAndForcePlastic()
        {
            double u1 = 0, u2 = 0;
            CalcTotalUtilization(ref u1, ref u2);
            double uMax = Util.LargestValue(u1, u2);
            double sFac = 1 / uMax;

            double axUtil = 0, mUtil1 = 0, mUtil2 = 0;
            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);


            if (uMax > 1)
            {
                //Scale forces and moments... Ignoring the twist, since it is ignored in the utilization calculations.
                _N = sFac * _N;
                _iM1[0] = sFac * _iM1[0];
                _iM1[1] = sFac * _iM1[1];
                _iM2[0] = sFac * _iM2[0];
                _iM2[1] = sFac * _iM2[1];
            }
        }


        /** Recursive function used to scale the forces and moments
        *   
        */
        private void RecursiveScaleForceMoment(ref double sFac, ref double util, double maxUtil, ref int counter)
        {
            if (Math.Abs(util - maxUtil) > 0.01 && counter < 20)
            {
                sFac = maxUtil / util;

                _N = sFac * _N;
                _iM1[0] = sFac * _iM1[0];
                _iM1[1] = sFac * _iM1[1];
                _iM2[0] = sFac * _iM2[0];
                _iM2[1] = sFac * _iM2[1];

                double u1 = 0, u2 = 0;
                CalcTotalUtilization(ref u1, ref u2);
                double uMax = Util.LargestValue(u1, u2);

                counter++;
                RecursiveScaleForceMoment(ref sFac, ref uMax, maxUtil, ref counter);
            }
        }

        /** Calculates and scales the moments based on (from UI) chosen bending utilization
        * 
        */
        private void ScaleMomentFromUtilization(double u)
        {
            _utB = u;
            double axUtil = 0, mUtil1 = 0, mUtil2 = 0;
            double sFac;

            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);

            double maxUtil = Util.LargestValue(mUtil1, mUtil2);

            //Bending utilization
            if (maxUtil > _utB)
            {
                sFac = _utB / (maxUtil);
                _iM1[0] = sFac * _iM1[0];
                _iM1[1] = sFac * _iM1[1];
                _iM2[0] = sFac * _iM2[0];
                _iM2[1] = sFac * _iM2[1];
            }
        }


        /** Calculates and scales the force based on (from UI) chosen axial utilization
        * 
        */
        private void ScaleForceFromUtilization(double u)
        {
            _utA = u;
            double axUtil = 0, mUtil1 = 0, mUtil2 = 0;
            double sFac;

            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);

            //Axial utilization
            if (axUtil > _utA)
            {
                sFac = _utA / axUtil;
                _N = sFac * _N;
            }
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------- Force/Moment control funtcions -----------------------//
        #region

        /** Scale forces and moments for separate utilization, axial and bending. 
        * Used in plastic analysis with custom control.
        * 
        */
        private void ScaleMomentAndForce(double ua, double ub)
        {
            double axUtil = 0, mUtil1 = 0, mUtil2 = 0;
            double sFac;

            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);
            double maxBendUtil = Util.LargestValue(mUtil1, mUtil2);

            //Scale the axial force such that it full fills the chosen level of axial utilization.
            sFac = ua / axUtil;
            _N = sFac * _N;

            //Scale the moment such that the chosen level of bending utilization is obtained.
            sFac = ub / maxBendUtil;
            _iM1[0] = sFac * _iM1[0];
            _iM1[1] = sFac * _iM1[1];
            _iM2[0] = sFac * _iM2[0];
            _iM2[1] = sFac * _iM2[1];

            double u1 = 0, u2 = 0;
            CalcTotalUtilization(ref u1, ref u2);
        }

        /** Scale forces and moments for separate utilization, axial and bending. 
        * Used in plastic analysis where custom control is turned OFF.
        * 
        */
        private void ScaleMomentAndForce()
        {
            double u1 = 0, u2 = 0;
            CalcTotalUtilization(ref u1, ref u2);
            double uMax = Util.LargestValue(u1, u2);
            double sFac = 1 / uMax;

            double axUtil = 0, mUtil1 = 0, mUtil2 = 0;
            CalcSeparateUtilization(ref axUtil, ref mUtil1, ref mUtil2);

            //Scale forces and moments.
            _N = sFac * _N;
            _iM1[0] = sFac * _iM1[0];
            _iM1[1] = sFac * _iM1[1];
            _iM2[0] = sFac * _iM2[0];
            _iM2[1] = sFac * _iM2[1];
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------------------------------------------------------//
        #region

        public override void SetColorBy(Parameters p)
        {
            switch (p.ColorBy)
            {
                case eColorBy.Force:
                    _colorBy[0] = _N;
                    _colorBy[1] = _N;
                    break;

                case eColorBy.Utilisation:
                    _colorBy[0] = _U[0];
                    _colorBy[1] = _U[1];
                    break;

                case eColorBy.UtilisationBending:
                    _colorBy[0] = _Ub[0];
                    _colorBy[1] = _Ub[1];
                    break;

                case eColorBy.UtilisationAxial:
                    _colorBy[0] = _Ua[0];
                    _colorBy[1] = _Ua[1];
                    break;

                case eColorBy.Moment_X:
                    _colorBy[0] = _tM1[0];
                    _colorBy[1] = _tM2[0];
                    break;

                case eColorBy.Moment_Y:
                    _colorBy[0] = _tM1[1];
                    _colorBy[1] = _tM2[1];
                    break;

                case eColorBy.Twist:
                    _colorBy[0] = _twist;
                    _colorBy[1] = _twist;
                    break;

                default:
                    _colorBy[0] = 0;
                    _colorBy[1] = 0;
                    break;
            }
        }

        #endregion
        //----------------------------------------------------------------------------------//

    }
}
