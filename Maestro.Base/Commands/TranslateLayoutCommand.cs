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

using ICSharpCode.Core;
using Maestro.Base.Editor;
using Maestro.Base.UI;
using OSGeo.MapGuide.ObjectModels;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Maestro.Base.Commands
{
    internal class TranslateLayoutCommand : AbstractMenuCommand
    {
        public override void Run()
        {
            var wb = Workbench.Instance;
            var ed = wb.ActiveEditor;

            if (ed != null)
            {
                var rt = ed.EditorService.GetEditedResource().ResourceType;
                if (rt == ResourceTypes.ApplicationDefinition.ToString() ||
                    rt == ResourceTypes.WebLayout.ToString())
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(ed.GetXmlContent());

                    List<string> tags = new List<string>();
                    if (rt == ResourceTypes.WebLayout.ToString())
                        tags.AddRange(new string[] { "Title", "Tooltip", "Description", "Label", "Prompt" }); //NOXLATE
                    else if (rt == ResourceTypes.ApplicationDefinition.ToString())
                        tags.AddRange(new string[] { "Title", "Label", "Tooltip", "StatusText", "EmptyText" }); //NOXLATE
                    var diag = new LabelLocalizationDialog(doc, tags.ToArray());
                    if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (var ms = new MemoryStream())
                        {
                            doc.Save(ms);
                            ms.Position = 0L;
                            var txml = Encoding.UTF8.GetString(ms.GetBuffer());
                            ed.EditorService.UpdateResourceContent(txml);
                            ((ResourceEditorService)ed.EditorService).ReReadSessionResource();
                            ed.EditorService = ed.EditorService;
                        }
                    }
                }
            }
        }
    }
}