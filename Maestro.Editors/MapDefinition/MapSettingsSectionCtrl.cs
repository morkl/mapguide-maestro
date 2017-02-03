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

using Maestro.Editors.Common;
using Maestro.Shared.UI;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using OSGeo.MapGuide.ObjectModels.TileSetDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Maestro.Editors.MapDefinition
{
    //[ToolboxItem(true)]
    [ToolboxItem(false)]
    internal partial class MapSettingsSectionCtrl : EditorBindableCollapsiblePanel
    {
        public MapSettingsSectionCtrl()
        {
            InitializeComponent();
        }

        private IMapDefinition _map;

        private const string META_START = "<MapDescription>";
        private const string META_END = "</MapDescription>";

        private IEditorService _service;
        private bool _updatingExtents = false;

        public override void Bind(IEditorService service)
        {
            cmbBackgroundColor.ResetColors();

            _service = service;
            _service.RegisterCustomNotifier(this);
            _map = (IMapDefinition)service.GetEditedResource();

            var mdf3 = _map as IMapDefinition3;
            if (mdf3 == null)
            {
                pnlTileSource.Visible = false;
            }
            else
            {
                switch(mdf3.TileSourceType)
                {
                    case TileSourceType.External:
                        rdExternal.Checked = true;
                        break;
                    case TileSourceType.Inline:
                        rdInline.Checked = true;
                        break;
                    default: //Default to none
                        rdNone.Checked = true;
                        break;
                }
            }

            var bmeta = new Binding("Text", _map, "Metadata");
            bmeta.Parse += (sender, e) =>
            {
                e.Value = META_START + e.Value + META_END;
            };
            bmeta.Format += (sender, e) =>
            {
                if (e.Value != null)
                {
                    var str = e.Value.ToString();
                    if (str.StartsWith(META_START) && str.EndsWith(META_END))
                    {
                        e.Value = str.Substring(META_START.Length, str.Length - (META_START.Length + META_END.Length));
                    }
                }
            };
            TextBoxBinder.BindText(txtDescription, bmeta);
            TextBoxBinder.BindText(txtCoordinateSystem, _map, "CoordinateSystem");

            //ColorComboBox requires custom databinding
            cmbBackgroundColor.CurrentColor = Utility.ToColor(_map.BackgroundColor);
            cmbBackgroundColor.SelectedIndexChanged += (sender, e) =>
            {
                _map.BackgroundColor = Utility.FromColor(cmbBackgroundColor.CurrentColor);
            };
            PropertyChangedEventHandler mapChanged = (sender, e) =>
            {
                if (e.PropertyName == nameof(_map.BackgroundColor))
                {
                    cmbBackgroundColor.CurrentColor = Utility.ToColor(_map.BackgroundColor);
                }
                else if (e.PropertyName == nameof(_map.Extents))
                {
                    UpdateExtentsFromMap();
                }
            };
            _map.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(mapChanged, (eh) => _map.PropertyChanged -= eh);

            txtLowerX.Text = _map.Extents.MinX.ToString(CultureInfo.InvariantCulture);
            txtLowerY.Text = _map.Extents.MinY.ToString(CultureInfo.InvariantCulture);
            txtUpperX.Text = _map.Extents.MaxX.ToString(CultureInfo.InvariantCulture);
            txtUpperY.Text = _map.Extents.MaxY.ToString(CultureInfo.InvariantCulture);

            txtLowerX.TextChanged += (s, e) =>
            {
                if (_updatingExtents)
                    return;

                if (txtLowerX.Text.EndsWith(".")) //Maybe typing in decimals atm
                    return;

                double d;
                if (double.TryParse(txtLowerX.Text, out d))
                    _map.Extents.MinX = d;
            };

            txtLowerY.TextChanged += (s, e) =>
            {
                if (_updatingExtents)
                    return;

                if (txtLowerY.Text.EndsWith(".")) //Maybe typing in decimals atm
                    return;

                double d;
                if (double.TryParse(txtLowerY.Text, out d))
                    _map.Extents.MinY = d;
            };

            txtUpperX.TextChanged += (s, e) =>
            {
                if (_updatingExtents)
                    return;

                if (txtUpperX.Text.EndsWith(".")) //Maybe typing in decimals atm
                    return;

                double d;
                if (double.TryParse(txtUpperX.Text, out d))
                    _map.Extents.MaxX = d;
            };

            txtUpperY.TextChanged += (s, e) =>
            {
                if (_updatingExtents)
                    return;

                if (txtUpperY.Text.EndsWith(".")) //Maybe typing in decimals atm
                    return;

                double d;
                if (double.TryParse(txtUpperY.Text, out d))
                    _map.Extents.MaxY = d;
            };

            PropertyChangedEventHandler extChange = (sender, e) =>
            {
                UpdateExtentsFromMap();
                OnResourceChanged();
            };
            var ext = _map.Extents;
            ext.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(extChange, (eh) => ext.PropertyChanged -= eh);
        }

        private void UpdateExtentsFromMap()
        {
            _updatingExtents = true;
            try
            {
                txtLowerX.Text = _map.Extents.MinX.ToString(CultureInfo.InvariantCulture);
                txtLowerY.Text = _map.Extents.MinY.ToString(CultureInfo.InvariantCulture);
                txtUpperX.Text = _map.Extents.MaxX.ToString(CultureInfo.InvariantCulture);
                txtUpperY.Text = _map.Extents.MaxY.ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                _updatingExtents = false;
            }
        }

        private void btnPickCs_Click(object sender, EventArgs e)
        {
            string cs = _service.GetCoordinateSystem();
            if (!string.IsNullOrEmpty(cs))
            {
                string oldCs = txtCoordinateSystem.Text;
                txtCoordinateSystem.Text = cs;
                if (_map.Extents != null && !string.IsNullOrEmpty(oldCs) && !string.IsNullOrEmpty(cs) && !oldCs.Equals(cs))
                {
                    //Transform current extents
                    try
                    {
                        var trans = _service.CurrentConnection.CoordinateSystemCatalog.CreateTransform(oldCs, cs);

                        var oldExt = _map.Extents;

                        double llx;
                        double lly;
                        double urx;
                        double ury;

                        trans.Transform(oldExt.MinX, oldExt.MinY, out llx, out lly);
                        trans.Transform(oldExt.MaxX, oldExt.MaxY, out urx, out ury);

                        _map.Extents.MinX = llx;
                        _map.Extents.MinY = lly;
                        _map.Extents.MaxX = urx;
                        _map.Extents.MaxY = ury;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(Strings.CoordinateTransformationFailed, ex.Message));
                    }
                }
            }
        }

        private IEnumerable<string> CollectLayerIds()
        {
            HashSet<string> ids = new HashSet<string>();
            foreach (var layer in _map.MapLayer)
            {
                ids.Add(layer.ResourceId);
            }
            IMapDefinition3 mdf3 = _map as IMapDefinition3;
            if (_map.BaseMap != null)
            {
                foreach (var grp in _map.BaseMap.BaseMapLayerGroups)
                {
                    foreach (var layer in grp.BaseMapLayer)
                    {
                        ids.Add(layer.ResourceId);
                    }
                }
            }
            else if (mdf3 != null && !string.IsNullOrEmpty(mdf3.TileSetDefinitionID))
            {
                var tsd = (ITileSetDefinition)_service.CurrentConnection.ResourceService.GetResource(mdf3.TileSetDefinitionID);
                foreach (var grp in tsd.BaseMapLayerGroups)
                {
                    foreach (var layer in grp.BaseMapLayer)
                    {
                        ids.Add(layer.ResourceId);
                    }
                }
            }
            return ids;
        }

        private void btnSetZoom_Click(object sender, EventArgs e)
        {
            var diag = new ExtentCalculationDialog(_service.CurrentConnection, _map.CoordinateSystem, CollectLayerIds);
            if (diag.ShowDialog() == DialogResult.OK)
            {
                var env = diag.Extents;
                if (env != null)
                {
                    _map.SetExtents(env.MinX, env.MinY, env.MaxX, env.MaxY);
                }
                else
                {
                    MessageBox.Show(Strings.ErrorMapExtentCalculationFailed, Strings.TitleError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateTileSourceType()
        {
            var mdf3 = _map as IMapDefinition3;
            if (mdf3 != null)
            {
                if (rdExternal.Checked)
                    mdf3.TileSourceType = TileSourceType.External;
                else if (rdInline.Checked)
                    mdf3.TileSourceType = TileSourceType.Inline;
                else
                    mdf3.TileSourceType = TileSourceType.None;
            }
        }

        private void rdInline_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTileSourceType();
        }

        private void rdExternal_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTileSourceType();
        }

        private void rdNone_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTileSourceType();
        }
    }
}