using System;
using Rhino;
using Rhino.Commands;

namespace RH_Structures
{
    /** Selects lines and constraint points and builds the structure. Run from the panel's
    *   "Select geometry" button; selection needs a command context to work reliably on Mac.
    */
    public class RHSelectGeometryCommand : Command
    {
        public RHSelectGeometryCommand()
        {
            Instance = this;
        }

        public static RHSelectGeometryCommand Instance
        {
            get; private set;
        }

        public override string EnglishName
        {
            get { return "RelaxSelectGeometry"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            return RHStructuresPlugIn.Instance.Session.SelectGeometry() ? Result.Success : Result.Nothing;
        }
    }
}
