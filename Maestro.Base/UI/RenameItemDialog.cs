﻿#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
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

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Maestro.Base.UI
{
    internal partial class RenameItemDialog : Form
    {
        private RenameItemDialog()
        {
            InitializeComponent();
        }

        public RenameItemDialog(string oldName, IEnumerable<string> names)
            : this()
        {
            txtOld.Text = oldName;
            existingNames.AddRange(names);
            txtNew.Text = oldName;
            txtNew_MouseLeave(this, EventArgs.Empty);
        }

        protected override void OnLoad(EventArgs e)
        {
            //NOTE: This only can work when txtNew is the first item in the tab order.
            txtNew.Focus();
            txtNew.SelectionStart = txtNew.Text.Length;
            txtNew.SelectionLength = 0;
        }

        public string NewName
        {
            get { return txtNew.Text; }
        }

        public bool UpdateReferences
        {
            get { return chkUpdateRefs.Checked; }
        }

        public bool Overwrite
        {
            get { return chkOverwrite.Checked; }
        }

        private List<string> existingNames = new List<string>();

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void txtNew_TextChanged(object sender, EventArgs e)
        {
            btnRename.Enabled = (txtNew.Text.Length > 0) && (txtNew.Text != txtOld.Text);
        }

        private void txtNew_MouseLeave(object sender, EventArgs e)
        {
            lblExists.Visible = existingNames.Contains(txtNew.Text);
        }

        private void chkUpdateRefs_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUpdateRefs.Checked)
                chkOverwrite.Checked = true;
        }
    }
}