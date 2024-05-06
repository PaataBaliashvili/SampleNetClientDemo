using UnityEngine;

namespace Tools
{
    public class Triangle
    {
        public HalfEdge HalfEdge;
        public Vertex Vertex1;
        public Vertex Vertex2;
        public Vertex Vertex3;

        public Triangle(Vertex vertex1, Vertex vertex2, Vertex vertex3)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
        }
    
        public void ChangeOrientation()
        {
            (Vertex1, Vertex2) = (Vertex2, Vertex1);
        }
    }
}