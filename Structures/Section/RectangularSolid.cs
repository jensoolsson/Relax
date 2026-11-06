using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    class RectangularSolid : CrossSection
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
            _crossSectionType = eCrossSection.Rectangular_Solid;
            _elemMesh = new Rectangular();

            _b = 0.1;       //Outer width of section
            _h = 0.2;       //Outer height of section
            _t = 0.01;      //Wall thickness,for simplicity same in all directions

            _iB = _b;
            _iH = _h;
            _iT = _t;
        }

        public RectangularSolid()
        {
            Initialise();
        }

        //---------------------------------- Methods ---------------------------------------//
        #region

        public override void CalcSectionProperties()
        {
            _A = _b * _h;

            //bh3/12..
            _Ix = (_b * Math.Pow(_h, 3) / 12.0);
            _Iy = (_h * Math.Pow(_b, 3) / 12.0);

            _Ixy = 0; //Confirm this.

            double a = _b / 2;
            double b = _h / 2;

            _J = a * b * b * b * ((16 / 3) - 3.36 * (b / a) * (1 - (b * b * b * b) / (12 * a * a * a * a)));
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

        public override void Scale(double sf)
        {
            _h = sf * _iH;
            _b = sf * _iB;
            _t = sf * _iT;
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
