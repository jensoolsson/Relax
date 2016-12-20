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
    public abstract class ElementMesh
    {
        protected Mesh _mesh;
        protected int _nrSections;
        protected System.Drawing.Color _color1;
        protected System.Drawing.Color _color2;

        public void Initialise() 
        {
            _color1 = new System.Drawing.Color();
            _color2 = new System.Drawing.Color();
        }

        public ElementMesh()
        {
            _mesh = new Mesh();
        }

        //---------------------------------- Methods ---------------------------------------//
        #region

        public Mesh Mesh
        {
            get { return _mesh; }
            set { _mesh = value; }
        }

        public int NrSections
        {
            get { return _nrSections; }
            set { _nrSections = value; }
        }

        public abstract void BuildMesh(Bar bar);

        public abstract void BuildMesh(Beam beam);

        public abstract void ColorMesh(Bar bar, double realMin, double realMax, double showMin, double showMax);


        #endregion
        //----------------------------------------------------------------------------------//
    }
}
