using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using verb.core;
using verb.eval;
using verb.geom;

namespace VerbTest
{
    public static class VerbTest
    {
        /// <summary>
        /// The VerbTest function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A VerbTestOutputs instance containing computed results and the model with any new elements.</returns>
        public static VerbTestOutputs Execute(Dictionary<string, Model> inputModels, VerbTestInputs input)
        {
            /// Your code here.
            var output = new VerbTestOutputs();
            var sphere = new SphericalSurface(input.SphereCenter.ToVPt(), input.SphereRadius);
            List<Vector3> pts = new List<Vector3>();
            for (double u = 0; u < 1.0; u += 0.05)
            {
                for (double v = 0; v < 1.0; v += 0.05)
                {
                    var pt = sphere.point(u, v).ToVector3();
                    pts.Add(pt);
                }
            }
            var mesh = sphere.tessellate(new AdaptiveRefinementOptions
            {
                minDivsU = 50,
                minDivsV = 50,
                refine = false
            }).ToMesh();
            mesh.Material = new Material("White", Colors.White);
            output.Model.AddElement(new MeshElement(mesh, new Material("Shiny", Colors.Red, 0.9, 0.6)));
            output.Model.AddElement(new ModelPoints(pts, BuiltInMaterials.Black));

            var Random = new Random(5);
            var curves = new Array<object>();
            for (var u = 0; u < 4; u++)
            {
                var points = new Array<object>();
                var ctrlPts = new List<Vector3>();
                for (var v = 0; v < 4; v++)
                {
                    var pt = new Vector3(u * input.Width / 4, v * input.Length / 4, Random.NextDouble() * input.Height);
                    points.push(pt.ToVPt());
                    ctrlPts.Add(pt);
                }
                // output.Model.AddElement(new ModelCurve(new Polyline(ctrlPts)));
                var curve = NurbsCurve.byPoints(points, new haxe.lang.Null<int>(3, true));
                curves.push(curve);
            }
            Console.WriteLine("Done making curves");
            var nurbsSrf = NurbsSurface.byLoftingCurves(curves, new haxe.lang.Null<int>(3, true));
            Console.WriteLine("Done making srf");

            var srfMesh = nurbsSrf.tessellate(new AdaptiveRefinementOptions
            {
                minDivsU = 50,
                minDivsV = 50,
                refine = false
            }).ToMesh();
            Console.WriteLine("Done making mesh");

            output.Model.AddElement(new MeshElement(srfMesh, new Material("Shiny Blue", Colors.Blue, 0.9, 0.6, null, false, true)));
            return output;
        }

        public static Elements.Geometry.Mesh ToMesh(this MeshData vMesh)
        {
            var mesh = new Elements.Geometry.Mesh();
            var vertices = new List<Vertex>();
            for (int i = 0; i < vMesh.points.length; i++)
            {
                var pt = (vMesh.points[i] as Array<double>).ToVector3();
                var normal = (vMesh.normals[i] as Array<double>).ToVector3();
                var vertex = new Vertex(pt, normal);
                vertices.Add(vertex);
                mesh.AddVertex(vertex);
            }
            Console.WriteLine(vertices.Count);
            for (int i = 0; i < vMesh.faces.length; i++)
            {
                var face = vMesh.faces[i] as Array<int>;
                mesh.AddTriangle(vertices[face[2]], vertices[face[1]], vertices[face[0]]);
            }
            return mesh;
        }

        public static Array<double> ToVPt(this Vector3 v)
        {
            return new Array<double>(new double[] { v.X, v.Y, v.Z });
        }
        public static Vector3 ToVector3(this Array<double> pt)
        {
            var X = double.IsNaN(pt[0]) ? 0 : pt[0];
            var Y = double.IsNaN(pt[1]) ? 0 : pt[1];
            var Z = double.IsNaN(pt[2]) ? 0 : pt[2];
            return new Vector3(X, Y, Z);
        }
    }
}