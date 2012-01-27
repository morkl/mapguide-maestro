﻿#region Disclaimer / License
// Copyright (C) 2012, Jackie Ng
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
using System.Text;
using Maestro.Shared.UI;
using Maestro.AddIn.GeoRest.Model;
using System.IO;
using System.Xml;

namespace Maestro.AddIn.GeoRest.Services
{
    public class GeoRestService : ServiceBase
    {
        private string _configRootPath;
        private string _geoRestUrl;

        public void Connect(string configRootPath, string geoRestUrl)
        {
            _configRootPath = configRootPath;
            _geoRestUrl = geoRestUrl;
        }

        public string ConfigurationRoot { get { return _configRootPath; } }

        public string GeoRestUrl 
        { 
            get 
            {
                if (!_geoRestUrl.EndsWith("/"))
                    _geoRestUrl += "/";
                return _geoRestUrl; 
            } 
        }

        public FileSystemEntry[] GetEntries(string path)
        {
            var entries = new List<FileSystemEntry>();
            foreach (var dir in Directory.GetDirectories(path))
            {
                entries.Add(new FileSystemEntry() { Name = Path.GetFileName(dir), IsFolder = true });
            }
            foreach (var f in Directory.GetFiles(path))
            {
                entries.Add(new FileSystemEntry() { Name = Path.GetFileName(f), IsFolder = false });
            }
            return entries.ToArray();
        }

        private static XmlNode FindManyTemplateNode(XmlNode representationNode)
        {
            foreach (XmlNode child in representationNode.ChildNodes)
            {
                if (child.Name == "Templates")
                {
                    foreach (XmlNode tpl in child.ChildNodes)
                    {
                        if (tpl.Name == "Many")
                        {
                            return tpl;
                        }
                    }
                }
            }
            return null;
        }

        public RepresentationPreview[] GetRepresentationPreviews(string path)
        {
            if (!path.ToLower().EndsWith("restcfg.xml"))
                throw new InvalidOperationException(Properties.Resources.ErrPreviewNotRestCfg);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode resNode = doc.SelectSingleNode("//RestConfig/Data/Resource");
            if (resNode == null)
                throw new InvalidOperationException(Properties.Resources.ErrRestCfgMissingResourceNode);

            if (resNode.Attributes["uripart"] == null)
                throw new InvalidOperationException(Properties.Resources.ErrRestCfgMissingUriPart);

            string uripart = resNode.Attributes["uripart"].Value;

            // This is the current list of previewable representations
            //
            // - OData
            // - KML (Many template specified)
            // - KMZ (Many template specified)
            // - HTML (Many template specified)
            // 
            // Single template reperesentations cannot be previewed as they require identifying
            // information (ids) that cannot be inferred 

            List<RepresentationPreview> previewableItems = new List<RepresentationPreview>();
            var representations = doc.GetElementsByTagName("Representation");
            foreach (XmlNode repNode in representations)
            {
                var renAttr = repNode.Attributes["renderer"];
                if (renAttr != null)
                {
                    switch (renAttr.Value)
                    {
                        case "Template":
                            {
                                var patAttr = repNode.Attributes["pattern"];
                                if (patAttr != null)
                                {
                                    var many = FindManyTemplateNode(repNode);
                                    if (many != null)
                                    {
                                        switch (patAttr.Value)
                                        {
                                            case ".kml":
                                                {
                                                    previewableItems.Add(new RepresentationPreview()
                                                    {
                                                        Name = string.Format(Properties.Resources.PreviewTypeKmlMany, uripart),
                                                        Url = _geoRestUrl + "data/" + uripart + "/.kml"
                                                    });
                                                }
                                                break;
                                            case ".kmz":
                                                {
                                                    previewableItems.Add(new RepresentationPreview()
                                                    {
                                                        Name = string.Format(Properties.Resources.PreviewTypeKmzMany, uripart),
                                                        Url = _geoRestUrl + "data/" + uripart + "/.kmz"
                                                    });
                                                }
                                                break;
                                            case ".html":
                                                {
                                                    previewableItems.Add(new RepresentationPreview()
                                                    {
                                                        Name = string.Format(Properties.Resources.PreviewTypeHtmlMany, uripart),
                                                        Url = _geoRestUrl + "data/" + uripart + "/.html"
                                                    });
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            break;
                        case "OData":
                            {
                                previewableItems.Add(new RepresentationPreview()
                                {
                                    Name = string.Format(Properties.Resources.PreviewTypeODataRaw, uripart),
                                    Url = _geoRestUrl + "OData.svc/" + uripart
                                });
                            }
                            break;
                    }
                }
            }

            return previewableItems.ToArray();
        }
    }
}