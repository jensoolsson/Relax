using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public abstract class Material
    {
        protected double _density;
        protected double _E;
        protected double _G;
        protected double _yieldStrength;
        protected double _ultimateStrength;

        public Material()
        {

        }

        public double Density
        {
            get { return _density; }
            set { _density = value; }
        }

        public double YoungsModulus
        {
            get { return _E; }
            set { _E = value; }
        }

        public double ShearModulus
        {
            get { return _G; }
            set { _G = value; }
        }

        public double YieldStrength
        {
            get { return _yieldStrength; }
            set { _yieldStrength = value; }
        }

        public double UlitimateStrength
        {
            get { return _ultimateStrength; }
            set { _ultimateStrength = value; }
        }


    }
}
