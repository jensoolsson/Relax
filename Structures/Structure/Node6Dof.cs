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
    public class Node6Dof : Node3Dof
    {
        private double[] _rot;                      //Rotation for the next timespet before applied to the nodes 
        private double[] _angRot;                   //Angle in relation to coord system.
        private double[] _angVel;                   //Angular velocity
        private double[] _angAcc;                   //Angular acceleration 
        private double[] _tM;                       //Moment                  
        private double[] _pM;                       //Previous moment
        private double[] _moi;                      //Moment of inertia

        private CoordSystem _cs;                    //Coordinate system
       
        private double _stabilityLimitRot;                       
        private double _gamma;                      

        private int[] _dofs;
        private double[] _rotStiffness;             //Rotational stiffness used for moment of inertia calculations


        public new void Initialise()
        {
            _rot = new double[3] { 0, 0, 0 };
            _move = new double[3] { 0, 0, 0 };
            _angRot = new double[3] { 0, 0, 0 };             //Angle in relation to coord system.
            _angVel = new double[3] { 0, 0, 0 };
            _angAcc = new double[3] { 0, 0, 0 };
            _tM = new double[3] { 0, 0, 0 };
            _pM = new double[3] { 0, 0, 0 };
            _prevForce = new double[3] { 0, 0, 0 };
            _moi = new double[3];

            _cs = new CoordSystem();

            _stabilityLimitRot = 0;
            _gamma = 0;

            _rotStiffness = new double[3] { 0, 0, 0 };

            //Set degrees of freedom for matrix analysis.
            SetDofs();
        }


        //--------------------------------- Constructors -----------------------------------//
        #region

        public Node6Dof() : base()
        {
            Initialise();
        }

        public Node6Dof(Point3d pt) : base(pt)
        {
            Initialise();
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------- Properties and add methods----------------------------//
        #region


        /** TODO: Rename.
        * 
        */
        public double[] Rotation
        {
            get { return _rot; }
            set { _rot = value; }
        }

        public double[] AngularPosition
        {
            get { return _angRot; }
            set { _angRot = value; }
        }

        public double[] AngularAcceleration
        {
            get { return _angAcc; }
            set { _angAcc = value; }
        }

        public double[] AngularVelocity
        {
            get { return _angVel; }
            set { _angVel = value; }
        }

        public CoordSystem CoordSystem
        {
            get { return _cs; }
            set { _cs = value; }
        }

        public double[] Moment
        {
            get { return _tM; }
            set { _tM = value; }
        }

        public double[] PreviousMoment
        {
            get { return _pM; }
            set { _pM = value; }
        }

        public double[] PreviousForce
        {
            get { return _prevForce; }
            set { _prevForce = value; }
        }

        public int[] DOFS
        {
            get { return _dofs; }
            set { _dofs = value; }
        }

        public double StabilityLimitRotations
        {
            get { return _stabilityLimitRot; }
            set { _stabilityLimitRot = value; }
        }

        public void AddMoment(double mx, double my, double mz)
        {
            _tM[0] += mx;
            _tM[1] += my;
            _tM[2] += mz;
        }

        public double[] RotationalStiffness
        {
            get { return _rotStiffness; }
            set { _rotStiffness = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Initialise and reset ------------------------------//
        #region

        /** Sets vector with degrees of freedom for matrix analysis.
        * 
        */
        public void SetDofs()
        {
            _dofs = new int[6];
            for (int i = 0; i < 6; i++)
            {
                _dofs[i] = _id * 6 + i;
            }
        }

        /** Set the coordinate system for the nodes to coincide with the global x,y,z coordinate system.    
        *   
        */
        public override void InitialiseCoordSystem()
        {
            Vector3d xVec = new Vector3d(1, 0, 0);
            Vector3d yVec = new Vector3d(0, 1, 0);
            Vector3d zVec = new Vector3d(0, 0, 1);

            //HACK!!!! If there is just links of lines the normals will not have been calculated correctly so
            //therefor set the normal to the z-vector
            if (_normal.IsTiny())
                _normal = zVec;

            _cs.X = xVec;
            _cs.Y = yVec;
            _cs.Z = zVec;
        }

        /** Clear force, moment and rotations
        *
        */
        public override void Clear() 
        {
            base.Clear();
            ClearMoments();
            ClearRotations();
            ClearRotationalStiffness();
        }

        private void ClearMoments()
        {
            _tM[0] = 0;
            _tM[1] = 0;
            _tM[2] = 0;
        }

        private void ClearRotationalStiffness() 
        {
            _rotStiffness[0] = 0;
            _rotStiffness[1] = 0;
            _rotStiffness[2] = 0;
        }

        /** Set rotations from the last iteration to zero.
        *
        */ 
        private void ClearRotations()
        {
            _rot[0] = 0;
            _rot[1] = 0;
            _rot[2] = 0;
        }

        private void ClearAngularVelocity()
        {
            _angVel[0] = 0;
            _angVel[1] = 0;
            _angVel[2] = 0;
        }

        private void ClearAngularAcceleration()
        {
            _angAcc[0] = 0;
            _angAcc[1] = 0;
            _angAcc[2] = 0;
        }

        private void ClearAngularRotation()
        {
            _angRot[0] = 0;
            _angRot[1] = 0;
            _angRot[2] = 0;
        }



        /** Sets current state to initial state.
        * 
        */
        public override void Rewind()
        {   
            base.Rewind();
            Clear();
            InitialiseCoordSystem();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Calculate -------------------------------------//
        #region

        /** Calculate the mass and moment of inertia for faster convergence. 
        *   
        */
        public override void CalcFictiousMass(double dt, double transStiffSF, double rotStiffSF)
        {
            CalcFictiousMassInternal(dt, transStiffSF, rotStiffSF);
            //CalcMomentOfInertia(dt);
        }

        /** Calculate the fictional mass based on axial and rotational stiffness. 
        *   
        */
        private void CalcFictiousMassInternal(double dt, double alpha, double beta)
        {    
            _gamma = 0;         //Scale factor for bending stiffness used when taking bending into account for the mass calc. 

            //Fictious translational mass
            _mf[0] = 1 + Math.Pow(dt, 2) * (alpha * _transStiffness[0] + _gamma * (_rotStiffness[2] + _rotStiffness[1])) / 2;
            _mf[1] = 1 + Math.Pow(dt, 2) * (alpha * _transStiffness[1] + _gamma * (_rotStiffness[2] + _rotStiffness[0])) / 2;
            _mf[2] = 1 + Math.Pow(dt, 2) * (alpha * _transStiffness[2] + _gamma * (_rotStiffness[0] + _rotStiffness[1])) / 2;

            //Fictious moment of inertia
            _moi[0] = 1 + Math.Pow(dt, 2) * beta * _rotStiffness[0] / 2;
            _moi[1] = 1 + Math.Pow(dt, 2) * beta * _rotStiffness[1] / 2;
            _moi[2] = 1 + Math.Pow(dt, 2) * beta * _rotStiffness[2] / 2;
        }


        public override void TransformDueToStrain(double dt, double vd, double loadSF, bool isSelfWeight, double surfaceLoadMagnitude) 
        {
            CalcTranslation(dt, vd, loadSF, isSelfWeight, surfaceLoadMagnitude);

            CalcRotation(dt, vd);

            RotateCS();
        }


        /** Calculate the rotation of the node due to the moment timestep (dt) and carry over factor (cf)
        * 
        */
        private void CalcRotation(double dt, double cf)
        {
            if (_constraint == eConstraint.Fixed)
                return;

            //Storing the previous moment for visualization.
            _pM = _tM;

            //Calculate angular acceleration
            _angAcc[0] = _tM[0] / _moi[0];
            _angAcc[1] = _tM[1] / _moi[1];
            _angAcc[2] = _tM[2] / _moi[2];

            //Calculate angular velocity
            _angVel[0] = _angAcc[0] * dt + cf * _angVel[0];
            _angVel[1] = _angAcc[1] * dt + cf * _angVel[1];
            _angVel[2] = _angAcc[2] * dt + cf * _angVel[2];

            //Calculate the new rotation
            _rot[0] = _angVel[0] * dt;
            _rot[1] = _angVel[1] * dt;
            _rot[2] = _angVel[2] * dt;

            //Make sure the rotation is not to big and might cause instability.
            StabilityControlRotation();

            //Update the total rotaion
            _angRot[0] = _angRot[0] + _rot[0];
            _angRot[1] = _angRot[1] + _rot[1];
            _angRot[2] = _angRot[2] + _rot[2];
        }

        /** Rotate the node coordinate system with the angle calculated in _newRot.
        * 
        */
        private void RotateCS()
        {
            if (_rot[0] + _rot[1] + _rot[2] == 0)
                return;

            Vector3d x = new Vector3d(1, 0, 0);
            Vector3d y = new Vector3d(0, 1, 0);
            Vector3d z = new Vector3d(0, 0, 1);

            Vector3d rotVec = new Vector3d();
            try
            {
                rotVec = CalcRotationVector(x, y, z, _rot[0], _rot[1], _rot[2]);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
                throw new Exception("Function CalcRotationVector called from RotateCS in class Node6Dof failing");
            }

            if (!rotVec.IsTiny())
            {
                Matrix tM = new Matrix(3, 3);
                try
                {
                    tM = _cs.CalcTransformationMatrix(rotVec);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                    throw new Exception("Function CalcTransformationMatrix called from RotateCS in class Node6Dof failing");
                }
                _cs.Transform(tM);
                CoordSystem tempCS;

                //Rotate the coordinate systems for the beam ends that connect to this node. 
                foreach (Edge e in _edges)
                {
                    if (e.GetType() == typeof(Beam)) 
                    {
                        Beam b = (Beam)e;               //This should prb be possible to avoid...

                        if (b.GetStart().Equals(this))
                            tempCS = b.CoordSystem1;
                        else
                            tempCS = b.CoordSystem2;

                        tempCS.Transform(tM);
                    }
                }
            }
        }

        /** Calculate one rotation vector based on rotation around the three axises.
        *   
        */
        private Vector3d CalcRotationVector(Vector3d v1, Vector3d v2, Vector3d v3, double a1, double a2, double a3)
        {
            v1.Unitize();
            v2.Unitize();
            v3.Unitize();

            v1 = v1 * Math.Tan(a1 / 2);
            v2 = v2 * Math.Tan(a2 / 2);
            v3 = v3 * Math.Tan(a3 / 2);

            Vector3d v4, v5;
            v4 = (v1 + v2 + Util.MyOnCrossProduct(v2, v1)) / (1 - Util.MyDotProd(v1, v2));
            v5 = (v3 + v4 + Util.MyOnCrossProduct(v4, v3)) / (1 - Util.MyDotProd(v3, v4));

            return v5;
        }

            
        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Stability Controll --------------------------------//
        #region

        /** Make sure that the rotation is not to big.
        * 
        */
        public void StabilityControlRotation()
        {
            double sFac;
            double maxRot = -1000000;

            if (_rot[0] > _stabilityLimitRot || _rot[1] > _stabilityLimitRot || _rot[2] > _stabilityLimitRot)
            {
                //Find the greatest rotation.
                for (int i = 0; i < 3; i++)
                    if (_rot[i] > maxRot)
                        maxRot = _rot[i];

                sFac = _stabilityLimitRot / maxRot;

                _rot[0] = sFac * _rot[0];
                _rot[1] = sFac * _rot[1];
                _rot[2] = sFac * _rot[2];
            }
        }

        public override void CalcStabilityLimit()
        {
            base.CalcStabilityLimit();

            _stabilityLimitRot = Math.PI / 100;
        }

        #endregion
        //----------------------------------------------------------------------------------//



    }
}
