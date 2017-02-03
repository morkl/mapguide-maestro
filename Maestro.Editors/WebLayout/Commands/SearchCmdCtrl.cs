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
using OSGeo.MapGuide.MaestroAPI.Schema;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using OSGeo.MapGuide.ObjectModels.LayerDefinition;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using OSGeo.MapGuide.ObjectModels.WebLayout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Maestro.Editors.WebLayout.Commands
{
    [ToolboxItem(false)]
    internal partial class SearchCmdCtrl : EditorBase
    {
        public SearchCmdCtrl()
        {
            InitializeComponent();
            grdOutputColumns.AutoGenerateColumns = false;
        }

        private IEditorService _edsvc;
        private IWebLayout _wl;
        private ISearchCommand _cmd;
        private ClassDefinition _cls;

        private bool _init = true;

        public override void Bind(IEditorService service)
        {
            _edsvc = service;
        }

        private BindingList<IResultColumn> _columns = new BindingList<IResultColumn>();

        internal void Bind(ISearchCommand searchCommandType, IEditorService service)
        {
            cmbTargetFrame.DataSource = Enum.GetValues(typeof(TargetType));
            _init = true;
            Bind(service);
            _cmd = searchCommandType;

            _wl = (IWebLayout)_edsvc.GetEditedResource();
            var wlMap = _wl.Map;
            wlMap.PropertyChanged += WeakEventHandler.Wrap<PropertyChangedEventHandler>(OnWebMapLayoutPropertyChanged, (eh) => wlMap.PropertyChanged -= eh);

            LoadLayers();

            if (!string.IsNullOrEmpty(_cmd.Layer))
            {
                foreach (var lyr in _layers)
                {
                    if (lyr.Name == _cmd.Layer)
                    {
                        txtLayer.Text = _cmd.Layer;
                        txtLayer.Tag = lyr.ResourceId;
                    }
                }
            }

            TextBoxBinder.BindText(txtFrame, _cmd, nameof(_cmd.TargetFrame));
            ComboBoxBinder.BindSelectedIndexChanged(cmbTargetFrame, nameof(cmbTargetFrame.SelectedItem), _cmd, nameof(_cmd.Target));
            TextBoxBinder.BindText(txtLayer, _cmd, nameof(_cmd.Layer));
            TextBoxBinder.BindText(txtFilter, _cmd, nameof(_cmd.Filter));
            TextBoxBinder.BindText(txtPrompt, _cmd, nameof(_cmd.Prompt));

            NumericBinder.BindValueChanged(numLimit, _cmd, nameof(_cmd.MatchLimit));

            UpdateColumns();
            foreach (var col in _cmd.ResultColumns.Column)
            {
                _columns.Add(col);
            }
            grdOutputColumns.DataSource = _columns;
            _columns.AddingNew += OnAddingNewColumn;
            _columns.ListChanged += OnColumnsChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            _init = false;
        }

        private void OnAddingNewColumn(object sender, AddingNewEventArgs e)
        {
            if (_cls == null)
                return;

            e.NewObject = _cmd.ResultColumns.CreateColumn("MyProperty", _cls.Properties[0].Name); //NOXLATE
        }

        private void OnColumnsChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    {
                        _cmd.ResultColumns.AddResultColumn(_columns[e.NewIndex]);
                    }
                    break;

                case ListChangedType.Reset:
                    {
                        if (_cleanup)
                            return;

                        _cmd.ResultColumns.Clear();
                    }
                    break;
            }
        }

        private bool _cleanup = false;

        protected override void UnsubscribeEventHandlers()
        {
            _cleanup = true;

            if (_wl != null && _wl.Map != null)
                _wl.Map.PropertyChanged -= OnWebMapLayoutPropertyChanged;

            _columns.AddingNew -= OnAddingNewColumn;
            _columns.ListChanged -= OnColumnsChanged;

            base.UnsubscribeEventHandlers();
        }

        private System.Collections.Generic.List<IMapLayer> _layers;

        private void LoadLayers()
        {
            var map = (IMapDefinition)_edsvc.CurrentConnection.ResourceService.GetResource(_wl.Map.ResourceId);

            _layers = new System.Collections.Generic.List<IMapLayer>(map.MapLayer);
        }

        private void OnWebMapLayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_wl.Map.ResourceId))
            {
                LoadLayers();
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (txtLayer.Tag != null)
            {
                var resSvc = _edsvc.CurrentConnection.ResourceService;
                var ldf = (ILayerDefinition)resSvc.GetResource(txtLayer.Tag.ToString());
                var vl = (IVectorLayerDefinition)ldf.SubLayer;

                var fs = (IFeatureSource)resSvc.GetResource(vl.ResourceId);

                string expr = _edsvc.EditExpression(txtFilter.Text, _cls, fs.Provider, vl.ResourceId, ExpressionEditorMode.Filter, false);
                if (expr != null)
                {
                    txtFilter.Text = expr;
                }
            }
            else
            {
                MessageBox.Show(Strings.SelectLayerFirst);
            }
        }

        private void UpdateColumns()
        {
            if (txtLayer.Tag != null)
            {
                _columns.Clear();

                var conn = _edsvc.CurrentConnection;
                var ldf = (ILayerDefinition)conn.ResourceService.GetResource(txtLayer.Tag.ToString());
                var vl = (IVectorLayerDefinition)ldf.SubLayer;

                _cls = conn.FeatureService.GetClassDefinition(vl.ResourceId, vl.FeatureName);

                COL_PROPERTY.DisplayMember = "Name";
                COL_PROPERTY.DataSource = _cls.Properties;
            }
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            if (_init)
                return;

            UpdateColumns();
        }

        private class LayerItem
        {
            public string ResourceId { get; set; }

            public string Name { get; set; }
        }

        private void btnBrowseLayer_Click(object sender, EventArgs e)
        {
            var layers = new System.Collections.Generic.List<LayerItem>();
            foreach (var l in _layers)
            {
                layers.Add(new LayerItem() { Name = l.Name, ResourceId = l.ResourceId });
            }
            var layer = GenericItemSelectionDialog.SelectItem(Strings.SelectLayer, Strings.SelectLayer, layers.ToArray(), "Name", "Name");
            if (layer != null)
            {
                txtLayer.Tag = layer.ResourceId;
                txtLayer.Text = layer.Name;
            }
        }
    }
}