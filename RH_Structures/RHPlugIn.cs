using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.PlugIns;
using Rhino.UI;

namespace RH_Structures
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class RHStructuresPlugIn : Rhino.PlugIns.PlugIn
    {
        private const string PanelIconResource = "RH_Structures.Resources.Relax.png";

        private RelaxSession _session;

        public RHStructuresPlugIn()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the RhinoPluginTemplatePlugIn plug-in.</summary>
        public static RHStructuresPlugIn Instance
        {
            get; private set;
        }

        ///<summary>Analysis state shared by the panel and the commands.</summary>
        public RelaxSession Session
        {
            get
            {
                if (_session == null)
                    _session = new RelaxSession();
                return _session;
            }
        }

        //Load at start-up so a docked Relax panel can be restored.
        public override PlugInLoadTime LoadTime
        {
            get { return PlugInLoadTime.AtStartup; }
        }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            Panels.RegisterPanel(this, typeof(StructuresPanel), "Relax", GetType().Assembly, PanelIconResource, PanelType.System);

            RhinoDoc.CloseDocument += OnCloseDocument;

            return LoadReturnCode.Success;
        }

        protected override void OnShutdown()
        {
            RhinoDoc.CloseDocument -= OnCloseDocument;

            if (_session != null)
                _session.Dispose();
            _session = null;

            base.OnShutdown();
        }

        //Do not keep relaxing a model whose document has been closed.
        private void OnCloseDocument(object sender, DocumentEventArgs e)
        {
            if (_session != null)
                _session.Stop();
        }
    }
}
