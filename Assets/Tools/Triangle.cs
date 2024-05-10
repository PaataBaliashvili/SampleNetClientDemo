using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public class Triangle
    {
        public readonly List<Edge> Edges = new();
        public readonly List<int> VertIndices;
        public HalfEdge HalfEdge;
        public Vertex Vertex1;
        public Vertex Vertex2;
        public Vertex Vertex3;

        public Triangle(Vertex vertex1, Vertex vertex2, Vertex vertex3, List<int> vertIndices)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
            VertIndices = vertIndices;
            
            Edges.Add(new Edge(vertIndices[0], vertIndices[1]));
            Edges.Add(new Edge(vertIndices[1], vertIndices[2]));
            Edges.Add(new Edge(vertIndices[2], vertIndices[1]));
        }
    
        public void ChangeOrientation()
        {
            (Vertex1, Vertex2) = (Vertex2, Vertex1);
        }
    }
}