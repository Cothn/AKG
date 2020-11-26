using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DModelViewer
{
    public class ObjParser
    {

        public (List<Vector4>, List<int[]>) Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            var vertex = new List<Vector4>();
            var textureVertex = new List<Vector3>();
            var normalVectors = new List<Vector3>();
            var poligons = new List<int[]>();
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            
            
            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    var coords = line.Split(' ')
                                     .Skip(1)
                                     .Select(c => float.Parse(c,NumberStyles.Any, ci))
                                     .ToArray();

                    vertex.Add(new Vector4(coords[0], coords[1], coords[2], coords.Length > 3 ? coords[3] : 1));
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
                                     .Select(c => Int32.Parse(c, NumberStyles.Any, ci))
                                     .ToArray();

                    poligons.Add(coords);
                    continue;
                }
            }

            return (vertex, poligons);

        }

       
    }
}
