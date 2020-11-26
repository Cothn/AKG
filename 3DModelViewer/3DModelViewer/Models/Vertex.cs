using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DModelViewer.Models
{
    public class Vertex
    {
        public Vertex()
        { 
        }

        public Vertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }


    }
}
