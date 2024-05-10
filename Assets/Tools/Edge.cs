using UnityEngine;

namespace Tools
{
    public class Edge
    {
        public readonly int V1;
        public readonly int V2;
        
        public Edge(int v1, int v2)
        {
            V1 = v1;
            V2 = v2;
        }
    }
}