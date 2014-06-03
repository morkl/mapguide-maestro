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
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Maestro.Editors.Generic;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using Maestro.Editors;

namespace MaestroFsPreview
{
    public partial class MainForm : Form
    {
        private MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IEditorService edSvc)
            : this()
        {
            _edSvc = edSvc;
            localFsPreviewCtrl.Init(edSvc);
        }

        private IEditorService _edSvc;

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var picker = new ResourcePicker(_edSvc.ResourceService, ResourceTypes.FeatureSource.ToString(), ResourcePickerMode.OpenResource))
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    this.FeatureSourceID = picker.ResourceID;
                }
            }
        }

        private IFeatureSource _fs;

        public string FeatureSourceID
        {
            get { return txtFeatureSource.Text; }
            set 
            { 
                txtFeatureSource.Text = value;
                InitPreview();
            }
        }

        private void InitPreview()
        {
            _fs = (IFeatureSource)_edSvc.ResourceService.GetResource(this.FeatureSourceID);
            var caps = _edSvc.FeatureService.GetProviderCapabilities(_fs.Provider);
            localFsPreviewCtrl.SupportsSQL = caps.Connection.SupportsSQL;
            localFsPreviewCtrl.ReloadTree(this.FeatureSourceID, caps);
        }
    }
}
