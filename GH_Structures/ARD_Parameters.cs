using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Structures;

namespace GH_Structures
{
    public class ARD_Parameters : GH_Component
    {

        private Parameters _prevParameters;

        /// <summary>
        /// Initializes a new instance of the GH_Parameters class.
        /// </summary>
        public ARD_Parameters()
            : base("ARD_Parameters", "Parameters", "Parameters for the structure", "F+P", "Melax")
        {
        
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //00
            pManager.AddNumberParameter("Time step", "dt", "Time step", GH_ParamAccess.item, 0.01);
            //01
            pManager.AddNumberParameter("Carry over factor", "c", "Carry over factor (i.e. damping)", GH_ParamAccess.item, 0.95);
            //02
            pManager.AddVectorParameter("Gravity", "g", "Gravity vector", GH_ParamAccess.item, new Vector3d(0,0,-9.81));
            //03
            pManager.AddVectorParameter("Load", "l", "Additional load vector", GH_ParamAccess.item, new Vector3d(0, 0, 0));
            //04
            pManager.AddIntegerParameter("Iterations", "i", "Number of iterations per execution", GH_ParamAccess.item, 10);
            //05
            pManager.AddBooleanParameter("Enabled", "Enabled", "Changes/updates to the component are only applied when 'Enabled' is set to true", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "P", "Parameters", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double dt = 0.01;
            double cf = 0.95;
            int iterations = 0;
            Vector3d gravity = new Vector3d(0, 0, -9.81);
            Vector3d load = new Vector3d(0, 0, 0);
            bool enabled = false;

            DA.GetData<double>(0, ref dt);
            DA.GetData<double>(1, ref cf);
            DA.GetData<Vector3d>(2, ref gravity);
            DA.GetData<Vector3d>(3, ref load);
            DA.GetData<int>(4, ref iterations);
            DA.GetData<bool>(5, ref enabled);

            if (enabled) 
            {
                Parameters newParameters = new Parameters();
                newParameters.TimeStep = dt;
                newParameters.ViscousDamping = cf;
                newParameters.GravityMagnitude = gravity.Length;
                newParameters.SurfaceLoadMagnitude = load.Length;
                newParameters.NrIterations = iterations;
                gravity.Unitize();
                load.Unitize();
                newParameters.GravityVector = gravity;
                newParameters.SurfaceLoadVector = load;

                if (!ARD_Util.IsSame(_prevParameters, newParameters))
                {
                    DA.SetData(0, newParameters);
                    _prevParameters = newParameters;
                }
                else
                    DA.SetData(0, _prevParameters);
            }

        }

        public override void AddedToDocument(GH_Document document)
        {
            //Create instance of the parameters class with prese values for all parameters..
            _prevParameters = new Parameters();
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
                return GH_Structures.Properties.Resources.Parameter_24x24;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{061369f4-b699-4f2e-b421-308214f734aa}"); }
        }
    }
}