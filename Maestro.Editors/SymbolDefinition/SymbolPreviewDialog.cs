﻿#region Disclaimer / License

// Copyright (C) 2012, Jackie Ng
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

using System.Drawing;
using System.Windows.Forms;

namespace Maestro.Editors.SymbolDefinition
{
    /// <summary>
    /// Displays an image of a symbol definition preview
    /// </summary>
    public partial class SymbolPreviewDialog : Form
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public SymbolPreviewDialog(Image img)
        {
            InitializeComponent();
            pictureBox1.Image = img;
        }
    }
}