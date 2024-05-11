using UnityEngine;

namespace Tools
{
    public class Edge
    {
        public readonly Vertex V1;
        public readonly Vertex V2;
        
        public Edge(Vertex v1, Vertex v2)
        {
            V1 = v1;
            V2 = v2;
        }
    }
}