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
using OSGeo.MapGuide.ObjectModels.PrintLayout;
using System.ComponentModel;

namespace Maestro.Editors.PrintLayout
{
    //[ToolboxItem(true)]
    [ToolboxItem(false)]
    internal partial class PrintPagePropertiesSectionCtrl : EditorBindableCollapsiblePanel
    {
        public PrintPagePropertiesSectionCtrl()
        {
            InitializeComponent();
        }

        private IPrintLayout _layout;

        public override void Bind(IEditorService service)
        {
            cmbBgColor.ResetColors();
            service.RegisterCustomNotifier(this);
            _layout = (IPrintLayout)service.GetEditedResource();

            //ColorComboBox requires custom databinding
            cmbBgColor.CurrentColor = Utility.ToColor(_layout.PageProperties.BackgroundColor);
            cmbBgColor.SelectedIndexChanged += (sender, e) =>
            {
                _layout.PageProperties.BackgroundColor = Utility.FromColor(cmbBgColor.CurrentColor);
            };
            _layout.LayoutProperties.PropertyChanged += (sender, e) =>
            {
                OnResourceChanged();
            };

            CheckBoxBinder.BindChecked(chkCustomLogos, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowCustomLogos));
            CheckBoxBinder.BindChecked(chkCustomText, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowCustomText));
            CheckBoxBinder.BindChecked(chkDateTime, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowDateTime));
            CheckBoxBinder.BindChecked(chkLegend, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowLegend));
            CheckBoxBinder.BindChecked(chkNorthArrow, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowNorthArrow));
            CheckBoxBinder.BindChecked(chkScaleBar, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowScaleBar));
            CheckBoxBinder.BindChecked(chkTitle, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowTitle));
            CheckBoxBinder.BindChecked(chkURL, _layout.LayoutProperties, nameof(_layout.LayoutProperties.ShowURL));
        }
    }
}