﻿#region Disclaimer / License

// Copyright (C) 2014, Jackie Ng
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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
#pragma warning disable 1591, 0114, 0108, 0114, 0108
namespace OSGeo.MapGuide.ObjectModels.Common
{
    partial class ResourceSecurityTypeGroups
    {
        internal ResourceSecurityTypeGroups()
        {
        }
    }

    partial class ResourceSecurityTypeUsers
    {
        internal ResourceSecurityTypeUsers()
        {
        }
    }

    partial class ResourceDocumentHeaderType
    {
        public static ResourceDocumentHeaderType CreateDefault()
        {
            return new ResourceDocumentHeaderType()
            {
                Security = new ResourceSecurityType() { Inherited = true }
            };
        }


        /// <summary>
        /// For internal use only. Made public to satisfy serialization requirements. Use <see cref="M:OSGeo.MapGuide.ObjectModels.ResourceDocumentHeaderType.CreateDefault"/> to
        /// create new instances
        /// </summary>
        public ResourceDocumentHeaderType()
        {
        }

        //Required for saving/updating

        [XmlAttribute("noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")] //NOXLATE
        public string ValidatingSchema
        {
            get { return "ResourceDocumentHeader-1.0.0.xsd"; } //NOXLATE
            set { }
        }
    }

    partial class ResourceFolderHeaderType
    {
        public static ResourceFolderHeaderType CreateDefault()
        {
            return new ResourceFolderHeaderType()
            {
                Security = new ResourceSecurityType() { Inherited = true }
            };
        }

        /// <summary>
        /// For internal use only. Made public to satisfy serialization requirements. Use <see cref="M:OSGeo.MapGuide.ObjectModels.ResourceDocumentHeaderType.CreateDefault"/> to
        /// create new instances
        /// </summary>
        public ResourceFolderHeaderType()
        {
        }

        //Required for saving/updating

        [XmlAttribute("noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")] //NOXLATE
        public string ValidatingSchema
        {
            get { return "ResourceFolderHeader-1.0.0.xsd"; } //NOXLATE
            set { }
        }
    }

    partial class ResourceDocumentHeaderTypeMetadataSimple
    {
        internal ResourceDocumentHeaderTypeMetadataSimple()
        {
        }
    }

    partial class ResourceDocumentHeaderTypeMetadata
    {
        internal ResourceDocumentHeaderTypeMetadata()
        {
        }

        /// <summary>
        /// Returns a <see cref="NameValueCollection"/> of all the metadata properties
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetProperties() => this.Simple.Property.ToDictionary(p => p.Name, p => p.Value);

        /// <summary>
        /// Sets a metadata property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetProperty(string name, string value)
        {
            foreach (var val in this.Simple.Property)
            {
                if (val.Name == name)
                {
                    val.Value = value;
                    return;
                }
            }

            this.Simple.Property.Add(new ResourceDocumentHeaderTypeMetadataSimpleProperty()
            {
                Name = name,
                Value = value
            });
        }

        /// <summary>
        /// Applies the specified set of properties to this instance
        /// </summary>
        /// <param name="values"></param>
        public void ApplyProperties(IDictionary<string, string> values)
        {
            var dict = GetProperties();
            foreach (var kvp in values)
            {
                dict[kvp.Key] = kvp.Value;
            }

            this.Simple.Property.Clear();
            foreach (var kvp in dict)
            {
                this.Simple.Property.Add(new ResourceDocumentHeaderTypeMetadataSimpleProperty()
                {
                    Name = kvp.Key,
                    Value = kvp.Value
                });
            }
        }
    }
}