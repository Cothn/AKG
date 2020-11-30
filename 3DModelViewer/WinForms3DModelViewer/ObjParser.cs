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

        public (List<Vector4>, List<int[][]>, List<Vector3>, List<Vector3>) Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            var vectors = new List<Vector4>();
            var textureVectors = new List<Vector3>();
            var normalVectors = new List<Vector3>();
            var poligons = new List<int[][]>();
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

                    vectors.Add(new Vector4(coords[0], coords[1], coords[2], coords.Length > 3 ? coords[3] : 1));
                }
                else if (line.StartsWith("f "))
                {

                    var coords = line.Split(' ')
                        .Skip(1)
                        .Select(c => c.Split('/'))
                        .Select(c => c.Select(a => Int32.TryParse(a,out int res) ? res : 0).ToArray())
                        .ToArray();

                    if (coords.Length > 3)
                    {
                        int[][] tmpCoords = new int[3][];

                        tmpCoords[0] = coords[0];
                        tmpCoords[1] = coords[1];
                        tmpCoords[2] = coords[2];
                        poligons.Add(tmpCoords);
                        
                        tmpCoords = new int[3][];
                        tmpCoords[0] = coords[1];
                        tmpCoords[1] = coords[2];
                        tmpCoords[2] = coords[3];
                        poligons.Add(tmpCoords);
                        
                        tmpCoords = new int[3][];
                        tmpCoords[0] = coords[2];
                        tmpCoords[1] = coords[3];
                        tmpCoords[2] = coords[0];
                        poligons.Add(tmpCoords);
                        
                        tmpCoords = new int[3][];
                        tmpCoords[0] = coords[3];
                        tmpCoords[1] = coords[0];
                        tmpCoords[2] = coords[1];
                        poligons.Add(tmpCoords);
                    }
                    else
                    {
                        poligons.Add(coords);
                    }

                    
                }
                else if (line.StartsWith("vn"))
                {
                    var coords = line.Split(' ')
                        .Skip(1).Where(c => !string.IsNullOrEmpty(c))
                        .Select(c => float.Parse(c,NumberStyles.Any, ci))
                        .ToArray();

                    normalVectors.Add(new Vector3(coords[0], coords[1], coords[2]));
                }
                else if (line.StartsWith("vt "))
                {
                    var coords = line.Split(' ')
                        .Skip(1).Where(c => !string.IsNullOrEmpty(c))
                        .Select(c => float.Parse(c,NumberStyles.Any, ci))
                        .ToArray();

                    if (coords.Length < 3)
                    {
                        textureVectors.Add(new Vector3(coords[0], coords[1], 0));
                    }
                    else
                    {
                        textureVectors.Add(new Vector3(coords[0], coords[1], coords[2]));
                    }
                }
            }

            return (vectors, poligons, normalVectors, textureVectors);

        }

       
    }
}
