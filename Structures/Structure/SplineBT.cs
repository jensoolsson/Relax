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
    public class SplineBT : SplineB
    {
        // Not entierly convinced this element works i.e. that the 
        // torsion calculations are implemeted correctly...

        private Plane _plane_klm;
        private Point3d _midPt_klm;
        private Vector3d _normal_klm;
        private Vector3d _shearTwistS;
        private Vector3d _shearTwistE;
        private double _prevMax;

        public new void Initialise()
        {
            _plane_klm = new Plane();
            _midPt_klm = new Point3d();
            _normal_klm = new Vector3d();
            _prevMax = 0;

            _nts_Sf = 5;
            _sfEA = 0.01;
        }

        public SplineBT(Node3Dof nodeStart, Node3Dof nodeEnd) : base(nodeStart, nodeEnd) 
        {
            Initialise();
        }

        public Vector3d ShearTwistStart
        {
            get { return _shearTwistS; }
            set { _shearTwistS = value; }
        }

        public Vector3d ShearTwistEnd
        {
            get { return _shearTwistE; }
            set { _shearTwistE = value; }
        }

        public Point3d MidPoint2
        {
            get { return _midPt_klm; }
            set { _midPt_klm = value; }
        }

        public Vector3d Normal2
        {
            get { return _normal_klm; }
            set { _normal_klm = value; }
        }

        public Plane Plane2
        {
            get { return _plane_klm; }
            set { _plane_klm = value; }
        }

        public override void ClearForce()
        {
            base.ClearShearBend();

            ClearShearTwist();
        }

        private void ClearShearTwist()
        {
            _shearTwistS.X = 0;
            _shearTwistS.Y = 0;
            _shearTwistS.Z = 0;

            _shearTwistE.X = 0;
            _shearTwistE.Y = 0;
            _shearTwistE.Z = 0;
        }

        public override void CalcStrain()
        {
            base.CalcStrain();

            if (_isNextSet)
                base.CalcShearForce((SplineB)this, _nextSBar);

            if (_isPrevSet && _isNextSet)
                CalcShearFromTwist((SplineBT)_prevSBar, this, (SplineBT)_nextSBar);
        }

        public override void ApplyStrainToNodes()
        {
            base.ApplyStrainToNodes();

            ApplyShearBendToNodes();

            ApplyShearTiwstToNodes();
        }

        public void CalcShearFromTwist(SplineBT sBT_jk, SplineBT sBT_kl, SplineBT sBT_lm)
        {
            double hm, hj, angle, deg, a, b, c, pj, pk, pl, pm, J, G;
            Vector3d vPj, vPk, vPl, vPm;

            Node3Dof nj = sBT_jk.GetStart();
            Node3Dof nk = sBT_jk.GetEnd();
            Node3Dof nl = sBT_kl.GetEnd();
            Node3Dof nm = sBT_lm.GetEnd();

            Vector3d vSB1 = sBT_jk.GetEnd().Point - sBT_jk.GetStart().Point;
            Vector3d vSB3 = sBT_lm.GetEnd().Point - sBT_lm.GetStart().Point;

            double testAngle = Vector3d.VectorAngle(vSB1, vSB3);
            double testAngleDeg = Util.RadToDeg(testAngle);

            if (testAngleDeg < 4)
            {
                return;
            }

            _plane_jkl = new Plane(nj.Point, nk.Point, nl.Point);
            _plane_klm = new Plane(nk.Point, nl.Point, nm.Point);

            _midPt_jkl = (sBT_jk.GetStart().Point + sBT_jk.GetEnd().Point + sBT_kl.GetEnd().Point) / 3;
            _midPt_klm = (sBT_kl.GetStart().Point + sBT_kl.GetEnd().Point + sBT_lm.GetEnd().Point) / 3;

            _normal_jkl = _plane_jkl.Normal;
            _normal_klm = _plane_klm.Normal;
            _normal_jkl.Unitize();
            _normal_klm.Unitize();

            Line line_jl = new Line(nj.Point, nl.Point);
            double dist = nk.Point.DistanceTo(line_jl.ClosestPoint(nk.Point, true));

            if (dist < 0.01)
            {

            }

            angle = Vector3d.VectorAngle(_normal_jkl, _normal_klm);
            deg = Util.RadToDeg(angle);

            _angle = angle;

            if (deg < 5 || deg > 45)
            {
                return;
            }

            Line line_kl = new Line(nk.Point, nl.Point);
            Point3d cp_nj_kl = line_kl.ClosestPoint(nj.Point, false);
            Point3d cp_nm_kl = line_kl.ClosestPoint(nm.Point, false);

            hj = nj.Point.DistanceTo(cp_nj_kl);
            hm = nm.Point.DistanceTo(cp_nm_kl);

            a = nk.Point.DistanceTo(cp_nj_kl);
            b = nk.Point.DistanceTo(nl.Point);
            c = nl.Point.DistanceTo(cp_nm_kl);

            J = _crossSection.J;
            G = _material.ShearModulus;

            pj = (G * J * (angle - _restAng1)) / (hj * sBT_jk.Length);
            pm = (G * J * (angle - _restAng2)) / (hm * sBT_jk.Length);
            pl = (pj * a + pm * (b + c)) / b;
            pk = pl + pj - pm;


            List<double> vals = new List<double>();
            vals.Add(Math.Abs(pj));
            vals.Add(Math.Abs(pm));
            vals.Add(Math.Abs(pl));
            vals.Add(Math.Abs(pk));

            double max = Util.Max(vals);

            if (max > (5 * _prevMax) && _prevMax > 0)
            {

            }

            vPj = pj * 1 * (new Vector3d(_normal_jkl));
            vPm = pm * -1 * (new Vector3d(_normal_klm));
            vPk = pk * -1 * (new Vector3d(_normal_jkl));
            vPl = pl * 1 * (new Vector3d(_normal_klm));

            //Apply to the bars
            sBT_jk.ShearTwistStart += vPj;
            sBT_kl.ShearTwistStart += vPk;
            sBT_kl.ShearTwistEnd += vPl;
            sBT_lm.ShearTwistEnd += vPm;

            _prevMax = max;
        }

        private void ApplyShearTiwstToNodes()
        {
            GetStart().ShearTwistForce[0] += _shearTwistS.X;
            GetStart().ShearTwistForce[1] += _shearTwistS.Y;
            GetStart().ShearTwistForce[2] += _shearTwistS.Z;

            GetEnd().ShearTwistForce[0] += _shearTwistE.X;
            GetEnd().ShearTwistForce[1] += _shearTwistE.Y;
            GetEnd().ShearTwistForce[2] += _shearTwistE.Z;
        }


    }
}
