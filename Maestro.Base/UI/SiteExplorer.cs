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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core.WinForms;
using Aga.Controls.Tree;
using Maestro.Base.Services;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Resource;
using ICSharpCode.Core;
using Maestro.Base.UI.Preferences;
using Maestro.Shared.UI;
using Maestro.Base.Commands.SiteExplorer;
using Maestro.Base.Commands;
using System.Linq;

namespace Maestro.Base.UI
{
    public partial class SiteExplorer : ViewContentBase, ISiteExplorer
    {
        /// <summary>
        /// Internal use only. Do not invoke directly. Use <see cref="ViewContentManager"/> for that
        /// </summary>
        public SiteExplorer()
        {
            InitializeComponent();
            Application.Idle += new EventHandler(OnIdle);
            ndResource.ToolTipProvider = new RepositoryItemToolTipProvider();
            ndResource.DrawText += new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(OnNodeDrawText);

            this.Title = Properties.Resources.Content_SiteExplorer;
            this.Description = Properties.Resources.Content_SiteExplorer;

            var ts = ToolbarService.CreateToolStripItems("/Maestro/Shell/SiteExplorer/Toolbar", this, true);
            tsSiteExplorer.Items.AddRange(ts);

            _connManager = ServiceRegistry.GetService<ServerConnectionManager>();

            var omgr = ServiceRegistry.GetService<OpenResourceManager>();
            var clip = ServiceRegistry.GetService<ClipboardService>();
            _model = new RepositoryTreeModel(_connManager, trvResources, omgr, clip);
            trvResources.Model = _model;
        }

        void OnIdle(object sender, EventArgs e)
        {
            foreach (var item in tsSiteExplorer.Items)
            {
                if (item is IStatusUpdate)
                    ((IStatusUpdate)item).UpdateStatus();
            }
        }

        public string ConnectionName
        {
            get
            {
                if (trvResources.SelectedNode != null)
                {
                    var item = (RepositoryItem)trvResources.SelectedNode.Tag;
                    return item.ConnectionName;
                }
                else if (trvResources.SelectedNodes != null && trvResources.SelectedNodes.Count > 0)
                {
                    var item = (RepositoryItem)trvResources.SelectedNodes[0].Tag;
                    return item.ConnectionName;
                }
                else if (trvResources.Root.Children.Count == 1)
                {
                    var item = (RepositoryItem)trvResources.Root.Children[0].Tag;
                    return item.ConnectionName;
                }
                return null;
            }
        }

        private RepositoryTreeModel _model;
        private ServerConnectionManager _connManager;

        protected override void OnLoad(EventArgs e)
        {
            
        }

        public override bool AllowUserClose
        {
            get
            {
                return false;
            }
        }

        public override ViewRegion DefaultRegion
        {
            get
            {
                return ViewRegion.Left;
            }
        }

        public void FullRefresh()
        {
            _model.FullRefresh();
            foreach(var node in trvResources.Root.Children)
            {
                node.Expand();
            }
        }

        public void RefreshModel(string connectionName)
        {
            RefreshModel(connectionName, null);
        }

        public void RefreshModel(string connectionName, string resId)
        {
            if (!string.IsNullOrEmpty(resId))
            {
                var rid = new ResourceIdentifier(resId);
                if (!rid.IsFolder)
                    resId = rid.ParentFolder;

                //If this node is not initially expanded, we get NRE on refresh
                ExpandNode(connectionName, resId);
                
                var path = _model.GetPathFromResourceId(connectionName, resId);
                while (path == null)
                {
                    resId = ResourceIdentifier.GetParentFolder(resId);
                    path = _model.GetPathFromResourceId(connectionName, resId);
                }

                var node = trvResources.FindNode(path, true);
                if (node != null)
                {
                    //Walk back up until node has children. We want to refresh from this node down
                    while (node.Children.Count == 0 && node != trvResources.Root)
                        node = node.Parent;
                }
                trvResources.SelectedNode = node;
            }
            _model.Refresh();
            if (!string.IsNullOrEmpty(resId))
            {
                SelectNode(connectionName, resId);
            }
            //trvResources.Root.Children[0].Expand();
        }

        private void trvResources_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNodeAdv node = trvResources.GetNodeAt(new Point(e.X, e.Y));
            if (node != null)
            {
                var item = node.Tag as RepositoryItem;
                if (item != null && !item.IsFolder)
                {
                    var conn = _connManager.GetConnection(RepositoryTreeModel.GetParentConnectionName(item));
                    var resMgr = ServiceRegistry.GetService<OpenResourceManager>();
                    resMgr.Open(item.ResourceId, conn, false, this);
                }
            }
        }

        private void trvResources_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var items = this.SelectedItems;
                if (items.Length > 0)
                {
                    if (items.Length == 1) //Single select
                    {
                        RepositoryItem item = items[0];
                        if (item.IsFolder)
                            MenuService.ShowContextMenu(this, "/Maestro/Shell/SiteExplorer/SelectedFolder", trvResources, e.X, e.Y);
                        else
                            MenuService.ShowContextMenu(this, "/Maestro/Shell/SiteExplorer/SelectedDocument", trvResources, e.X, e.Y);
                    }
                    else //Multi select
                    {
                        //All must be uniform type
                        int folderCount = 0;

                        foreach (var item in items)
                        {
                            if (item.IsFolder)
                                folderCount++;
                        }

                        if (folderCount == 0) //All selected documents
                        {
                            MenuService.ShowContextMenu(this, "/Maestro/Shell/SiteExplorer/SelectedDocuments", trvResources, e.X, e.Y);
                        }
                        else if (folderCount == items.Length) //All selected folders
                        {
                            MenuService.ShowContextMenu(this, "/Maestro/Shell/SiteExplorer/SelectedFolders", trvResources, e.X, e.Y);
                        }
                        else //Mixed selection
                        {
                            MenuService.ShowContextMenu(this, "/Maestro/Shell/SiteExplorer/SelectedMixedResources", trvResources, e.X, e.Y);
                        }
                    }
                }
            }
        }


        public RepositoryItem[] SelectedItems
        {
            get 
            {
                List<RepositoryItem> items = new List<RepositoryItem>();
                if (trvResources.SelectedNodes.Count > 0)
                {
                    foreach (var node in trvResources.SelectedNodes)
                    {
                        items.Add((RepositoryItem)node.Tag);
                    }
                }
                return items.ToArray();
            }
        }

        private void trvResources_Expanding(object sender, TreeViewAdvEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    this.Cursor = Cursors.WaitCursor;
                }));
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
            }
        }

        private void trvResources_Expanded(object sender, TreeViewAdvEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    this.Cursor = Cursors.Default;
                }));
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }


        public void ExpandNode(string connectionName, string folderId)
        {
            if ("Library://".Equals(folderId))
                return;

            var path = _model.GetPathFromResourceId(connectionName, folderId);
            if (path != null)
            {
                var node = trvResources.FindNode(path, true);
                if (node != null)
                {
                    node.IsExpanded = true;
                }
            }
        }

        public void SelectNode(string connectionName, string resourceId)
        {
            var path = _model.GetPathFromResourceId(connectionName, resourceId);
            if (path != null)
            {
                var node = trvResources.FindNode(path, true);
                if (node != null)
                {
                    trvResources.SelectedNode = node;
                }
            }
        }

        public void FlagNode(string connectionName, string resourceId, NodeFlagAction action)
        {
            var path = _model.GetPathFromResourceId(connectionName, resourceId);
            if (path != null)
            {
                var node = trvResources.FindNode(path, true);
                if (node != null)
                {
                    var item = (RepositoryItem)node.Tag;
                    switch (action)
                    {
                        //case NodeFlagAction.IndicateCopy:
                        //case NodeFlagAction.IndicateCut:
                        //    item.IsClipboarded = true;
                        //    break;
                        case NodeFlagAction.HighlightDirty:
                            item.IsDirty = true;
                            break;
                        case NodeFlagAction.HighlightOpen:
                            item.IsOpen = true;
                            break;
                        case NodeFlagAction.None:
                            item.Reset();
                            break;
                    }
                    trvResources.Invalidate();
                }
            }
        }

        void OnNodeDrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
        {
            if (e.Node.Tag == null)
                return;

            var ocolor = PropertyService.Get(ConfigProperties.OpenColor, Color.LightGreen);
            var dcolor = PropertyService.Get(ConfigProperties.DirtyColor, Color.Pink);

            var item = (RepositoryItem)e.Node.Tag;
            var ctx = e.Context;
            if (item.ClipboardState != RepositoryItem.ClipboardAction.None)
            {
                var oldFont = e.Font;
                e.Font = new Font(oldFont.FontFamily, oldFont.Size, oldFont.Style | FontStyle.Italic);
            }
            if (item.IsDirty)
                e.BackgroundBrush = new SolidBrush(dcolor);
            else if (item.IsOpen)
                e.BackgroundBrush = new SolidBrush(ocolor);
        }

        private void trvResources_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var nodes = e.Item as TreeNodeAdv[];
            if (nodes != null)
            {
                List<RepositoryItem> rids = new List<RepositoryItem>();
                foreach (var n in nodes)
                {
                    rids.Add((RepositoryItem)n.Tag);
                }
                trvResources.DoDragDrop(rids.ToArray(), DragDropEffects.All);
            }
        }

        private void trvResources_DragDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(RepositoryItem[])) as RepositoryItem[];
            if (data == null)
            {
                //See if the mouse is currently over a node
                var node = trvResources.GetNodeAt(trvResources.PointToClient(new Point(e.X, e.Y)));
                SiteExplorerDragDropHandler.OnDragDrop(this, e, node);
            }
            else
            {
                //See if the mouse is currently over a node
                var node = trvResources.GetNodeAt(trvResources.PointToClient(new Point(e.X, e.Y)));
                if (node == null)
                    return;

                //Can only drop in a folder
                var item = node.Tag as RepositoryItem;
                if (item != null && item.IsFolder)
                {
                    string connectionName = RepositoryTreeModel.GetParentConnectionName(item);
                    string folderId = item.ResourceId;

                    //I think it's nice to ask for confirmation
                    if (data.Length > 0)
                    {
                        if (!MessageService.AskQuestion(Properties.Resources.ConfirmMove))
                            return;
                    }

                    if (data.First().ConnectionName == connectionName)
                    {
                        string[] folders = MoveResourcesWithinConnection(connectionName, data.Select(x => x.ResourceId).ToArray(), folderId);

                        foreach (var fid in folders)
                        {
                            LoggingService.Info("Refreshing: " + fid + " on " + connectionName); //LOCALIZEME
                            RefreshModel(connectionName, fid);
                        }
                    }
                    else
                    {
                        //TODO: Revisit later
                        MessageService.ShowError("Moving resources between connections is currently not implemented");
                        return;
                    }
                }
            }
        }

        private void trvResources_DragOver(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(RepositoryItem[])) as RepositoryItem[];
            if (data == null)
            {
                SiteExplorerDragDropHandler.OnDragEnter(this, e);
            }
            else
            {
                //See if the mouse is currently over a node
                var node = trvResources.GetNodeAt(trvResources.PointToClient(new Point(e.X, e.Y)));
                if (node == null)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                //Is it a folder?
                var item = node.Tag as RepositoryItem;
                if (item != null && item.IsFolder)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void trvResources_DragEnter(object sender, DragEventArgs e)
        {
            //TODO: There is a whole lot of interesting things we can do here
            // (eg. Create a Feature Source from a dragged SDF file)
        }

        private string [] MoveResourcesWithinConnection(string connectionName, ICollection<string> resIds, string folderId)
        {
            var wb = Workbench.Instance;
            var notMovedToTarget = new List<string>();
            var notMovedFromSource = new List<string>();
            var omgr = ServiceRegistry.GetService<OpenResourceManager>();
            var conn = _connManager.GetConnection(connectionName);

            var dlg = new ProgressDialog();
            var worker = new ProgressDialog.DoBackgroundWork((w, e, args) =>
            {
                LengthyOperationProgressCallBack cb = (sender, cbe) =>
                {
                    w.ReportProgress(cbe.Progress, cbe.StatusMessage);
                };

                var f = (string)args[0];
                var resourceIds = (ICollection<string>)args[1];

                foreach (var r in resourceIds)
                {
                    if (ResourceIdentifier.IsFolderResource(r))
                    {
                        //IMPORTANT: We need to tweak the target resource id
                        //otherwise the content *inside* the source folder is
                        //moved instead of the folder itself!
                        var rid = new ResourceIdentifier(r);
                        var target = folderId + rid.Name + "/";
                        conn.ResourceService.MoveResourceWithReferences(r, target, null, cb);
                    }
                    else
                    {
                        var rid = new ResourceIdentifier(r);
                        var target = folderId + rid.Name + "." + rid.Extension;
                        if (omgr.IsOpen(r))
                        {
                            notMovedFromSource.Add(r);
                            continue;
                        }

                        if (!omgr.IsOpen(target))
                            conn.ResourceService.MoveResourceWithReferences(r, target, null, cb);
                        else
                            notMovedToTarget.Add(r);
                    }
                }

                //Collect affected folders and refresh them
                Dictionary<string, string> folders = new Dictionary<string, string>();
                folders.Add(folderId, folderId);
                foreach (var n in resourceIds)
                {
                    var ri = new ResourceIdentifier(n);
                    var parent = ri.ParentFolder;
                    if (parent != null && !folders.ContainsKey(parent))
                        folders.Add(parent, parent);
                }

                return folders.Keys;
            });

            var affectedFolders = (IEnumerable<string>)dlg.RunOperationAsync(wb, worker, folderId, resIds);

            if (notMovedToTarget.Count > 0 || notMovedFromSource.Count > 0)
            {
                MessageService.ShowMessage(string.Format(
                    Properties.Resources.NotCopiedOrMovedDueToOpenEditors,
                    Environment.NewLine + string.Join(Environment.NewLine, notMovedToTarget.ToArray()) + Environment.NewLine,
                    Environment.NewLine + string.Join(Environment.NewLine, notMovedFromSource.ToArray()) + Environment.NewLine));
            }

            return new List<string>(affectedFolders).ToArray();
        }

        private void trvResources_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                new DeleteSelectedItemsCommand().Run();
            }
            else if (e.KeyCode == Keys.F2)
            {
                new RenameCommand().Run();
            }
        }

        private void trvResources_KeyDown(object sender, KeyEventArgs e)
        {
            //Note: Even though the attached context menu has the shortcuts specified
            //for Cut/Copy/Paste, I'm guessing the TreeViewAdv control is muffling the
            //event. Nevertheless this handler's got it covered and keeping those 
            //shortcuts there is useful as a visual reference, even if they don't work
            //the original way.

            //Note: We handle keydown when intercepting pressing the Control+C/X/V
            //because the keys are not actually released yet.
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.C:
                        new CopyCommand().Run();
                        break;
                    case Keys.X:
                        new CutCommand().Run();
                        break;
                    case Keys.V:
                        new PasteCommand().Run();
                        break;
                }
            }
        }

        public string[] ConnectionNames
        {
            get { return _connManager.GetConnectionNames().ToArray(); }
        }
    }
}
