﻿#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// http://trac.osgeo.org/mapguide/wiki/maestro, jumpinjackie@gmail.com
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using ICSharpCode.Core;
using Maestro.Editors;
using Maestro.Editors.FeatureSource;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591

namespace Maestro.Base.Editor
{
    /// <summary>
    /// A specialized editor for Feature Source resources.
    /// </summary>
    /// <remarks>
    /// Although public, this class is undocumented and reserved for internal use by built-in Maestro AddIns
    /// </remarks>
    public partial class FeatureSourceEditor : EditorContentBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FeatureSourceEditor()
        {
            InitializeComponent();
        }

        private IResource _res;
        private IEditorService _edsvc;
        private bool _init = false;

        /// <summary>
        /// Binds the specified resource to this control. This effectively initializes
        /// all the fields in this control and sets up databinding on all fields. All
        /// subclasses *must* override this method.
        ///
        /// Also note that this method may be called more than once (e.g. Returning from
        /// and XML edit of this resource). Thus subclasses must take this scenario into
        /// account when implementing
        /// </summary>
        /// <param name="service">The editor service</param>
        protected override void Bind(IEditorService service)
        {
            if (!_init)
            {
                _edsvc = service;
                _res = _edsvc.GetEditedResource();
                _init = true;
            }

            panelBody.Controls.Clear();

            var fsEditor = new FeatureSourceEditorCtrl();
            fsEditor.Dock = DockStyle.Fill;
            panelBody.Controls.Add(fsEditor);

            var fsOpts = new FsEditorOptionPanel();
            fsOpts.Dock = DockStyle.Top;
            fsOpts.Bind(_edsvc);
            fsEditor.Controls.Add(fsOpts);

            fsEditor.Bind(_edsvc);
        }

        protected override void OnBeforeSave(object sender, CancelEventArgs e)
        {
            System.Collections.Generic.List<string> affectedMapDefinitions = new System.Collections.Generic.List<string>();
            var resSvc = _edsvc.CurrentConnection.ResourceService;
            var refs = resSvc.EnumerateResourceReferences(_edsvc.ResourceID);
            foreach (var r in refs.ResourceId)
            {
                ResourceIdentifier rid = new ResourceIdentifier(r);
                if (rid.ResourceType == ResourceTypes.LayerDefinition.ToString())
                {
                    var lrefs = resSvc.EnumerateResourceReferences(r);
                    foreach (var lr in lrefs.ResourceId)
                    {
                        ResourceIdentifier rid2 = new ResourceIdentifier(lr);
                        if (rid2.ResourceType == ResourceTypes.MapDefinition.ToString())
                        {
                            var mdf = (IMapDefinition)resSvc.GetResource(lr);
                            if (mdf.BaseMap != null)
                            {
                                foreach (var blg in mdf.BaseMap.BaseMapLayerGroups)
                                {
                                    foreach (var bl in blg.BaseMapLayer)
                                    {
                                        if (bl.ResourceId.Equals(r))
                                        {
                                            affectedMapDefinitions.Add(r);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (affectedMapDefinitions.Count > 0)
            {
                if (!MessageService.AskQuestionFormatted(Strings.Confirm, Strings.ConfirmBaseMapInvalidationFeatureSourceSave, string.Join(Environment.NewLine, affectedMapDefinitions.ToArray()) + Environment.NewLine))
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnBeforeSave(sender, e);
        }

        /// <summary>
        /// Gets whether this resource can be profiled
        /// </summary>
        public override bool CanProfile
        {
            get
            {
                return true;
            }
        }

        public override Icon ViewIcon
        {
            get
            {
                return Properties.Resources.icon_featuresource;
            }
        }
    }
}