


using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Vertex = Tools.Vertex;

public static class GeometryUtils
{
    public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
    {
        //Make sure the triangles have the same orientation
        OrientTrianglesClockwise(triangles);

        //First create a list with all possible half-edges
        List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle t = triangles[i];
	
            HalfEdge he1 = new HalfEdge(t.Vertex1);
            HalfEdge he2 = new HalfEdge(t.Vertex2);
            HalfEdge he3 = new HalfEdge(t.Vertex3);

            he1.nextEdge = he2;
            he2.nextEdge = he3;
            he3.nextEdge = he1;

            he1.prevEdge = he3;
            he2.prevEdge = he1;
            he3.prevEdge = he2;

            //The vertex needs to know of an edge going from it
            he1.V.halfEdge = he2;
            he2.V.halfEdge = he3;
            he3.V.halfEdge = he1;

            //The face the half-edge is connected to
            t.HalfEdge = he1;

            he1.t = t;
            he2.t = t;
            he3.t = t;

            //Add the half-edges to the list
            halfEdges.Add(he1);
            halfEdges.Add(he2);
            halfEdges.Add(he3);
        }

        //Find the half-edges going in the opposite direction
        for (int i = 0; i < halfEdges.Count; i++)
        {
            HalfEdge he = halfEdges[i];

            Vertex goingToVertex = he.V;
            Vertex goingFromVertex = he.prevEdge.V;

            for (int j = 0; j < halfEdges.Count; j++)
            {
                //Dont compare with itself
                if (i == j)
                {
                    continue;
                }

                HalfEdge heOpposite = halfEdges[j];

                //Is this edge going between the vertices in the opposite direction
                if (goingFromVertex.Position == heOpposite.V.Position && goingToVertex.Position == heOpposite.prevEdge.V.Position)
                {
                    he.oppositeEdge = heOpposite;

                    break;
                }
            }
        }


        return halfEdges;
    }
    
    public static void OrientTrianglesClockwise(List<Triangle> triangles)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle tri = triangles[i];

            // Vector2 v1 = new Vector2(tri.Vertex1.Position.x, tri.Vertex1.Position.z);
            // Vector2 v2 = new Vector2(tri.Vertex2.Position.x, tri.Vertex2.Position.z);
            // Vector2 v3 = new Vector2(tri.Vertex3.Position.x, tri.Vertex3.Position.z);

            if (!IsTriangleOrientedClockwise(tri.Vertex1.Position, tri.Vertex2.Position, tri.Vertex3.Position))
            {
                tri.ChangeOrientation();
            }
        }
    }
    
    public static bool IsTriangleOrientedClockwise(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        bool isClockWise = true;

        float determinant = p1.x * p2.z + p3.x * p1.z + p2.x * p3.z - p1.x * p3.z - p3.x * p2.z - p2.x * p1.z;

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }
    
    public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
    {
        bool isIntersecting = false;

        float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

        //Make sure the denominator is > 0, if not the lines are parallel
        if (denominator != 0f)
        {
            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
            float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            //Are the line segments intersecting if the end points are the same
            if (shouldIncludeEndPoints)
            {
                //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
                {
                    isIntersecting = true;
                }
            }
            else
            {
                //Is intersecting if u_a and u_b are between 0 and 1
                if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
                {
                    isIntersecting = true;
                }
            }
		
        }

        return isIntersecting;
    }
}