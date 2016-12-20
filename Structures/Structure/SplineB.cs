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
    public class SplineB : Bar
    {
        protected SplineB _prevSBar;
        protected SplineB _nextSBar;
        protected double _restAng1;
        protected double _restAng2;
        protected Point3d _midPt_jkl;
        protected Plane _plane_jkl;
        protected Vector3d _normal_jkl;

        protected double _angle;

        protected bool _isPrevSet;
        protected bool _isNextSet;

        protected Vector3d _shearBendS;
        protected Vector3d _shearBendE;

        public new void Initialise() 
        {
            _plane_jkl = new Plane();
            _midPt_jkl = new Point3d();
            _normal_jkl = new Vector3d();
            _restAng1 = Util.DegToRad(0);
            _restAng2 = Util.DegToRad(0);
            _isPrevSet = false;
            _isNextSet = false;

            _nts_Sf = 0.1;
            _sfEA = 0.01;
        }

        public SplineB()
        {
        
        }

        public SplineB(Node3Dof ns, Node3Dof ne) : base(ns, ne) 
        {
            Initialise();
        }

        public SplineB PreviousSplineBar 
        {
            get { return _prevSBar; }
            set { _prevSBar = value; }
        }

        public SplineB NextSplineBar
        {
            get { return _nextSBar; }
            set { _nextSBar = value; }
        }

        public bool PreviousSplineBarIsSet 
        {
            get { return _isPrevSet; }
            set { _isPrevSet = value; }
        }

        public bool NextSplineBarIsSet
        {
            get { return _isNextSet; }
            set { _isNextSet = value; }
        }

        public Vector3d ShearBendStart
        {
            get { return _shearBendS; }
            set { _shearBendS = value; }
        }

        public Vector3d ShearBendEnd
        {
            get { return _shearBendE; }
            set { _shearBendE = value; }
        }

        public Vector3d Normal1 
        {
            get { return _normal_jkl; }
            set { _normal_jkl = value; }
        }

        public Plane Plane1 
        {
            get { return _plane_jkl; }
            set { _plane_jkl = value; }
        }

        public Point3d MidPoint1 
        {
            get { return _midPt_jkl; }
            set { _midPt_jkl = value; }
        }

        public double RestAngle1 
        {
            get { return _restAng1; }
            set { _restAng1 = value; }
        }

        public double RestAngle2
        {
            get { return _restAng2; }
            set { _restAng2 = value; }
        }

        public double Angle 
        {
            get { return _angle; }
            set { _angle = value; }
        }


        public override void ClearForce() 
        {
            ClearShearBend();
        }

        protected void ClearShearBend()
        {
            _shearBendS.X = 0;
            _shearBendS.Y = 0;
            _shearBendS.Z = 0;

            _shearBendE.X = 0;
            _shearBendE.Y = 0;
            _shearBendE.Z = 0;
        }



        public override void CalcStrain()
        {
            base.CalcStrain();

            if(_isNextSet)
                CalcShearForce(this, _nextSBar);
        }

        public override void ApplyStrainToNodes()
        {
            base.ApplyStrainToNodes();

            ApplyShearBendToNodes();
        }

        /** Assuming small angles between the bars.
        *    
        */
        protected void CalcShearForce(SplineB sBar1, SplineB sBar2)
        {
            double Sa, Sb, alpha, I, E, la, lc, lb;
            Node3Dof ni, nj, nk;
            Vector3d va, vb, vc, vPerp_a, vPerp_b;

            ni = sBar1.GetStart();
            nj = sBar1.GetEnd();
            nk = sBar2.GetEnd();

            _midPt_jkl = (ni.Point + nj.Point + nk.Point) / 3;

            //Calculate alpha angle between the two bars.
            va = nj.Point - ni.Point;
            vb = nk.Point - nj.Point;
            vc = nk.Point - ni.Point;

            //The three node span a plane, and the plane normal is:
            _normal_jkl = Vector3d.CrossProduct(vc, va);
            _normal_jkl.Unitize();

            //Vectors in the plane, perpendicular to bars:
            vPerp_a = Vector3d.CrossProduct(va, _normal_jkl);
            vPerp_b = Vector3d.CrossProduct(vb, _normal_jkl);

            vPerp_a.Unitize();
            vPerp_b.Unitize();

            alpha = Vector3d.VectorAngle(va, vb);

            double alphaDeg = Util.RadToDeg(alpha);

            //Assuming cirular cross sections, so either Ix, or Iy
            I = sBar1.CrossSection.Ix;
            E = sBar1.Material.YoungsModulus;
            la = sBar1.Length;
            lb = sBar2.Length;
            lc = sBar1.GetStart().Point.DistanceTo(sBar2.GetEnd().Point);

            Sa = (2 * E * I * Math.Sin(alpha)) / (la * lc);
            Sb = (2 * E * I * Math.Sin(alpha)) / (lb * lc);

            //Saving the force as vector with direction and magnitude..

            //Reg dir is "down" in the Barnes paper figure.
            Vector3d Sa_vec_Reg, Sb_vec_Reg, Sa_vec_Rev, Sb_vec_Rev;

            Sa_vec_Reg = Sa * vPerp_a;
            Sb_vec_Reg = Sb * vPerp_b;
            Sa_vec_Rev = new Vector3d(Sa_vec_Reg);
            Sb_vec_Rev = new Vector3d(Sb_vec_Reg);
            Sa_vec_Rev.Reverse();
            Sb_vec_Rev.Reverse();

            sBar1.ShearBendStart += Sa_vec_Rev;
            sBar1.ShearBendEnd += Sa_vec_Reg;
            sBar2.ShearBendStart += Sb_vec_Reg;
            sBar2.ShearBendEnd = Sb_vec_Rev;
        }

        protected void ApplyShearBendToNodes()
        {
            GetStart().ShearBendForce[0] += _shearBendS.X;
            GetStart().ShearBendForce[1] += _shearBendS.Y;
            GetStart().ShearBendForce[2] += _shearBendS.Z;

            GetEnd().ShearBendForce[0] += _shearBendE.X;
            GetEnd().ShearBendForce[1] += _shearBendE.Y;
            GetEnd().ShearBendForce[2] += _shearBendE.Z;
        }


        public override void DirectSizing()
        {
            //Yet to be implemeted...
        }




    }
}
