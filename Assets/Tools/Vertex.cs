using UnityEngine;

namespace Tools
{
    public class Vertex
    {
        public HalfEdge halfEdge;
        
        public Vertex(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }
}

