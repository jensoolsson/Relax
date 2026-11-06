using System;
using Rhino;
using Rhino.Commands;

namespace RH_Structures
{
    /** Adds the current (relaxed) element lines to the document. Run from the panel's "Bake"
    *   button, so the added objects get an undo record.
    */
    public class RHBakeCommand : Command
    {
        public RHBakeCommand()
        {
            Instance = this;
        }

        public static RHBakeCommand Instance
        {
            get; private set;
        }

        public override string EnglishName
        {
            get { return "RelaxBake"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (!RHStructuresPlugIn.Instance.Session.HasStructure)
            {
                RhinoApp.WriteLine("Relax: select geometry first.");
                return Result.Nothing;
            }

            RHStructuresPlugIn.Instance.Session.Bake(doc);
            return Result.Success;
        }
    }
}
