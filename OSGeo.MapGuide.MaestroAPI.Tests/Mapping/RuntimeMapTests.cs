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

using Moq;
using NUnit.Framework;
using OSGeo.MapGuide.MaestroAPI.Mapping;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.MaestroAPI.Tests;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.LayerDefinition;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using OSGeo.MapGuide.ObjectModels.TileSetDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OSGeo.MapGuide.MaestroAPI.Mapping.Tests
{
    [TestFixture]
    public class RuntimeMapTests
    {
        private static RuntimeMap CreateTestMap()
        {
            var layers = new Dictionary<string, ILayerDefinition>();
            var mdf = (IMapDefinition)ObjectFactory.Deserialize(ResourceTypes.MapDefinition.ToString(), Utils.OpenFile("UserTestData\\TestTiledMap.xml"));
            mdf.ResourceID = "Library://UnitTest/Test.MapDefinition";
            foreach (var lyr in mdf.BaseMap.BaseMapLayerGroups.First().BaseMapLayer)
            {
                var ldf = ObjectFactory.CreateDefaultLayer(LayerType.Vector, new Version(1, 0, 0));
                ldf.ResourceID = lyr.ResourceId;
                layers.Add(lyr.ResourceId, ldf);
            }
            var conn = new Mock<IServerConnection>();
            var mapSvc = new Mock<IMappingService>();
            var resSvc = new Mock<IResourceService>();
            var caps = new Mock<IConnectionCapabilities>();

            foreach (var kvp in layers)
            {
                resSvc.Setup(r => r.GetResource(kvp.Key)).Returns(kvp.Value);
            }
            resSvc.Setup(r => r.GetResource("Library://UnitTest/Test.MapDefinition")).Returns(mdf);

            foreach (var kvp in layers)
            {
                mapSvc.Setup(m => m.CreateMapLayer(It.IsAny<RuntimeMap>(), It.IsAny<IBaseMapLayer>()))
                      .Returns((RuntimeMap rtMap, IBaseMapLayer rl) => new RuntimeMapLayer(rtMap) { Name = rl.Name, LayerDefinitionID = kvp.Key });
            }
            mapSvc.Setup(m => m.CreateMapGroup(It.IsAny<RuntimeMap>(), It.IsAny<IBaseMapGroup>()))
                  .Returns((RuntimeMap rtMap, IBaseMapGroup grp) => new RuntimeMapGroup(rtMap, grp));
            mapSvc.Setup(m => m.CreateMapGroup(It.IsAny<RuntimeMap>(), It.IsAny<string>()))
                  .Returns((RuntimeMap rtMap, string name) => new RuntimeMapGroup(rtMap, name));
            
            caps.Setup(c => c.SupportedServices).Returns(new int[] { (int)ServiceType.Mapping });
            caps.Setup(c => c.SupportedCommands).Returns(new int[0]);
            caps.Setup(c => c.GetMaxSupportedResourceVersion(It.IsAny<string>())).Returns(new Version(1, 0, 0));
            
            conn.Setup(c => c.Capabilities).Returns(caps.Object);
            conn.Setup(c => c.ResourceService).Returns(resSvc.Object);
            conn.Setup(c => c.GetService((int)ServiceType.Mapping)).Returns(mapSvc.Object);

            var map = new RuntimeMap(conn.Object, mdf, 1.0, true);
            Assert.AreEqual(15, map.FiniteDisplayScaleCount);
            Assert.NotNull(map.Layers);
            Assert.NotNull(map.Groups);
            return map;
        }

        [Test]
        public void UnsupportedConnectionTest()
        {
            var res = (IMapDefinition)ObjectFactory.Deserialize(ResourceTypes.MapDefinition.ToString(), Utils.OpenFile("UserTestData\\TestTiledMap.xml"));
            var conn = new Mock<IServerConnection>();
            var caps = new Mock<IConnectionCapabilities>();
            caps.Setup(c => c.SupportedServices).Returns(new int[0]);
            caps.Setup(c => c.SupportedCommands).Returns(new int[0]);
            conn.Setup(c => c.Capabilities).Returns(caps.Object);

            Assert.Throws<NotSupportedException>(() => new RuntimeMap(conn.Object, res, 1.0, true));
        }

        [Test]
        public void GetFiniteDisplayScaleAtTest()
        {
            var map = CreateTestMap();
            Assert.Throws<IndexOutOfRangeException>(() => map.GetFiniteDisplayScaleAt(-1));
            Assert.Throws<IndexOutOfRangeException>(() => map.GetFiniteDisplayScaleAt(16));
            for (int i = 0; i < map.FiniteDisplayScaleCount; i++)
            {
                Assert.Greater(map.GetFiniteDisplayScaleAt(i), 0);
            }
        }

        [Test]
        public void SetViewCenterTest()
        {
            var map = CreateTestMap();
            map.SetViewCenter(-37, 23);
            Assert.AreEqual(-37, map.ViewCenter.X);
            Assert.AreEqual(23, map.ViewCenter.Y);
        }

        [Test]
        public void GetGroupByNameTest()
        {
            var map = CreateTestMap();
            Assert.NotNull(map.Groups["Base Layer Group"]);
            Assert.Null(map.Groups["asjgsfdsf"]);
        }

        [Test]
        public void GetLayerByObjectIdTest()
        {
            var map = CreateTestMap();
            var lyr = map.Layers["Rail"];
            var lyr2 = map.GetLayerByObjectId(lyr.ObjectId);
            Assert.NotNull(lyr2);
            Assert.AreSame(lyr, lyr2);
            Assert.Null(map.GetLayerByObjectId("asjgsfdsf"));
        }

        [Test]
        public void InsertLayerTest()
        {
            var map = CreateTestMap();
            //Mock setup in CreateTestMap() will ensure we get a service that does
            //the things we require
            var mapSvc = (IMappingService)map.CurrentConnection.GetService((int)ServiceType.Mapping);
            var layer = new Mock<IBaseMapLayer>();
            layer.Setup(l => l.Name).Returns("Test");
            layer.Setup(l => l.ResourceId).Returns("Library://Test/Test.LayerDefinition");
            var lyr = mapSvc.CreateMapLayer(map, layer.Object);
            int count = map.Layers.Count;
            map.Layers.Insert(0, lyr);
            Assert.AreEqual(count + 1, map.Layers.Count);
            var lyr2 = map.Layers[0];
            Assert.AreSame(lyr, lyr2);
        }

        [Test]
        public void SetLayerIndexTest()
        {
            var map = CreateTestMap();
            //Mock setup in CreateTestMap() will ensure we get a service that does
            //the things we require
            var mapSvc = (IMappingService)map.CurrentConnection.GetService((int)ServiceType.Mapping);
            var layer = new Mock<IBaseMapLayer>();
            layer.Setup(l => l.Name).Returns("Test");
            layer.Setup(l => l.ResourceId).Returns("Library://Test/Test.LayerDefinition");
            var lyr = mapSvc.CreateMapLayer(map, layer.Object);
            int count = map.Layers.Count;
            int idx = map.Layers.Count - 1;
            map.Layers[idx] = lyr;
            Assert.AreEqual(count, map.Layers.Count);
            var lyr2 = map.Layers[idx];
            Assert.AreSame(lyr, lyr2);
        }

        [Test]
        public void RemoveLayerAtTest()
        {
            var map = CreateTestMap();
            
            Assert.Throws<ArgumentOutOfRangeException>(() => map.Layers.RemoveAt(5));
            int count = map.Layers.Count;
            var lyr = map.Layers[0];
            map.Layers.RemoveAt(0);
            Assert.AreEqual(count - 1, map.Layers.Count);
            Assert.Null(map.Layers[lyr.Name]);
        }

        [Test]
        public void IndexOfLayerTest()
        {
            var map = CreateTestMap();
            Assert.GreaterOrEqual(map.IndexOfLayer("Rail"), 0);
            Assert.GreaterOrEqual(map.IndexOfLayer("Parcels"), 0);
            Assert.GreaterOrEqual(map.IndexOfLayer("HydrographicPolygons"), 0);
            Assert.Less(map.IndexOfLayer("sdjgdsfasdf"), 0);
        }

        [Test]
        public void RemoveLayerTest()
        {
            var map = CreateTestMap();
            Assert.AreEqual(3, map.Layers.Count);
            map.Layers.Remove("Rail");
            Assert.AreEqual(2, map.Layers.Count);
            map.Layers.Remove("asdgjsdfdsf");
            Assert.AreEqual(2, map.Layers.Count);
        }

        [Test]
        public void RemoveGroupTest()
        {
            var map = CreateTestMap();
            Assert.AreEqual(1, map.Groups.Count);
            map.Groups.Remove("Base Layer Group");
            Assert.AreEqual(0, map.Groups.Count);

            map = CreateTestMap();
            var grp = map.Groups[0];
            map.Groups.Remove(grp);
            Assert.AreEqual(0, map.Groups.Count);
        }

        [Test]
        public void GetLayersOfGroupTest()
        {
            var map = CreateTestMap();
            var layers = map.GetLayersOfGroup("Base Layer Group");
            Assert.AreEqual(3, layers.Length);
            layers = map.GetLayersOfGroup("asdjgdsfd");
            Assert.AreEqual(0, layers.Length);
        }

        [Test]
        public void GetGroupsOfGroupTest()
        {
            var map = CreateTestMap();
            var groups = map.GetGroupsOfGroup("Base Layer Group");
            Assert.AreEqual(0, groups.Length);
            groups = map.GetGroupsOfGroup("asdjsdfdsfd");
            Assert.AreEqual(0, groups.Length);
            var mapSvc = (IMappingService)map.CurrentConnection.GetService((int)ServiceType.Mapping);
            var grp = mapSvc.CreateMapGroup(map, "Test");
            grp.Group = "Base Layer Group";
            map.Groups.Add(grp);
            Assert.AreEqual(2, map.Groups.Count);
            groups = map.GetGroupsOfGroup("Base Layer Group");
            Assert.AreEqual(1, groups.Length);
            groups = map.GetGroupsOfGroup("Test");
            Assert.AreEqual(0, groups.Length);
        }
        
        [Test]
        public void GetLayerByNameTest()
        {
            var map = CreateTestMap();
            Assert.NotNull(map.Layers["Rail"]);
            Assert.NotNull(map.Layers["Parcels"]);
            Assert.NotNull(map.Layers["HydrographicPolygons"]);
            Assert.Null(map.Layers["sdhgdsafdsfd"]);
        }

        [Test]
        public void UpdateMapDefinitionTest()
        {
            var map = CreateTestMap();
            IMapDefinition mdf = ObjectFactory.CreateMapDefinition(new Version(1, 0, 0), "Test Map");
            map.UpdateMapDefinition(mdf);
            Assert.AreEqual(0, mdf.GetDynamicLayerCount());
            Assert.AreEqual(0, mdf.GetGroupCount());
            Assert.NotNull(mdf.BaseMap);
            var grp = mdf.BaseMap.GetFirstGroup();
            Assert.NotNull(grp);
            Assert.AreEqual(3, grp.BaseMapLayer.Count());
            Assert.AreEqual("Base Layer Group", grp.Name);
        }

        [Test]
        public void ToMapDefinitionTest()
        {
            var map = CreateTestMap();
            var mdf = map.ToMapDefinition(false);
            Assert.AreEqual(0, mdf.GetDynamicLayerCount());
            Assert.AreEqual(0, mdf.GetGroupCount());
            Assert.NotNull(mdf.BaseMap);
            var grp = mdf.BaseMap.GetFirstGroup();
            Assert.NotNull(grp);
            Assert.AreEqual(3, grp.BaseMapLayer.Count());
            Assert.AreEqual("Base Layer Group", grp.Name);

            mdf = map.ToMapDefinition(true);
            Assert.AreEqual(0, mdf.GetDynamicLayerCount());
            Assert.AreEqual(0, mdf.GetGroupCount());
            Assert.NotNull(mdf.BaseMap);
            grp = mdf.BaseMap.GetFirstGroup();
            Assert.NotNull(grp);
            Assert.AreEqual(3, grp.BaseMapLayer.Count());
            Assert.AreEqual("Base Layer Group", grp.Name);
        }
    }
}
