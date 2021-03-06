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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.MapGuide.MaestroAPI.Schema;
using NUnit.Framework;
using System.Xml;
using System.IO;
namespace OSGeo.MapGuide.MaestroAPI.Schema.Tests
{
    [TestFixture]
    public class SchemaTests
    {
        [Test]
        public void ClassDefinitionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new ClassDefinition(null, ""));
            Assert.Throws<ArgumentException>(() => new ClassDefinition("", ""));

            var cls = new ClassDefinition("Foo", "Bar");
            Assert.AreEqual("Foo", cls.Name);
            Assert.AreEqual("Bar", cls.Description);
            Assert.AreEqual(0, cls.IdentityProperties.Count);
            Assert.AreEqual(0, cls.Properties.Count);
            Assert.Null(cls.Parent);
        }

        [Test]
        public void GetOrdinalTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            Assert.Less(cls.GetOrdinal("Prop1"), 0);

            var prop = new DataPropertyDefinition("Prop1", "");
            prop.DataType = DataPropertyType.String;
            cls.AddProperty(prop);
            Assert.GreaterOrEqual(cls.GetOrdinal("Prop1"), 0);
        }

        [Test]
        public void ClearIdentityPropertiesTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            var prop = new DataPropertyDefinition("ID", "");
            prop.DataType = DataPropertyType.Int32;
            prop.IsAutoGenerated = true;
            cls.AddProperty(prop, true);
            Assert.AreEqual(cls, prop.Parent);
            Assert.AreEqual(1, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);
            Assert.GreaterOrEqual(cls.GetOrdinal("ID"), 0);
            cls.ClearIdentityProperties();
            Assert.AreEqual(0, cls.IdentityProperties.Count);
        }

        [Test]
        public void IndexOfPropertyTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            var prop = new DataPropertyDefinition("ID", "");
            prop.DataType = DataPropertyType.Int32;
            prop.IsAutoGenerated = true;
            cls.AddProperty(prop, true);
            Assert.AreEqual(cls, prop.Parent);
            Assert.AreEqual(1, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);
            Assert.GreaterOrEqual(cls.GetOrdinal("ID"), 0);

            var prop2 = new DataPropertyDefinition("Prop1", "");
            Assert.Less(cls.IndexOfProperty(prop2), 0);
            Assert.GreaterOrEqual(cls.IndexOfProperty(prop), 0);
        }

        [Test]
        public void RemovePropertyTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            var prop = new DataPropertyDefinition("ID", "");
            prop.DataType = DataPropertyType.Int32;
            prop.IsAutoGenerated = true;
            var prop2 = new DataPropertyDefinition("Prop1", "");
            cls.AddProperty(prop, true);
            cls.AddProperty(prop2);
            Assert.AreEqual(cls, prop.Parent);
            Assert.AreEqual(cls, prop2.Parent);
            Assert.AreEqual(2, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);

            cls.RemoveProperty("Prop1");
            Assert.AreEqual(1, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);
            Assert.Less(cls.IndexOfProperty(prop2), 0);

            cls.RemoveProperty(prop);
            Assert.AreEqual(0, cls.Properties.Count);
            Assert.AreEqual(0, cls.IdentityProperties.Count);
            Assert.Less(cls.IndexOfProperty(prop), 0);
        }

        [Test]
        public void RemovePropertyAtTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            var prop = new DataPropertyDefinition("ID", "");
            prop.DataType = DataPropertyType.Int32;
            prop.IsAutoGenerated = true;
            var prop2 = new DataPropertyDefinition("Prop1", "");
            cls.AddProperty(prop, true);
            cls.AddProperty(prop2);
            Assert.AreEqual(cls, prop.Parent);
            Assert.AreEqual(cls, prop2.Parent);
            Assert.AreEqual(2, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);

            int idx1 = cls.GetOrdinal("Prop1");
            Assert.GreaterOrEqual(idx1, 0);
            cls.RemovePropertyAt(idx1);
            Assert.AreEqual(1, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);
            Assert.Less(cls.IndexOfProperty(prop2), 0);

            int idx2 = cls.GetOrdinal("ID");
            Assert.GreaterOrEqual(idx2, 0);
            cls.RemovePropertyAt(idx2);
            Assert.AreEqual(0, cls.Properties.Count);
            Assert.AreEqual(0, cls.IdentityProperties.Count);
            Assert.Less(cls.IndexOfProperty(prop), 0);
        }

        [Test]
        public void FindPropertyTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            var prop = new DataPropertyDefinition("ID", "");
            prop.DataType = DataPropertyType.Int32;
            prop.IsAutoGenerated = true;
            var prop2 = new DataPropertyDefinition("Prop1", "");
            Assert.Null(cls.FindProperty("ID"));
            cls.AddProperty(prop, true);
            Assert.Null(cls.FindProperty("Prop1"));
            cls.AddProperty(prop2);
            Assert.AreEqual(cls, prop.Parent);
            Assert.AreEqual(cls, prop2.Parent);
            Assert.AreEqual(2, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);

            Assert.NotNull(cls.FindProperty("ID"));
            Assert.NotNull(cls.FindProperty("Prop1"));
        }

        [Test]
        public void CloneTest()
        {
            var cls = new ClassDefinition("Foo", "Bar");
            var prop = new DataPropertyDefinition("ID", "");
            prop.DataType = DataPropertyType.Int32;
            prop.IsAutoGenerated = true;
            var prop2 = new DataPropertyDefinition("Prop1", "");
            Assert.Null(cls.FindProperty("ID"));
            cls.AddProperty(prop, true);
            Assert.Null(cls.FindProperty("Prop1"));
            cls.AddProperty(prop2);
            Assert.AreEqual(cls, prop.Parent);
            Assert.AreEqual(cls, prop2.Parent);
            Assert.AreEqual(2, cls.Properties.Count);
            Assert.AreEqual(1, cls.IdentityProperties.Count);

            var cls2 = ClassDefinition.Clone(cls);
            Assert.AreEqual(cls.Name, cls2.Name);
            Assert.AreEqual(cls.Description, cls2.Description);
            Assert.AreEqual(cls.Properties.Count, cls2.Properties.Count);
            Assert.AreEqual(cls.IdentityProperties.Count, cls2.IdentityProperties.Count);
            Assert.AreNotSame(cls, cls2);
        }
    }
}
