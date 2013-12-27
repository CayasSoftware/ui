﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using OsmSharp.Math.Geo.Projections;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Test.Performance.UI.Scene;
using OsmSharp.UI.Map.Styles.MapCSS;
using OsmSharp.UI.Map.Styles.Streams;
using OsmSharp.UI.Renderer.Scene;
using OsmSharp.UI.Renderer.Scene.Simplification;

namespace OsmSharp.Test.Performance.UI.Styles.MapCSS
{
    /// <summary>
    /// Contains tests for the MapCSS interpreter.
    /// </summary>
    public static class MapCSSInterpreterTests
    {
        /// <summary>
        /// Executes the tests.
        /// </summary>
        public static void Test()
        {
            // create a layered scene.
            Scene2D scene = new Scene2D(new OsmSharp.Math.Geo.Projections.WebMercator(), new List<float>(new float[] {
                16, 13, 10 }));

            // tests map css interpreter.
            MapCSSInterpreterTests.TestInterpret("MapCSSInterpreter", @"mapcss\complete.mapcss", scene, "kempen.osm.pbf");

            var merger = new Scene2DObjectMerger();
            scene = merger.BuildMergedScene(scene);

            // tests serialization of the scene.
            Scene2DTests.TestSerialize("Scene2DLayered", @"mapcss\complete.map", scene, true);
        }

        /// <summary>
        /// Tests interpreting all data from a given pbf source.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mapCSS"></param>
        /// <param name="scene"></param>
        /// <param name="pbfSource"></param>
        public static void TestInterpret(string name, string mapCSS, Scene2D scene, string pbfSource)
        {
            FileInfo cssFile = new FileInfo(string.Format(@".\TestFiles\{0}", mapCSS));
            Stream cssStream = cssFile.OpenRead();
            MapCSSInterpreter interpreter = new MapCSSInterpreter(cssStream, new MapCSSDictionaryImageSource());

            MapCSSInterpreterTests.TestInterpret(name, interpreter, scene, pbfSource);

            cssStream.Dispose();
        }

        /// <summary>
        /// Tests interpreting all data from a given pbf source.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scene"></param>
        /// <param name="interpreter"></param>
        /// <param name="pbfSource"></param>
        public static void TestInterpret(string name, MapCSSInterpreter interpreter, Scene2D scene, string pbfSource)
        {
            StyleOsmStreamSceneTarget target = new StyleOsmStreamSceneTarget(
                interpreter, scene, new WebMercator());
            FileInfo testFile = new FileInfo(string.Format(@".\TestFiles\{0}", pbfSource));
            Stream stream = testFile.OpenRead();
            PBFOsmStreamSource source = new PBFOsmStreamSource(stream);
            OsmStreamFilterProgress progress = new OsmStreamFilterProgress(source);
            target.RegisterSource(progress);

            PerformanceInfoConsumer performanceInfo = new PerformanceInfoConsumer(string.Format("{0}.Add", name));
            performanceInfo.Start();
            performanceInfo.Report("Interpreting style with objects from {0}...", pbfSource.ToString());

            target.Pull();

            performanceInfo.Stop();

            Console.Write("", scene.BackColor);
            stream.Dispose();
        }
    }
}
