﻿#region Disclaimer / License

// Copyright (C) 2014, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
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

using OSGeo.MapGuide.ObjectModels.Common;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Maestro.Editors.FeatureSource
{
    /// <summary>
    /// A dialog that displays spatial context information
    /// </summary>
    public partial class SpatialContextInfoDialog : Form
    {
        private SpatialContextInfoDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new instance of SpatialContextInfoDialog
        /// </summary>
        /// <param name="sc">The spatial context to display</param>
        public SpatialContextInfoDialog(IFdoSpatialContext sc)
            : this()
        {
            txtCSName.Text = sc.CoordinateSystemName;
            txtCSWkt.Text = sc.CoordinateSystemWkt;
            txtDescription.Text = sc.Description;
            txtExtentType.Text = sc.ExtentType.ToString();
            if (sc.ExtentType == FdoSpatialContextListSpatialContextExtentType.Static)
            {
                var bounds = sc.Extent;
                txtMaxX.Text = bounds.MaxX.ToString(CultureInfo.InvariantCulture);
                txtMaxY.Text = bounds.MaxY.ToString(CultureInfo.InvariantCulture);
                txtMinX.Text = bounds.MinX.ToString(CultureInfo.InvariantCulture);
                txtMinY.Text = bounds.MinY.ToString(CultureInfo.InvariantCulture);
            }
            txtName.Text = sc.Name;
            txtXYTolerance.Text = sc.XYTolerance.ToString(CultureInfo.InvariantCulture);
            txtZTolerance.Text = sc.ZTolerance.ToString(CultureInfo.InvariantCulture);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}