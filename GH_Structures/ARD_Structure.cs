using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Structures;

namespace GH_Structures
{
    public class ARD_Structure : GH_Component
    {

        private Structure _prevStructure;

        /// <summary>
        /// Initializes a new instance of the GH_Structure class.
        /// </summary>
        public ARD_Structure()
            : base("ARD_Structure", "Structure", "Structure consisting of finite elements and nodes", "F+P", "Melax")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //00
            pManager.AddGenericParameter("Elements", "L", "Lines representing structural elements", GH_ParamAccess.list);
            //01
            pManager.AddGenericParameter("ConstraintPoints", "C", "ConstraintPoints", GH_ParamAccess.list);
            //02
            pManager.AddGenericParameter("Parameters", "P", "Parameters", GH_ParamAccess.item);
            //03
            pManager.AddBooleanParameter("Enabled", "Enabled", "Structure will not be created/updated until 'Enabled' is set to true", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure", "S", "Structure", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> lines = new List<Line>();
            List<Point3d> pts = new List<Point3d>();
            Parameters p = new Parameters();
            bool enabled = false;
            Mesh mesh = new Mesh();

            DA.GetDataList<Line>(0, lines);
            DA.GetDataList<Point3d>(1, pts);
            DA.GetData(2, ref p);
            DA.GetData(3, ref enabled);

            if (enabled) 
            {
                Structure newStructure = new Structure(lines, pts);

                if (!ARD_Util.IsSame(_prevStructure, newStructure))     //Not same, so update the structure.
                {
                    newStructure.Preprocess_GH();
                    _prevStructure = newStructure;
                }
                else                                                    //Same, so use the old one which is already preprocessed..
                {
                    newStructure = _prevStructure;
                }

                mesh = newStructure.GetMesh();

                DA.SetData(0, newStructure);
                DA.SetData(1, mesh);
            }
        }

        public override void  AddedToDocument(GH_Document document)
        {
            _prevStructure = new Structure();
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return GH_Structures.Properties.Resources.Structures_24x24;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{30D28837-389C-4E4B-A456-04055D5C17E6}"); }
        }
    }
}