using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Structures;

namespace GH_Structures
{
    public class ARD_Solver : GH_Component
    {

        private Structure _prevStructure;
        private Parameters _prevParameters;
        private Main _mainSolver;

        /// <summary>
        /// Initializes a new instance of the GH_Main class.
        /// </summary>
        public ARD_Solver()
            : base("ARD_Solver", "Solver", "Main program that runs structural analysis", "F+P", "Melax")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //00
            pManager.AddGenericParameter("Structure", "S", "Structure to analyse", GH_ParamAccess.item);
            //01
            pManager.AddGenericParameter("Parameters", "P", "Parameters for analysis and drawing", GH_ParamAccess.item);
            //02
            pManager.AddBooleanParameter("Execute Relaxation", "Enabled", "True means execute relaxation", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //00
            pManager.AddGenericParameter("Mesh", "M", "Mesh from the form finding", GH_ParamAccess.item);
            //01
            pManager.AddGenericParameter("Elemet Mesh", "EM", "Mesh for elements", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Parameters p = new Parameters();
            Structure structure = new Structure();
            bool enabled = false;
            
            if(!DA.GetData(0, ref structure)){return;}
            if(!DA.GetData(1, ref p)){return;}

            if(!ARD_Util.IsSame(_mainSolver.Parameters, p))
            {
                _mainSolver.Parameters = p;
            }

            if (!ARD_Util.IsSame(_mainSolver.Structure, structure))  //Not the same structure
            {
                structure.PreprocessStructure(p);
                _mainSolver.Structure = structure;
            }
            else //Same structure, so do not pre process, only update.
            {
                //_mainSolver.Structure.Update_GH();
            }

            DA.GetData(2, ref enabled);

            if (enabled) 
            {
                //_mainSolver.RunDR_GH();

                _mainSolver.Structure.BuildMesh();

                _mainSolver.Structure.UpdateGeometry(p);

                DA.SetData(0, _mainSolver.Structure.GetMesh());

                DA.SetDataList(1,  ARD_Util.WrapObjects<Mesh>(_mainSolver.Structure.Get3dBars()));
            }
        }

        public override void AddedToDocument(GH_Document document)
        {
            _prevStructure = new Structure();
            _prevParameters = new Parameters();
            _mainSolver = new Main(_prevStructure, _prevParameters);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return GH_Structures.Properties.Resources.Execute_24x24;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{f826a999-833f-4b34-b3fd-8a5834256ed3}"); }
        }
    }
}