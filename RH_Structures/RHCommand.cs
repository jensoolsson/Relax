using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace RH_Structures
{
    public class RHStructuresCommand : Command
    {


        public RHStructuresCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RHStructuresCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "RH_Structures"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Panels.OpenPanel(StructuresPanel.PanelId);

            return Result.Success;
        }
    }
}
