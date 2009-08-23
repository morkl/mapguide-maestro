#region Disclaimer / License
// Copyright (C) 2009, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
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
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.PackageBuilder;

namespace OSGeo.MapGuide.Maestro.PackageManager
{
    public partial class PackageEditor : Form
    {
        private string m_filename;
        private FormMain m_owner;
        private Dictionary<string, ResourceItem> m_resources;
        private Globalizator.Globalizator m_globalizor = null;

        public PackageEditor(string filename, FormMain owner)
            : this()
        {
            m_filename = filename;
            m_owner = owner;
            m_resources = new Dictionary<string, ResourceItem>();
            ResourceTree.ImageList = owner.ResourceEditorMap.SmallImageList;
            ResourceDataFileList.SmallImageList = ResourceEditors.ShellIcons.ImageList;
        }

        private PackageEditor()
        {
            InitializeComponent();
            m_globalizor = new Globalizator.Globalizator(this);
        }

        public static void EditPackage(ServerConnectionI connection, FormMain owner)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = ".mgp";
            dlg.Filter = "MapGuide Packages (*.mgp)|*.mgp|Zip files (*.zip)|*.zip|All files (*.*)|*.*";
            dlg.FilterIndex = 0;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.Title = Globalizator.Globalizator.Translate("OSGeo.MapGuide.Maestro.PackageManager.PackageEditor", System.Reflection.Assembly.GetExecutingAssembly(), "Select the package to edit");

            if (dlg.ShowDialog(owner) == DialogResult.OK)
            {
                PackageEditor pe = new PackageEditor(dlg.FileName, owner);
                pe.ShowDialog(owner);
            }

        }

        private void PackageEditor_Load(object sender, EventArgs e)
        {
            this.Show();

            try
            {
                m_resources = PackageProgress.ListPackageContents(this, m_owner.CurrentConnection, m_filename);
                if (m_resources == null)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                RebuildTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(m_globalizor.Translate("Failed to read package. Error message was: {0}"), ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            OKBtn.Enabled = true;
        }

        private void RebuildTree()
        {
            ResourceTree.Nodes.Clear();
            TreeNode root = ResourceTree.Nodes.Add("Library://");
            root.ImageIndex = root.SelectedImageIndex = m_owner.ResourceEditorMap.ServerIcon;

            foreach (ResourceItem ri in m_resources.Values)
            {
                string partial = ri.ResourcePath.Substring(root.Text.Length);
                string[] parts = partial.Split('/');
                TreeNode cur = root;
                root.Expand();

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    TreeNode next = null;
                    foreach (TreeNode n in cur.Nodes)
                        if (n.Text == parts[i])
                        {
                            next = n;
                            break;
                        }
                    if (next == null)
                    {
                        cur = cur.Nodes.Add(parts[i]);
                        cur.ImageIndex = cur.SelectedImageIndex = m_owner.ResourceEditorMap.FolderIcon;
                    }
                    else
                        cur = next;

                    cur.Expand();
                }

                if (parts[parts.Length - 1].Trim().Length > 0)
                {
                    TreeNode n = cur.Nodes.Add(parts[parts.Length - 1]);
                    n.Tag = ri;
                    n.ImageIndex = n.SelectedImageIndex = m_owner.ResourceEditorMap.GetImageIndexFromResourceID(ri.ResourcePath);
                }
                else
                    cur.Tag = ri;
            }
        }

        private void MainGroup_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ResourcDataFileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeleteResourceButton.Enabled = EditResourceData.Enabled = SaveResourceData.Enabled = ResourceDataFileList.SelectedItems.Count == 1;
        }

        private void AddResourceData_Click(object sender, EventArgs e)
        {
            if (BrowseResourceDataFile.ShowDialog(this) == DialogResult.OK)
            {
                ResourceDataItem rdi = new ResourceDataItem(
                    System.IO.Path.GetFileName(BrowseResourceDataFile.FileName),
                    "application/octet-stream",
                    BrowseResourceDataFile.FileName,
                    "File");
                rdi.EntryType = EntryTypeEnum.Added;

                ((ResourceItem)ResourceTree.SelectedNode.Tag).Items.Add(rdi);
                RefreshFileList();
            }
        }

        private void RefreshFileList()
        {
            ResourceDataFileList.Items.Clear();
            foreach (ResourceDataItem rdi in ((ResourceItem)ResourceTree.SelectedNode.Tag).Items)
                if (rdi.EntryType != EntryTypeEnum.Deleted)
                {
                    ResourceDataFileList.Items.Add(new ListViewItem(new string[] {
                        rdi.ResourceName, rdi.ContentType, rdi.DataType.ToString(), rdi.Filename
                    }, ResourceEditors.ShellIcons.GetShellIcon(rdi.Filename))).Tag = rdi;
                }
        }

        private void ResourceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DeleteResourceButton.Enabled = MainGroup.Panel2.Enabled = (ResourceTree.SelectedNode != null && ResourceTree.SelectedNode.Tag as ResourceItem != null);

            if (MainGroup.Panel2.Enabled)
            {
                HeaderFilepath.Text = ((ResourceItem)ResourceTree.SelectedNode.Tag).Headerpath;
                ContentFilePath.Text = ((ResourceItem)ResourceTree.SelectedNode.Tag).Contentpath;
                RefreshFileList();
            }
        }

        private void DeleteResourceData_Click(object sender, EventArgs e)
        {
            if (ResourceDataFileList.SelectedItems.Count != 1 || ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem == null)
                return;

            (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).EntryType = EntryTypeEnum.Deleted;
            RefreshFileList();
        }

        private void SaveResourceData_Click(object sender, EventArgs e)
        {
            try
            {
                if (ResourceDataFileList.SelectedItems.Count != 1 || ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem == null)
                    return;

                SaveResourceDataFile.FileName = System.IO.Path.GetFileName((ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).Filename);

                if (SaveResourceDataFile.ShowDialog(this) != DialogResult.OK)
                    return;

                if ((ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).Filename == SaveResourceDataFile.FileName)
                    return;

                if ((ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).EntryType == EntryTypeEnum.Regular)
                {
                    using(ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(m_filename))
                    {
                        int index = FindZipEntry(zipfile, (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).Filename);
                        if (index >= 0)
                            using (System.IO.FileStream fs = new System.IO.FileStream(SaveResourceDataFile.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                                Utility.CopyStream(zipfile.GetInputStream(index), fs);
                    }
                }
                else if ((ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).EntryType == EntryTypeEnum.Added)
                {
                    System.IO.File.Copy((ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).Filename, SaveResourceDataFile.FileName, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(m_globalizor.Translate("An error occured while copying file: {0}"), ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void EditResourceData_Click(object sender, EventArgs e)
        {
            if (ResourceDataFileList.SelectedItems.Count != 1 || ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem == null)
                return;

            EditResourceDataEntry dlg = new EditResourceDataEntry();
            dlg.ResourceName = (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).ResourceName;
            dlg.ContentType = (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).ContentType;
            dlg.DataType = (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).DataType;
            dlg.Filename = (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).Filename;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).ResourceName = dlg.ResourceName;
                (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).ContentType = dlg.ContentType;
                (ResourceDataFileList.SelectedItems[0].Tag as ResourceDataItem).DataType = dlg.DataType;
                RefreshFileList();
            }
        }

        private void ResourceDataFileList_DoubleClick(object sender, EventArgs e)
        {
            EditResourceData_Click(sender, e);
        }

        private void ResourceTree_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode x = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (x == null || x.Tag == null)
                return;

            TreeNode n = ResourceTree.GetNodeAt(ResourceTree.PointToClient(new Point(e.X, e.Y)));
            if (n != null && n.Tag as ResourceItem != null && (n.Tag as ResourceItem).IsFolder)
            {
                //Can't drag onto its' own child
                TreeNode t = n;
                while (t.Parent != null)
                    if (t == x)
                        return;
                    else
                        t = t.Parent;

                x.Remove();
                n.Nodes.Add(x);
            }
            else
                return;
        }

        private void ResourceTree_DragOver(object sender, DragEventArgs e)
        {
            TreeNode x = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (x == null || x.Tag == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            TreeNode n = ResourceTree.GetNodeAt(ResourceTree.PointToClient(new Point(e.X, e.Y)));
            if (n != null && n.Tag as ResourceItem != null && (n.Tag as ResourceItem).IsFolder)
            {
                //Can't drag onto its' own child
                TreeNode t = n;
                while (t.Parent != null)
                    if (t == x)
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    else
                        t = t.Parent;

                e.Effect = DragDropEffects.Move;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void ResourceTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ResourceTree.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void AddFolderButton_Click(object sender, EventArgs e)
        {
            TreeNode n = new TreeNode(m_globalizor.Translate("New folder"), m_owner.ResourceEditorMap.FolderIcon, m_owner.ResourceEditorMap.FolderIcon);
            n.Tag = new ResourceItem("", "", "");
            (n.Tag as ResourceItem).EntryType = EntryTypeEnum.Added;
            (n.Tag as ResourceItem).IsFolder = true;

            if (ResourceTree.SelectedNode == null || ResourceTree.SelectedNode.Parent == null)
                ResourceTree.Nodes[0].Nodes.Add(n);
            else if (ResourceTree.SelectedNode.Tag as ResourceItem != null)
            {
                if ((ResourceTree.SelectedNode.Tag as ResourceItem).IsFolder)
                    ResourceTree.SelectedNode.Nodes.Add(n);
                else if (ResourceTree.SelectedNode.Parent == null)
                    ResourceTree.Nodes[0].Nodes.Add(n);
                else
                    ResourceTree.SelectedNode.Parent.Nodes.Add(n);
            }

            n.EnsureVisible();
            ResourceTree.SelectedNode = n;
            ResourceTree.Focus();
        }

        private void DeleteResourceButton_Click(object sender, EventArgs e)
        {
            if (ResourceTree.SelectedNode == null || ResourceTree.SelectedNode.Tag as ResourceItem == null)
                return;

            if (MessageBox.Show(this, m_globalizor.Translate("Do you want to remove the selected item?"), Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            TreeNode root = ResourceTree.SelectedNode;

            Queue<TreeNode> lst = new Queue<TreeNode>();
            lst.Enqueue(root);

            while (lst.Count > 0)
            {
                TreeNode n = lst.Dequeue();
                foreach (TreeNode tn in n.Nodes)
                    lst.Enqueue(tn);

                if (n.Tag as ResourceItem != null)
                {
                    if ((n.Tag as ResourceItem).EntryType == EntryTypeEnum.Regular)
                    {
                        for (int i = 0; i < (n.Tag as ResourceItem).Items.Count; i++)
                            if ((n.Tag as ResourceItem).Items[i].EntryType == EntryTypeEnum.Added)
                            {
                                (n.Tag as ResourceItem).Items.RemoveAt(i);
                                i--;
                            }
                    }
                    (n.Tag as ResourceItem).EntryType = EntryTypeEnum.Deleted;
                }
            }

            root.Remove();
        }

        private void AddResourceButton_Click(object sender, EventArgs e)
        {
            AddResourceEntry dlg = new AddResourceEntry();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                int imageindex = m_owner.ResourceEditorMap.GetImageIndexFromResourceType(System.IO.Path.GetExtension(dlg.ResourceName).Replace(".", ""));
                TreeNode n = new TreeNode(dlg.ResourceName, imageindex, imageindex);
                ResourceItem i = new ResourceItem("", dlg.HeaderFilepath, dlg.ContentFilepath);
                i.EntryType = EntryTypeEnum.Added;
                n.Tag = i;

                if (ResourceTree.SelectedNode == null || ResourceTree.SelectedNode.Parent == null)
                    ResourceTree.Nodes[0].Nodes.Add(n);
                else if (ResourceTree.SelectedNode.Tag as ResourceItem != null)
                {
                    if ((ResourceTree.SelectedNode.Tag as ResourceItem).IsFolder)
                        ResourceTree.SelectedNode.Nodes.Add(n);
                    else if (ResourceTree.SelectedNode.Parent == null)
                        ResourceTree.Nodes[0].Nodes.Add(n);
                    else
                        ResourceTree.SelectedNode.Parent.Nodes.Add(n);
                }
                
                n.EnsureVisible();
                ResourceTree.SelectedNode = n;
                ResourceTree.Focus();
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            SavePackageDialog.FileName = m_filename;
            if (SavePackageDialog.ShowDialog(this) != DialogResult.OK)
                return;

            //Preparation: Update all resources with the correct path, and build a list with them
            List<ResourceItem> items = new List<ResourceItem>();

            try
            {
                ResourceTree.PathSeparator = "/";
                ResourceTree.Nodes[0].Text = "Library:/";

                Queue<TreeNode> nl = new Queue<TreeNode>();
                foreach (TreeNode n in ResourceTree.Nodes[0].Nodes)
                    nl.Enqueue(n);

                while (nl.Count > 0)
                {
                    TreeNode n = nl.Dequeue();
                    foreach (TreeNode tn in n.Nodes)
                        nl.Enqueue(tn);

                    if (n.Tag as ResourceItem != null && (n.Tag as ResourceItem).EntryType != EntryTypeEnum.Deleted)
                    {
                        ResourceItem ri = new ResourceItem(n.Tag as ResourceItem);
                        ri.ResourcePath = n.FullPath + (ri.IsFolder ? "/" : "");
                        if (string.IsNullOrEmpty(ri.OriginalResourcePath))
                            ri.OriginalResourcePath = ri.ResourcePath;

                        items.Add(ri);
                    }
                }
            }
            finally
            {
                ResourceTree.Nodes[0].Text = "Library://";
            }

            try
            {
                if (PackageProgress.RebuildPackage(this, m_owner.CurrentConnection, m_filename, items, SavePackageDialog.FileName) != DialogResult.OK)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(m_globalizor.Translate("An error occured while building package: {0}"), ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private int FindZipEntry(ICSharpCode.SharpZipLib.Zip.ZipFile file, string path)
        {
            string p = path.Replace('\\', '/');
            foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in file)
                if (ze.Name.Replace('\\', '/').Equals(p))
                    return (int)ze.ZipFileIndex;

            return -1;
        }
    }
}