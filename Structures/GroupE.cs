using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    class GroupE : Group
    {

        protected List<Edge> _edges;
        protected Material _material;
        protected CrossSection _crossSection;
        protected eRelease _realease;
        
        public new void Initialise() 
        {
            _edges = new List<Edge>();
            _material = new Steel();
            _crossSection = new CircularHollow();
            _realease = eRelease.none;
        }


        public GroupE() : base() 
        {
        
        }

        public void AddElement(Edge e) 
        {
            _edges.Add(e);
        }

        public void AddElements(List<Edge> es)
        {
            _edges.AddRange(es);
        }

        public List<Edge> GetElements() 
        {
            return _edges;
        }

        public Material Material
        {
            get { return _material; }
            set { _material = value; }
        }

        public CrossSection CrossSection
        {
            get { return _crossSection; }
            set { _crossSection = value; }
        }

        public void ApplyMaterial() 
        {
            foreach (Bar b in Util.CastToSubclass<Bar, Edge>(_edges))
                b.Material = _material;
        }

        public void ApplyCrossSection()
        {
            foreach (Bar b in Util.CastToSubclass<Bar, Edge>(_edges))
                b.CrossSection = _crossSection;
        }



    }
}
