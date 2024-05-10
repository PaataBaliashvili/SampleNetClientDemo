using System.Collections.Generic;

namespace Tools.Pathfinding
{
    public class TriangleNode
    {
        public readonly List<TriangleNode> Neighbours = new();
        public readonly Triangle Triangle;
        public TriangleNode Previous;
        public float CostFromOrigin;
        public float LocationCost;
        public Edge SharedEdge;

        public TriangleNode(Triangle triangle)
        {
            Triangle = triangle;
            CostFromOrigin = float.MaxValue;
            LocationCost = 1f;
        }
    }
}