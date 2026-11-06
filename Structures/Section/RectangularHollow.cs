using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class RectangularHollow : CrossSection
    {
        //Initial sizes not changed when the section is scaled
        private double _iB;
        private double _iH;
        private double _iT;

        private double _prevB;
        private double _prevH;
        private double _prevT;


        public void Initialise() 
        {
            _crossSectionType = eCrossSection.Rectangular_Hollow;
            _elemMesh = new Rectangular();

            _b = 0.2;       //Outer width of section
            _h = 0.4;       //Outer height of section
            _t = 0.05;      //Wall thickness,for simplicity same in all directions

            _iB = _b;
            _iH = _h;
            _iT = _t;
        }

        public RectangularHollow() : base()
        {
            Initialise();
        }


        //---------------------------------- Methods ---------------------------------------//
        #region

        public override void CalcSectionProperties()
        {
            _A = _b * _h - (_b - 2 * _t) * (_h - 2 * _t);

            //bh3/12 for outer - bh3/12 for inner.
            _Ix = (_b * Math.Pow(_h, 3) / 12.0) - ((_b - 2 * _t) * Math.Pow((_h - 2 * _t), 3) / 12.0);
            _Iy = (_h * Math.Pow(_b, 3) / 12.0) - ((_h - 2 * _t) * Math.Pow((_b - 2 * _t), 3) / 12.0);

            _Ixy = 0; //Confirm this.

            //J = (2*t*t*(b^2)*(h^2)) / (b*t + h*t)
            double taljare = 2 * _t * _t * Math.Pow(_b - _t, 2) * Math.Pow(_h - _t, 2);
            double namnare = (_b - _t) * _t + (_h - _t) * _t;
            _J = taljare / namnare;
        }

        public override void Scale(double sf)
        {
            _h = sf * _iH;
            _b = sf * _iB;
            _t = sf * _iT;
        }

        public override void IncrementSize(double sf)
        {
            _prevH = _h;
            _prevB = _b;
            _prevT = _t;

            _h = sf * _h;
            _b = sf * _b;
            _t = sf * _t;
        }

        public override void ApplyPreviousSection()
        {
            _h = _prevH;
            _b = _prevB;
            _t = _prevT;
        }

        public override void GetXY(ref double x, ref double y)
        {
            x = _b;
            y = _h;
        }

        public override void Rewind()
        {
            _h = _iH;
            _b = _iB;
            _t = _iT;
            CalcSectionProperties();
        }

        #endregion
        //----------------------------------------------------------------------------------//
    }
}
