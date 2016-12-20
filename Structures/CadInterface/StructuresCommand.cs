using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Structures
{
    [System.Runtime.InteropServices.Guid("1b854e02-1fcd-485d-beae-d0e2246b9b9e")]
    public class StructuresCommand : Command
    {
        public StructuresCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static StructuresCommand Instance
        {
            get;
            private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "Structures"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            /* Run with UI */
            Dialog dlg = new Dialog();
            dlg.Show();

            /* Run as library */
            //Library_Emulator le = new Library_Emulator();


            return Result.Success;
        }
    }
}
