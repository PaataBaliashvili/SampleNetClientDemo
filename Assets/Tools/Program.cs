using System;
using System.Collections.Generic;

class Program
    {
        static void Main()
        {
            #region Create random triangle array

            List<int[]> tris = new List<int[]>();
            Random rand = new Random();

            for (int i = 0; i < 1000; i++)
            {
                int[] rndmTri = new int[3]
                {
                    rand.Next(0, 20),
                    rand.Next(0, 20),
                    rand.Next(0, 20)
                };

                tris.Add(rndmTri);
            }

            #endregion

            Find_Neighboring_Triangles triangles = new Find_Neighboring_Triangles
            (
                tris.ToArray()
            );

            #region Print results

            foreach (NeighboringTriangle tri in triangles.neighboringTriangles)
            {
                Console.WriteLine("Shared Vertex: " + tri.SharedVertex);

                foreach (int[] triangle in tri.Triangles)
                {
                    Console.WriteLine
                    (
                        triangle[0] + ", " +
                        triangle[1] + ", " +
                        triangle[2]
                    );
                }

                Console.WriteLine();
            }

            #endregion
        }
    }

    class Find_Neighboring_Triangles
    {
        public List<NeighboringTriangle> neighboringTriangles;
        readonly int[][] triangles;

        public Find_Neighboring_Triangles(int[][] TriangleIndicis)
        {
            neighboringTriangles = new List<NeighboringTriangle>();
            triangles = TriangleIndicis;

            GetNeighboringTriangles();
        }

        int[] GetDistinctIndicis()
        {
            List<int> DistinctIndicies = new List<int>();

            foreach (int[] triangle in triangles)
            {
                foreach (int indicie in triangle)
                {
                    if (!DistinctIndicies.Contains(indicie))
                        DistinctIndicies.Add(indicie);
                }
            }

            return DistinctIndicies.ToArray();
        }

        void GetNeighboringTriangles()
        {
            foreach (int DistinctIndicie in GetDistinctIndicis())
            {
                NeighboringTriangle neighboringTriangle = new NeighboringTriangle
                {
                    SharedVertex = DistinctIndicie
                };

                foreach (int[] triangle in triangles)
                {
                    foreach (int indicie in triangle)
                    {
                        // Checks if triangles share a common vertex
                        if (indicie == DistinctIndicie)
                        {
                            neighboringTriangle.Triangles.Add(triangle);
                            break;
                        }
                    }
                }

                if (neighboringTriangle.Triangles.Count > 1)
                    neighboringTriangles.Add(neighboringTriangle);
            }
        }
    }

    class NeighboringTriangle
    {
        public List<int[]> Triangles = new List<int[]>();
        public int SharedVertex;
    }
    