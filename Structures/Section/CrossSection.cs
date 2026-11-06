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
    public abstract class CrossSection
    {
        protected double _Ix;           //Moment of inertia around x-axis
        protected double _Iy;           //Moment of inertia around y-axis
        protected double _Ixy;          //"Product" of moments of inertia (NOT = Ix * Iy, product is just what its called).
        protected double _J;            //Torsinal modulus
        protected double _A;            //Section area
        protected double _t;            //Thickness of section
        protected double _r1;           //tube outer radius
        protected double _r2;           //tube iner radius
        protected double _h;            //height
        protected double _b;            //width

        protected eCrossSection _crossSectionType;
        protected ElementMesh _elemMesh;

        public CrossSection()
        {

        }

        //------------------------------------- Methods ------------------------------------//
        #region

        public abstract void CalcSectionProperties();

        public abstract void Scale(double sf);
        
        public abstract void IncrementSize(double sf);
        
        public abstract void ApplyPreviousSection();

        public abstract void GetXY(ref double x, ref double y);

        public abstract void Rewind();


        /** Passing on call to elementMesh class to build mesh geometry.
        * 
        */
        public void BuildMesh(Bar bar)
        {
            _elemMesh.BuildMesh(bar);
        }

        /** Passing on call to elementMesh class to color build mesh based on values min and max.
        * 
        */
        public void ColorMesh(Bar bar, double realMin, double realMax, double showMin, double showMax)
        {
            _elemMesh.ColorMesh(bar, realMin, realMax, showMin, showMax);
        }

        public Mesh GetMesh()
        {
            return _elemMesh.Mesh;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Propperties ----------------------------------//
        #region

        public double Ix
        {
            get { return _Ix; }
            set { _Ix = value; }
        }

        public double Iy
        {
            get { return _Iy; }
            set { _Iy = value; }
        }

        public double Ixy
        {
            get { return _Ixy; }
            set { _Ixy = value; }
        }

        public double J
        {
            get { return _J; }
            set { _J = value; }
        }

        public double Area
        {
            get { return _A; }
            set { _A = value; }
        }

        public double Thickness
        {
            get { return _t; }
            set { _t = value; }
        }

        public eCrossSection CrossSectionType
        {
            get { return _crossSectionType; }
            set { _crossSectionType = value; }
        }

        public double RadiusOuter
        {
            get { return _r1; }
            set { _r1 = value; }
        }

        public double Height
        {
            get { return _h; }
            set { _h = value; }
        }

        public double Width
        {
            get { return _b; }
            set { _b = value; }
        }

        public ElementMesh Mesh
        {
            get { return _elemMesh; }
        }

        public double RadiusInner
        {
            get { return _r2; }
            set { _r2 = value; }
        }


        #endregion
        //----------------------------------------------------------------------------------//



    }
}
