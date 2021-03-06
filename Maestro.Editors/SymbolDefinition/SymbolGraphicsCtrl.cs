﻿#region Disclaimer / License

// Copyright (C) 2011, Jackie Ng
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

using Maestro.Editors.Common;
using Maestro.Editors.SymbolDefinition.GraphicsEditors;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.ObjectModels.SymbolDefinition;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Maestro.Editors.SymbolDefinition
{
    [ToolboxItem(false)]
    internal partial class SymbolGraphicsCtrl : EditorBindableCollapsiblePanel
    {
        public SymbolGraphicsCtrl()
        {
            InitializeComponent();
        }

        private ISimpleSymbolDefinition _sym;
        private IServerConnection _conn;

        public override void Bind(IEditorService service)
        {
            service.RegisterCustomNotifier(this);
            _sym = (ISimpleSymbolDefinition)service.GetEditedResource();
            _conn = service.CurrentConnection;
            lstGraphics.Items.Clear();

            foreach (var g in _sym.Graphics)
            {
                AddGraphicsItem(g);
            }
        }

        private void AddGraphicsItem(IGraphicBase g)
        {
            var text = g as ITextGraphic;
            var img = g as IImageGraphic;
            var path = g as IPathGraphic;

            var li = new ListViewItem();
            li.Tag = g;
            if (text != null)
            {
                li.Text = Strings.SymbolGraphicsTextPlaceholder;
                li.ImageIndex = 0;
            }
            else if (path != null)
            {
                li.Text = Strings.SymbolGraphicsPathPlaceholder;
                li.ImageIndex = 1;
            }
            else if (img != null)
            {
                li.Text = Strings.SymbolGraphicsImagePlaceholder;
                li.ImageIndex = 2;
            }

            lstGraphics.Items.Add(li);
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = _sym.CreateTextGraphics();
            AddGraphicsItem(text);
            _sym.AddGraphics(text);
            this.OnResourceChanged();
            new TextDialog(this, _sym, text).ShowDialog();
        }

        private void pathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var path = _sym.CreatePathGraphics();
            AddGraphicsItem(path);
            _sym.AddGraphics(path);
            this.OnResourceChanged();
            new PathDialog(this, _sym, path).ShowDialog();
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var img = _sym.CreateImageGraphics();
            AddGraphicsItem(img);
            _sym.AddGraphics(img);
            this.OnResourceChanged();
            new ImageDialog(this, _conn, _sym, img).ShowDialog();
        }

        private void lstGraphics_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = btnDelete.Enabled = (lstGraphics.SelectedItems.Count == 1);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstGraphics.SelectedItems.Count == 1)
            {
                var li = lstGraphics.SelectedItems[0];
                var g = (IGraphicBase)li.Tag;

                var text = g as ITextGraphic;
                var img = g as IImageGraphic;
                var path = g as IPathGraphic;

                if (text != null)
                {
                    new TextDialog(this, _sym, text).ShowDialog();
                }
                else if (path != null)
                {
                    new PathDialog(this, _sym, path).ShowDialog();
                }
                else if (img != null)
                {
                    new ImageDialog(this, _conn, _sym, img).ShowDialog();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstGraphics.SelectedItems.Count == 1)
            {
                var li = lstGraphics.SelectedItems[0];
                var g = (IGraphicBase)li.Tag;
                lstGraphics.Items.Remove(li);
                _sym.RemoveGraphics(g);
                this.OnResourceChanged();
            }
        }
    }
}