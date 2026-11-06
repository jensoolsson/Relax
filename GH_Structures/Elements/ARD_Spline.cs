using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Structures;

namespace GH_Structures
{
    public class ARD_Spline : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ARD_Spline()
            : base("ARD_Spline", "Spline", "Spline element that takes axial force and has angular spring (sort of bending stiffness) between links", "F+P", "Melax")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //00
            pManager.AddLineParameter("Polyline", "PL", "Polylines as center lines for the elements", GH_ParamAccess.list);

            //01
            pManager.AddNumberParameter("Stiffness", "S", "Scale factor for the stiffness of the element", GH_ParamAccess.item, 1);

            //02
            pManager.AddNumberParameter("SlackLength", "SL", "Scale factor for the length of the element, should be inbetween (0 - 1)", GH_ParamAccess.item, 1);

            //03
            pManager.AddNumberParameter("SlackAngle", "SA", "Resting angle inbetween (0 - π). -1 = rest angle is same as initial angle", GH_ParamAccess.item, -1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //00
            pManager.AddGenericParameter("Spline", "E", "Spline element", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Polyline> plines = new List<Polyline>();
            List<Spline> splines = new List<Spline>();
            Spline tempS;

            if (!DA.GetDataList(0, plines)) { return; }

            foreach (Polyline pl in plines)
            {
                if (pl.Length > 0.0001)
                {
                    tempS = new Spline(pl, eSplineType.BendingSpline);
                    splines.Add(tempS);
                }
            }

            DA.SetDataList(0, splines);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return GH_Structures.Properties.Resources.Bar_24x24;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{60816CC5-DCFA-456E-BB87-55E750D39CB0}"); }
        }
    }
}
