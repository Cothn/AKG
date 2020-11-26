using _3DModelViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3DModelViewer
{
    public class ObjParser
    {
        public (List<Vector3>, List<int[]>) Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            var vertex = new List<Vector3>();
            var texturevVertex = new List<Vertex>();
            var normalVectors = new List<Vertex>();
            var poligons = new List<int[]>();

            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    var coords = line.Split(' ')
                                     .Skip(1)
                                     .Select(c => float.Parse(c))
                                     .ToArray();

                    vertex.Add(new Vector3(coords[0], coords[1], coords[2]));
                    continue;
                }
                if (line.StartsWith("vt"))
                {
                }
                if (line.StartsWith("vn"))
                {
                }
                if (line.StartsWith("f "))
                {
                    var coords = line.Split(' ')
                                     .Skip(1)
                                     .Select(c => c.Split('/').First())
                                     .Select(c => Int32.Parse(c))
                                     .ToArray();

                    poligons.Add(coords);
                    continue;
                }
            }

            return (vertex, poligons);

        }
    }
}
