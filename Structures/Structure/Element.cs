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
    public class Element : Edge
    {
        
        protected double _convergeForce;                    //Force from the element to be summed over the nodes for convergence test. 
        protected bool _isMeshBuilt;                        //boolean saying wethear or not mesh is built
        protected double[] _colorBy;                        //visValue is a container for a value that will be visualized. 
        protected CrossSection _crossSection;               //Cross section


        new public void Initialise()
        {
            _isMeshBuilt = false;
            _colorBy = new double[2];
            _crossSection = new CircularHollow();
        }

        
        public Element()
        {
            Initialise();
        }

        public Element(int id) : base(id)
        {
            Initialise();
        }

        public Element(Node3Dof nodeStart, Node3Dof nodeEnd) : base((Vertex)nodeStart, (Vertex)nodeEnd)
        {
            Initialise();
        }
        

        public double ConvergForce
        {
            get { return _convergeForce; }
            set { _convergeForce = value; }
        }

        public double[] ColorBy
        {
            get { return _colorBy; }
            set { _colorBy = value; }
        }

        public bool IsMeshBuilt
        {
            get { return _isMeshBuilt; }
            set { _isMeshBuilt = value; }
        }

        public Mesh GetMesh()
        {
            return _crossSection.GetMesh();
        }

        //Virtual methods

        public virtual void CalcDeformation() { }

        public virtual void CalcStrain() { }

        public virtual void CalcNodeStiffness() { }

        public virtual void ApplyStrainToNodes() { }

        public virtual void ApplyForceToNodes() { }

        public virtual void CalcElongation() { }

        public virtual void ClearForce() { }

        public virtual void CalcAxialForce() { }

        public virtual void ApplyReleases() { }

        public virtual void DirectSizing() { }

        public virtual void IncrementalSizing() { }

        public virtual void CalcSectionProperties() { }

        public virtual void ColorMesh(double realMin, double realMax, double showMin, double showMax) { }

        public virtual void BuildMesh() { }

        public virtual void ApplyLoadFromMass() { }

        public virtual void ApplyLoadFromSurface() { }

        public virtual void ApplyNodeStiffness() { }

        public virtual void CalcLoadFromSurface(Vector3d dirVec, double magnitude, eLoadType loadType) { }

        public virtual void CalcPointLoadFromSurface(Vector3d loadDir, double magnitude) { }

        public virtual void CalcLineLoadFromSurface(Vector3d loadDir, double loadPerMeter) { }

        public virtual void CalcAreaLoadFromSurface(Vector3d loadDir, double loadPerSquareMeter) { }

        public virtual void CalcLoadFromMass(Vector3d dirVec, double gravity, eLoadType loadType) { }

        public virtual void InitialiseMesh() { }

        public virtual void CalcInitialTwist() { }

        public virtual void InitialiseNormalCoordSystem() { }

        public virtual void InitialiseCoordSystem() { }

        public virtual void InitialiseVerticalCoordSystem() { }

        public virtual void CalcMass() { }

        public virtual void PostProcess() { }

        public virtual void CalcConvergForce() { }
        

    }
}
