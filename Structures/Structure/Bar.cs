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
    public class Bar : Element
    {
        protected CoordSystem _baseCS;                      //Initial coordinate system for the bar. 
        protected Material _material;                       //Material
        protected double[] _f;                              //Axial force vector
        protected double _N;                                //Magnitude of the force
        protected double _ea;                               //Elongation due to axial displacement
        protected double _sfEA;                             //Axial stiffness scale factor
        protected double _preStress;                        //Axial prestress.
        protected double _m;                                //The weight of the bar
        protected double _sl_Global_SF;                     //Slack length global scale factor.
        protected double _sl_Custom_SF;                     //Slack length custom scale factor.
        protected eOrientation _orientation;                //Cross section orientation
        protected double[] _loadFromMass;                   //Load caused by the weight of the beam, should be divided by 2 when applied to nodes.
        protected double[] _loadFromSurface;                //Load from surface, can be either derived from a line or an area load.
        
        protected double[] _transStiffness;                 //
        protected double _nts_Sf;                           //Node translational stiffness scale factor, smaller value means lowerd stiffness => faster convergence.

        protected double _prevCSSF;                         //Previous section scale factor                         
        protected double _CSSF;                             //Cross section scale factor



        new public void Initialise()
        {
            _baseCS = new CoordSystem();
            _material = new Steel();
            _f = new double[3];
            _N = 0;
            _ea = 0;
            _sfEA = 1;
            _preStress = 0;
            _m = 0;
            _sl_Global_SF = 1;
            _sl_Custom_SF = 1;
            _orientation = eOrientation.normal;
            _loadFromMass = new double[3] { 0, 0, 0 };
            _loadFromSurface = new double[3] { 0, 0, 0 };
            _transStiffness = new double[] { 0, 0, 0 };
            _nts_Sf = 0.1;

            _prevCSSF = 1;
        }


        //--------------------------------- Constructors -----------------------------------//
        #region

        public Bar() : base()
        {
            Initialise();
        }

        public Bar(Edge e) : base()
        {
            Initialise();
        }

        public Bar(int id) : base(id)
        {
            Initialise();
        }

        public Bar(Node3Dof nodeStart, Node3Dof nodeEnd) : base(nodeStart, nodeEnd)
        {
            Initialise();

            _crossSection = new CircularHollow();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Casting --------------------------------------//
        #region

        public new Node3Dof GetStart()
        {
            return (Node3Dof)_vertices[0];
        }

        public new Node3Dof GetEnd()
        {
            return (Node3Dof)_vertices[1];
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Properties ------------------------------------//
        #region

        public double PreStress
        {
            get { return _preStress; }
            set { _preStress = value; }
        }

        public eOrientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public CrossSection CrossSection
        {
            get { return _crossSection; }
            set { _crossSection = value; }
        }

        public Material Material
        {
            get { return _material; }
            set { _material = value; }
        }

        public double Mass
        {
            get { return _m; }
            set { _m = value; }
        }

        public double ForceMagnitude
        {
            get { return _N; }
            set { _N = value; }
        }

        public double[] LoadFromMass 
        {
            get { return _loadFromMass; }
            set { _loadFromMass = value; }
        }

        public double[] LoadFromSurface
        {
            get { return _loadFromSurface; }
            set { _loadFromSurface = value; }
        }

        public CoordSystem BaseCS
        {
            get { return _baseCS; }
            set { _baseCS = value; }
        }

        public double[] TransStiffness 
        {
            get { return _transStiffness; }
            set { _transStiffness = value; }
        }

        public double TransStiffnessScaleFactor 
        {
            get { return _nts_Sf; }
            set { _nts_Sf = value; }
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Initialise ------------------------------------//
        #region

        //The coordinate system for the bar is orientated to follow the convension used in the dissertation
        //"Stressed spline structures" by Sigrid Adriaenssens. That means the z-axis is aglined with the
        //bar itself, the y axis is the "normal" to the beam, and the x axis as the cross product 
        //between the two other axes Y x Z. So for bending the stiffer bending-axis would usually for be the x-axis
        //and twisting happens around the z-axis. 


        public override void InitialiseCoordSystem() 
        {
            if (_orientation == eOrientation.vertical)
                InitialiseVerticalCoordSystem();
            else if (_orientation == eOrientation.normal)
                InitialiseNormalCoordSystem();
        }

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

            _baseCS.UnitizeVectors();
        }

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

            _baseCS.UnitizeVectors();
        }

        public override void CalcInitialTwist()
        {
        }

        public override void InitialiseMesh() 
        {
            _crossSection.BuildMesh(this);
            _isMeshBuilt = true;
        }

        /** Calculate this bars contribution to axial stiffness
        * 
        */
        public override void CalcNodeStiffness()
        {
            CalcNodeTransStiffness();
        }

        public void CalcNodeTransStiffness()
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            //To make the code readable
            double E = _material.YoungsModulus;
            double G = _material.ShearModulus;
            double A = _crossSection.Area;

            //Axial stiffness.
            _transStiffness[0] = Math.Abs((1 / iL) * (E * A * p.X));
            _transStiffness[1] = Math.Abs((1 / iL) * (E * A * p.Y));
            _transStiffness[2] = Math.Abs((1 / iL) * (E * A * p.Z));
        }

        public override void ApplyNodeStiffness() 
        {
            GetStart().TranslationalStiffness[0] += _transStiffness[0];
            GetStart().TranslationalStiffness[1] += _transStiffness[1];
            GetStart().TranslationalStiffness[2] += _transStiffness[2];

            GetEnd().TranslationalStiffness[0] += _transStiffness[0];
            GetEnd().TranslationalStiffness[1] += _transStiffness[1];
            GetEnd().TranslationalStiffness[2] += _transStiffness[2];
        }

        


        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Calculate -------------------------------------//
        #region


        /** Calculate element deformation, which for a bar is only axial. 
        * 
        */
        public override void CalcDeformation()
        {
            CalcElongation();
        }

        public override void CalcStrain()
        {
            CalcAxialForce();
        }

        public override void ApplyStrainToNodes() 
        {
            ApplyForceToNodes();
        }

        public override void CalcElongation()
        {
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;

            //Elongation due to nodal displacement
            _ea = (Math.Pow(_length, 2) - Math.Pow(iL, 2)) / (2 * iL);
        }

        public override void ClearForce()
        {
            _f[0] = 0;
            _f[1] = 0;
            _f[2] = 0;
        }

        /** Calculate axial force 
        * 
        */
        public override void CalcAxialForce()
        {
            double iL = _sl_Custom_SF * _sl_Global_SF * _initLength;
            _N = (_sfEA * _material.YoungsModulus * _crossSection.Area / _initLength) * (_ea);

            AddPreStress();
        }

        /** Add axial prestress 
        * 
        */
        public void AddPreStress()
        {
            _N += _preStress;
        }

        public override void ApplyForceToNodes() 
        {
            Vector3d p = _vertices[1].Point - _vertices[0].Point;
            double[] _nForce = new double[3];

            p.Unitize();

            _nForce[0] = (p.X * _N);
            _nForce[1] = (p.Y * _N);
            _nForce[2] = (p.Z * _N);

            GetStart().ForceAdd(_nForce[0], _nForce[1], _nForce[2]);
            GetEnd().ForceAdd(-_nForce[0], -_nForce[1], -_nForce[2]);
        }

        public override void ApplyReleases()
        {
            //Yet to implemented
        }


        /** Sizing, not accounting for buckling.  
        * 
        */
        public override void DirectSizing() 
        {
            //Calculating scale factor but only changing to 10 % of the differance between
            //current and previous scale factor. 
            CalcScaleFactor(ref _CSSF);

            double prevPrevCSSF = _prevCSSF;
            double percentage = 0.1;
            double sfChange = _CSSF - _prevCSSF;

            _CSSF = _prevCSSF + percentage * sfChange;

            if (_CSSF > 0)
            {
                _crossSection.Scale(_CSSF);
                _prevCSSF = _CSSF;
            }
            if (_crossSection.Area < 0.0000785)                 //-> 5mm radius..
            {
                _crossSection.ApplyPreviousSection();
                _prevCSSF = prevPrevCSSF;
            }
        }

        /** Calculating cross section scale factor based on yeild strenght. 
        * 
        */
        public void CalcScaleFactor(ref double sFac)
        {
            double A = _crossSection.Area;
            double yLim = _material.YieldStrength;

            sFac = _N / (A * yLim);
        }

        public override void IncrementalSizing() 
        {
        
        }



        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------------- Mass and Load ----------------------------------//
        #region


        /** Calculate the mass for the beam. 
        * 
        */
        public override void CalcMass()
        {
            _m = _initLength * _crossSection.Area * _material.Density;
        }

        public override void CalcPointLoadFromSurface(Vector3d loadDir, double load)
        {
            loadDir.Unitize();

            _loadFromSurface[0] = load * loadDir.X;
            _loadFromSurface[1] = load * loadDir.Y;
            _loadFromSurface[2] = load * loadDir.Z;
        }

        public override void CalcLineLoadFromSurface(Vector3d loadDir, double loadPerMeter)
        {
            double val = _projLength * loadPerMeter;

            loadDir.Unitize();

            _loadFromSurface[0] = val * loadDir.X;
            _loadFromSurface[1] = val * loadDir.Y;
            _loadFromSurface[2] = val * loadDir.Z;
        }

        public override void CalcAreaLoadFromSurface(Vector3d loadDir, double loadPerSquareMeter)
        {
            double val = _projArea * loadPerSquareMeter;

            loadDir.Unitize();

            _loadFromSurface[0] = val * loadDir.X;
            _loadFromSurface[1] = val * loadDir.Y;
            _loadFromSurface[2] = val * loadDir.Z;
        }

        /** Calculate load from mass, (load divided in x,y,z componets for a generic gravity direction), load is different from mass in the sence that it allows for scaling.
        * 
        */
        public override void CalcLoadFromMass(Vector3d dirVec, double gravity, eLoadType loadType)
        {
            double val = _m * gravity;

            _loadFromMass[0] = val * dirVec.X;
            _loadFromMass[1] = val * dirVec.Y;
            _loadFromMass[2] = val * dirVec.Z;
        }

        /** Calculate load from surface 
        * 
        */
        public override void CalcLoadFromSurface(Vector3d dirVec, double magnitude, eLoadType loadType)
        {
            if(loadType == eLoadType.PointLoad)
                CalcPointLoadFromSurface(dirVec, magnitude);
            else if(loadType == eLoadType.LineLoad)
                CalcLineLoadFromSurface(dirVec, magnitude);
            else if(loadType == eLoadType.AreaLoad)
                CalcAreaLoadFromSurface(dirVec, magnitude);
        }

        /** Add calculated load from mass to the nodes. Half of the load on each node. 
        * 
        */
        public override void ApplyLoadFromMass()
        {
            GetEnd().LoadFromMass[0] += _loadFromMass[0] / 2.0;
            GetEnd().LoadFromMass[1] += _loadFromMass[1] / 2.0;
            GetEnd().LoadFromMass[2] += _loadFromMass[2] / 2.0;

            GetStart().LoadFromMass[0] += _loadFromMass[0] / 2.0;
            GetStart().LoadFromMass[1] += _loadFromMass[1] / 2.0;
            GetStart().LoadFromMass[2] += _loadFromMass[2] / 2.0;
        }

        public override void ApplyLoadFromSurface()
        {
            GetEnd().LoadFromSurface[0] += _loadFromSurface[0] / 2.0;
            GetEnd().LoadFromSurface[1] += _loadFromSurface[1] / 2.0;
            GetEnd().LoadFromSurface[2] += _loadFromSurface[2] / 2.0;

            GetStart().LoadFromSurface[0] += _loadFromSurface[0] / 2.0;
            GetStart().LoadFromSurface[1] += _loadFromSurface[1] / 2.0;
            GetStart().LoadFromSurface[2] += _loadFromSurface[2] / 2.0;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Section and Material ------------------------------//
        #region

        /** Color the mesh for the beam element. 
        * 
        */
        public override void ColorMesh(double realMin, double realMax, double showMin, double showMax)
        {
            _crossSection.ColorMesh(this, realMin, realMax, showMin, showMax);
        }

        /** Build the mesh for the beam element. 
        * 
        */
        public override void BuildMesh()
        {
            _crossSection.BuildMesh(this);
            _isMeshBuilt = true;
        }

        /** Calculate the section properties. 
        * 
        */
        public override void CalcSectionProperties()
        {
            _crossSection.CalcSectionProperties();
        }

        /** Scale section. 
        * 
        */
        public void ScaleSection(double sf)
        {
            _crossSection.Scale(sf);
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Utilities -------------------------------------//
        #region

        /** Set forces to zero. 
        * 
        */
        public void ClearCoodinateSystem()
        {
            _baseCS.Clear();
        }

        /** Set forces to zero. 
        * 
        */
        public void ClearAxialForce()
        {
            _f[0] = 0;
            _f[1] = 0;
            _f[2] = 0;
            _N = 0;
        }
        
        public override void Rewind() 
        {
            base.Rewind();
            ClearAxialForce();
            ClearCoodinateSystem();

            _crossSection.Rewind();

            InitialiseCoordSystem();
        } 

        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------------- PostProcess ------------------------------------//
        #region

        public override void PostProcess() 
        {
        
        }

        public override void CalcConvergForce() 
        {
            double lfm = Math.Sqrt(Math.Pow(_loadFromMass[0], 2) + Math.Pow(_loadFromMass[1], 2) + Math.Pow(_loadFromMass[2], 2));
            double lfs = Math.Sqrt(Math.Pow(_loadFromSurface[0], 2) + Math.Pow(_loadFromSurface[1], 2) + Math.Pow(_loadFromSurface[2], 2));

            _convergeForce = lfm + lfs;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- UI Stuff -------------------------------------//
        #region

        public virtual void SetColorBy(Parameters p)
        {
            switch (p.ColorBy)
            {
                case eColorBy.Force:
                    _colorBy[0] = _N;
                    _colorBy[1] = _N;
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
