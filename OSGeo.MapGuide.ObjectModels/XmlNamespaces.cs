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


namespace OSGeo.MapGuide.ObjectModels
{
    /// <summary>
    /// A set of XML namespaces commonly used in FDO XML documents
    /// </summary>
    public static class XmlNamespaces
    {
        /// <summary>
        /// XML Schema namespace
        /// </summary>
        public const string XS = "http://www.w3.org/2001/XMLSchema"; //NOXLATE

        /// <summary>
        /// XML Schema Instance namespace
        /// </summary>
        public const string XSI = "http://www.w3.org/2001/XMLSchema-instance"; //NOXLATE

        /// <summary>
        /// XLink namespace
        /// </summary>
        public const string XLINK = "http://www.w3.org/1999/xlink"; //NOXLATE

        /// <summary>
        /// GML (Geography Markup Language) namespace
        /// </summary>
        public const string GML = "http://www.opengis.net/gml"; //NOXLATE

        /// <summary>
        /// FDO (Feature Data Objects) namespace
        /// </summary>
        public const string FDO = "http://fdo.osgeo.org/schemas"; //NOXLATE

        /// <summary>
        /// FDS namespace
        /// </summary>
        public const string FDS = "http://fdo.osgeo.org/schemas/fds"; //NOXLATE
    }
}