#region Disclaimer / License

// Copyright (C) 2017, Jackie Ng
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    public static class ReflectionExtensions
    {
        public static bool IsAssignableFrom(this Type type, Type otherType) => type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
    }

    /// <summary>
    /// Shim interface
    /// </summary>
    public interface ICloneable
    {
        /// <summary>
        /// Clones this instance
        /// </summary>
        /// <returns>The clone</returns>
        object Clone();
    }
}

namespace OSGeo.MapGuide.ObjectModels
{
    /// <summary>
    /// Describes a font
    /// </summary>
    public struct FontInfo
    {
        public string Name;

        public bool Italic;

        public bool Bold;

        public bool Underline;
    }    

    /// <summary>
    /// Describes a color
    /// </summary>
    public struct ColorInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public ColorInfo(byte r, byte g, byte b, byte a = 0)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        /// <summary>
        /// Alpha
        /// </summary>
        public byte A { get; }

        /// <summary>
        /// Red
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Green
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Blue
        /// </summary>
        public byte B { get; }

        public static ColorInfo White => new ColorInfo(255, 255, 255);

        public static ColorInfo Black => new ColorInfo(0, 0, 0);

        public static ColorInfo Color() => new ColorInfo();

        /// <summary>
        /// Creates a color instance
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <returns>The color instance</returns>
        public static ColorInfo FromArgb(byte r, byte g, byte b)
        {
            return new ColorInfo(r, g, b);
        }

        /// <summary>
        /// Creates a color instance
        /// </summary>
        /// <param name="a">alpha</param> 
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <returns>The color instance</returns>
        public static ColorInfo FromArgb(byte a, byte r, byte g, byte b)
        {
            return new ColorInfo(r, g, b, a);
        }

        public static bool operator == (ColorInfo left, ColorInfo right)
        {
            return left.A == right.A 
                && left.R == right.R 
                && left.G == right.G
                && left.B == right.B;
        }

        public static bool operator != (ColorInfo left, ColorInfo right)
        {
            return !(left.A == right.A 
                && left.R == right.R 
                && left.G == right.G
                && left.B == right.B);
        }
    }
}
