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
    public class Node3Dof : Node
    {
        //Attributes
        protected double[] _x1;                         //New coordinates i.e. x1 = x(t + dt) = x(t) + v*dt 
        protected double[] _move;                       //Movement for the next timestep before applied to the nodes
        protected double[] _v;                          //Velocity      
        protected double[] _a;                          //Acceleration
        protected double[] _iF;                         //Internal force (from bars, cables and beams)
        protected double[] _tF;                         //Total force = internal force + load from mass + additional load.
        
        protected double[] _sbF;                        //Shear bending force (Spline bar specific)
        protected double[] _stF;                        //Shear twisting force (Spline bar specific)
        
        protected double[] _af;                         //Attraction force from external source
        protected double[] _if;                         //Inflation foce (along normal to vertex)
        protected double[] _d;                          //Displacement
        protected double[] _dampLoc;                    //Local damping                                                       
        protected double _dMag;                         //Displacement
        protected double _vMag;                         //Velocity magnitude
        protected double _fMag;                         //Force magnitude.

        protected double _m;                            //Lumped mass based on the real weight of the structure.
        protected double[] _mf;                         //Fictous mass used for faster convergence in form finding.                                 
        
        protected double _af_angle;                     //The angle between the attraction vector and the node normal.
        protected double _moveDist;                     //Distance the node has moved since last iteration

        protected double _loadSF;                         //Load scale factor
        protected double[] _loadFromSurface;              //Load from surface
        protected double[] _loadFromMass;                 //Load from weight of the structure
        protected double _loadArea;                       //Load area                    

        protected double _stabilityLimitTrans;
        protected double[] _prevForce;                    //Previous force

        protected double[] _transStiffness;


        private new void Initialise()
        {
            _x1 = new double[3] { 0.0, 0.0, 0.0 };
            _move = new double[3] { 0.0, 0.0, 0.0 };
            _v = new double[3] { 0.0, 0.0, 0.0 };
            _a = new double[3] { 0.0, 0.0, 0.0 };
            _iF = new double[3] { 0.0, 0.0, 0.0 };
            _tF = new double[3] { 0.0, 0.0, 0.0 };
            _sbF = new double[3] { 0.0, 0.0, 0.0 };
            _stF = new double[3] { 0.0, 0.0, 0.0 };
            _af = new double[3] { 0.0, 0.0, 0.0 };
            _if = new double[3] { 0.0, 0.0, 0.0 };
            _d = new double[3] { 0.0, 0.0, 0.0 };
            _dampLoc = new double[3] { 0.0, 0.0, 0.0 };

            _dMag = 0;
            _vMag = 0;
            _fMag = 0;
            _m = 0;
            _mf = new double[3] {0,0,0};
            _constraint = eConstraint.Free;
            _af_angle = 0;
            _moveDist = 0;

            _loadSF = 1;
            _loadFromSurface = new double[3] { 0, 0, 0 };
            _loadFromMass = new double[3] { 0, 0, 0 };

            _stabilityLimitTrans = 0;

            _transStiffness = new double[3] { 0, 0, 0 };
        }

        
        //---------------------------------- Constructors ----------------------------------//
        #region

        public Node3Dof()
        {
            Initialise();
        }

        public Node3Dof(Point3d pt) : base(pt)
        {
            Initialise();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------------ Casting -------------------------------------//
        #region

        //These functions set multiple objects at once and need casting because they are lists.
        
        /* Get topology functions */
        public void SetRingFaces(List<Plate> plates)
        {
        //    base.SetRingFaces(Util.CastToSuper<Face, Plate>(plates));
        }

        public void SetRingEdges(List<Bar> bars)
        {
        //    base.SetRingEdges(Util.CastToSuper<Edge, Bar>(bars));
        }

        public void SetRingVertices(List<Node3Dof> nodes)
        {
        //    base.SetRingVertices(Util.CastToSuper<Vertex, Node3Dof>(nodes));
        }
        

        /* Get topology functions */
        public new List<Bar> GetEdges()
        {
            return Util.CastToSubclass<Bar, Edge>(_edges);
        }

        public List<Plate> GetFaces()
        {
            return Util.CastToSubclass<Plate, Face>(_faces);
        }

        public List<Node3Dof> GetVertices()
        {
            return Util.CastToSubclass<Node3Dof, Vertex>(_vertices);
        }

        public List<Bar> GetBars() 
        {
            return Util.CastToSubclass<Bar, Edge>(_edges);
        }

        public List<Beam> GetBeams()
        {
            return Util.CastToSubclass<Beam, Edge>(_edges);
        }

        
        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------- Properties and add methods----------------------------//
        #region

        public double VelocityMagnitude
        {
            get { return _vMag; }
            set { _vMag = value; }
        }

        public double[] LocalDamping 
        {
            get { return _dampLoc; }
            set { _dampLoc = value; }
        }

        public double[] Velocity 
        {
            get { return _v; }
            set 
            { 
                _v[0] = value[0];
                _v[1] = value[1];
                _v[2] = value[2];
                _vMag = Math.Sqrt(_v[0] * _v[0] + _v[1] * _v[1] + _v[2] * _v[2]);
            }
        }

        public double[] Acceleration
        {
            get { return _a; }
            set
            {
                _a[0] = value[0];
                _a[1] = value[1];
                _a[2] = value[2];
            }
        }
        
        public double Mass
        {
            get { return _m; }
            set { _m = value; }
        }

        public void AddMass(double m)
        {
            _m += m;
        }

        public double[] MassFictitious
        {
            get { return _mf; }
            set { _mf = value; }
        }


        public double MoveDist
        {
            get { return _moveDist; }
            set { _moveDist = value; }
        }

        public double ForceMagnitude 
        {
            get { return _fMag; }
            set { _fMag = value; }
        }

        public double[] Force
        {
            get { return _tF; }
            set
            {
                _tF[0] = value[0];
                _tF[1] = value[1];
                _tF[2] = value[2];
            }
        }

        public double[] ShearBendForce
        {
            get { return _sbF; }
            set
            {
                _sbF[0] = value[0];
                _sbF[1] = value[1];
                _sbF[2] = value[2];
            }
        }

        public double[] ShearTwistForce
        {
            get { return _stF; }
            set
            {
                _stF[0] = value[0];
                _stF[1] = value[1];
                _stF[2] = value[2];
            }
        }

        public double[] AttractorForce 
        {
            get { return _af; }
            set 
            {
                _af[0] = value[0];
                _af[1] = value[1]; 
                _af[2] = value[2];
            }
        }

        public double[] InflationForce
        {
            get { return _if; }
            set
            {
                _if[0] = value[0];
                _if[1] = value[1];
                _if[2] = value[2];
            }
        }

        public double AttractionForceAngle
        {
            get { return _af_angle; }
            set { _af_angle = value; }
        }

        public double DisplacementMagnitude
        {
            get { return _dMag; }
            set { _dMag = value; }
        }

        public double[] Displacement
        {
            get { return _d; }
            set { _d = value; }
        }

        public double[] LoadFromSurface
        {
            get { return _loadFromSurface; }
            set { _loadFromSurface = value; }
        }

        public double[] LoadFromMass
        {
            get { return _loadFromMass; }
            set { _loadFromMass = value; }
        }

        public double LoadArea
        {
            get { return _loadArea; }
            set { _loadArea = value; }
        }

        public double LoadScaleFactor 
        {
            get { return _loadSF; }
            set { _loadSF = value; }
        }

        public double[] TranslationalStiffness 
        {
            get { return _transStiffness; }
            set { _transStiffness = value; }
        }

        public double StabilityLimitTranslations 
        {
            get { return _stabilityLimitTrans; }
            set { _stabilityLimitTrans = value; }
        }

        /** Adds force to the existing force
        * 
        */
        public void ForceAdd(double fx, double fy, double fz)
        {
            _iF[0] += fx;
            _iF[1] += fy;
            _iF[2] += fz;
        }

        /** Adds mass to the existing mass
        * 
        */
        public void MassAdd(double mass)
        {
            _m += mass;
        }

        /** Adds inflation force to the existing node force
        * 
        */
        public void InflationForceApply()
        {
            for(int i = 0; i < 3; i++)
                _iF[i] += _if[i];
        }

        /** Adds attractor force to the existing node force
        * 
        */
        public void AttractorForceApply()
        {
            for(int i = 0; i < 3; i++)
                _iF[i] += _af[i];
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Calculate ------------------------------------//
        #region

        /** Calculate the mass and moment of inertia for faster convergence. 
        *   
        */
        public override void CalcFictiousMass(double dt, double transStiffSF, double rotStiffSF)
        {
            CalcFictiousMassInternal(dt, transStiffSF);
        }

        /** Calculate the fictional mass based on axial and rotational stiffness. 
        *   
        */
        private void CalcFictiousMassInternal(double dt, double alpha)
        {
            //double sf = 0.01;
            //double sf = GetBars()[0].TransStiffnessScaleFactor;

            //Mass in x-dir is based on the axial stiffness in the x-dir and the bending stiffness
            //in the plane where the x-dir is the normal (yz plane).
            _mf[0] = 1 + Math.Pow(dt, 2) * (alpha * _transStiffness[0]);
            _mf[1] = 1 + Math.Pow(dt, 2) * (alpha * _transStiffness[1]);
            _mf[2] = 1 + Math.Pow(dt, 2) * (alpha * _transStiffness[2]);
        }

        /** Calculating the inflation force for the mNodes based on the inflation slider bar value 
        *          
        */
        public void CalcInflationForce(double totArea, double inflationForceValue)
        {
            double[] vec = new double[3];

            //Hardcoded values!!
            vec[0] = Normal.X * (inflationForceValue * _area / totArea) * 10000000;
            vec[1] = Normal.Y * (inflationForceValue * _area / totArea) * 10000000;
            vec[2] = Normal.Z * (inflationForceValue * _area / totArea) * 10000000;

            _if = vec;
        }

        /** Calculating the attractor attraction force. 
        *  
        */
        public void CalcAttractorForce(double attractorForceValue, double[] attractorPos)
        {
            double[] vec = new double[3];

            vec[0] = attractorPos[0] - _x[0];
            vec[1] = attractorPos[1] - _x[1];
            vec[2] = attractorPos[2] - _x[2];

            vec = Util.NormVector(vec);

            //Scaling the attractor force
            vec[0] = vec[0] * attractorForceValue * 100000;
            vec[1] = vec[1] * attractorForceValue * 100000;
            vec[2] = vec[2] * attractorForceValue * 100000;

            _af = vec;
        }

        /** Transform node due to strains.
        * 
        */
        public override void TransformDueToStrain(double dt, double vd, double loadSF, bool isSelfWeight, double surfaceLoadMagnitude)
        {
            CalcTranslation(dt, vd, loadSF, isSelfWeight, surfaceLoadMagnitude);
        }

        /** Calculate the rotation of the node due to the moment timestep (dt) and carry over factor (cf)
        * 
        */
        protected void CalcTranslation(double dt, double vd, double loadSF, bool isSelfWeight, double surfaceLoadMagnitude)
        {
            if (_constraint != eConstraint.Free)
                return;

            //Storing the previous force for visualization.
            _prevForce = _tF;

            _tF[0] = _iF[0] + loadSF * surfaceLoadMagnitude * _loadFromSurface[0] + _sbF[0] + _stF[0];
            _tF[1] = _iF[1] + loadSF * surfaceLoadMagnitude * _loadFromSurface[1] + _sbF[1] + _stF[1];
            _tF[2] = _iF[2] + loadSF * surfaceLoadMagnitude * _loadFromSurface[2] + _sbF[2] + _stF[2];

            if (isSelfWeight)
            {
                _tF[0] += loadSF * _loadFromMass[0];
                _tF[1] += loadSF * _loadFromMass[1];
                _tF[2] += loadSF * _loadFromMass[2];
            }
            
            //Calculate acceleration
            _a[0] = _tF[0] / _mf[0];
            _a[1] = _tF[1] / _mf[1];
            _a[2] = _tF[2] / _mf[2];

            //Calculate velocity
            _v[0] = vd * _v[0] + _a[0] * dt;
            _v[1] = vd * _v[1] + _a[1] * dt;
            _v[2] = vd * _v[2] + _a[2] * dt;

            //Calculate the new movement
            _move[0] = _v[0] * dt;
            _move[1] = _v[1] * dt;
            _move[2] = _v[2] * dt;

            //Check that the translation is not to big and might cause instability.
            StabilityControlTranslation();

            //Calculate translation
            _x[0] = _x[0] + _move[0];
            _x[1] = _x[1] + _move[1];
            _x[2] = _x[2] + _move[2];

            //Update Point3d instance
            _pt.X = _x[0];
            _pt.Y = _x[1];
            _pt.Z = _x[2];
        }

        /** Calculates the displacement. 
        *   
        */
        public void CalcDisplacement()
        {
            _d[0] = _x[0] - _initX[0];
            _d[1] = _x[1] - _initX[1];
            _d[2] = _x[2] - _initX[2];

            _dMag = Math.Sqrt(Math.Pow(_d[0], 2) + Math.Pow(_d[1], 2) + Math.Pow(_d[2], 2));
        }

        /** Calculating the angle between the node normal and the node-attractor point vector.
        *   Can be used the color mesh based on attractor point exposure.
        */
        public void CalcAngleToPt(double[] attPos)
        {
            Vector3d attVec = new Vector3d(attPos[0] - _x[0], attPos[1] - _x[1], attPos[2] - _x[2]);
            Vector3d.VectorAngle(_normal, attVec);
        }

        
        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Utilities ------------------------------------//
        #region

        /** Clear force on the node
        * 
        */        
        public override void Clear() 
        {
            ClearForce();
            ClearAcceleration();
            ClearVelocity();
            ClearSplineBarForce();
            ClearTranslationStiffness();
        }

        /** Clear force on the node
        * 
        */
        private void ClearForce()
        {
            _iF[0] = 0;
            _iF[1] = 0;
            _iF[2] = 0;
        }

        private void ClearAcceleration()
        {
            _a[0] = 0;
            _a[1] = 0;
            _a[2] = 0;
        }

        public void ClearSplineBarForce()
        {
            _sbF[0] = 0;
            _sbF[1] = 0;
            _sbF[2] = 0;

            _stF[0] = 0;
            _stF[1] = 0;
            _stF[2] = 0;
        }

        public void ClearTranslationStiffness() 
        {
            _transStiffness[0] = 0;
            _transStiffness[1] = 0;
            _transStiffness[2] = 0;
        }

        private void ClearVelocity()
        {
            _v[0] = 0;
            _v[1] = 0;
            _v[2] = 0;
        }

        /** Reverse normals
        * 
        */
        public void ReverseNormal()
        {
            _normal *= -1;
        }

        /** Returns the coordinates for the node as a string
        * 
        */
        public string PositionToString()
        {
            string output;
            output = "(" + _x[0] + "," + _x[1] + "," + _x[2] + ")";
            return output;
        }


        /** Calculating scaled new coordinates for the node. This is a stability measure to ensure the
        *   max move distance is not exceeded. 
        *   OBS! this method compares distance between the new coordinates and the old, therefor new 
        *   coordinates needs to be calculated before the scaling is done, otherwise exeption thrown!
        */
        public void ScaledNewCoord(double deltaT, double maxMoveDist)
        {
            double scaleMovement;

            if (_x1 == _initX && (MoveDist == 0))
                throw new Exception("No new coodinates have been calculated");

            scaleMovement = _moveDist / maxMoveDist;
            _x1[0] = _x[0] + (_v[0] * deltaT) / scaleMovement;
            _x1[1] = _x[1] + (_v[1] * deltaT) / scaleMovement;
            _x1[2] = _x[2] + (_v[2] * deltaT) / scaleMovement;
        }


        /** Find the shortest memeber connected to this node, the translational stability limit is the 
        *   set to be 5% of the shortest member length, meaning the node is prohibited to during one 
        *   iteration not move further then that limit.         
        */
        public override void CalcStabilityLimit()
        {
            double minL = 100000000;

            foreach (Bar br in GetBars())
            {
                if (br.Length < minL)
                    minL = br.Length;
            }
            _stabilityLimitTrans = 0.05 * minL;
        }

        /** Make sure that the translation is not to big.
        * 
        */
        public void StabilityControlTranslation()
        {
            double moveDist = Math.Sqrt(Math.Pow(_move[0], 2) + Math.Pow(_move[1], 2) + Math.Pow(_move[2], 2));
            double sf;
            if (moveDist > _stabilityLimitTrans)
            {
                sf = _stabilityLimitTrans / moveDist;
                _move[0] = sf * _move[0];
                _move[1] = sf * _move[1];
                _move[2] = sf * _move[2];

                //Damp out all movement.
                _v[0] = 0;
                _v[1] = 0;
                _v[2] = 0;
            }
        }


        public override void Rewind() 
        {
            base.Rewind();
            Clear();
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------------- Load and mass -----------------------------------//
        #region

        public override void ClearLoad()
        {
            ClearLoadFromMass();
            ClearLoadFromSurface();
            ClearSplineBarForce();
        }


        public void ClearLoadFromMass() 
        {
            _loadFromMass[0] = 0;
            _loadFromMass[1] = 0;
            _loadFromMass[2] = 0;
        }

        public void ClearLoadFromSurface()
        {
            _loadFromSurface[0] = 0;
            _loadFromSurface[1] = 0;
            _loadFromSurface[2] = 0;
        }


        public override void ApplyLoadFromSurface(double loadPerSquareMeter)
        {
            
        }




        #endregion
        //----------------------------------------------------------------------------------//

        public override void InitialiseCoordSystem()
        {
        }


    }
}
