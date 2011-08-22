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
using System.Text;
using OSGeo.MapGuide.MaestroAPI.Exceptions;
using OSGeo.MapGuide.MaestroAPI.Feature;
using GisSharpBlog.NetTopologySuite.IO;

namespace OSGeo.MapGuide.MaestroAPI.Native
{
    public class LocalNativeFeatureReader : FeatureReaderBase
    {
        private MgFeatureReader _reader;
        private WKTReader _mgReader;
        private MgAgfReaderWriter _agfRw;
        private MgWktReaderWriter _wktRw;

        public LocalNativeFeatureReader(MgFeatureReader reader) 
        {
            _reader = reader;
            _mgReader = new WKTReader();
            _agfRw = new MgAgfReaderWriter();
            _wktRw = new MgWktReaderWriter();
            base.ClassDefinition = Utility.ConvertClassDefinition(reader.GetClassDefinition());
            base.FieldCount = reader.GetPropertyCount();
        }

        protected override IFeature ReadNextFeature()
        {
            if (_reader.ReadNext())
                return new LocalNativeFeature(_reader, _mgReader, _agfRw, _wktRw);

            return null;
        }

        public override void Dispose()
        {
            _reader.Dispose();
            _agfRw.Dispose();
            _wktRw.Dispose();
            base.Dispose();
        }

        public override void Close()
        {
            _reader.Close();
        }
    }
}
