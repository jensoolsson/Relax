using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GH_Structures
{
    public class ARD_StructuresInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "GHStructures";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("f708249d-4721-49a6-8c88-e3142075aff5");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Foster and Partners";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
