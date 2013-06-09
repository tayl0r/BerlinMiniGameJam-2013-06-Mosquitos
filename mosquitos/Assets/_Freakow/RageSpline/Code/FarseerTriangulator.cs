using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Delaunay.Sweep;
using Poly2Tri.Triangulation.Delaunay;
using UnityEngine;

public class FarseerTriangulator : ScriptableObject, IRageTriangulator {

    public int[] Triangulate(UnityEngine.Vector2[] verts) {
        PolygonPoint[] points = new PolygonPoint[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            points[i] = new PolygonPoint(verts[i].x, verts[i].y);
        Polygon polygon = new Polygon(points);
        DTSweepContext tcx = new DTSweepContext();
        tcx.PrepareTriangulation(polygon);
        DTSweep.Triangulate(tcx);
        int[] resultPoints = new int[polygon.Triangles.Count * 3];
        int idx = 0;

        foreach (DelaunayTriangle triangle in polygon.Triangles) {
            resultPoints[idx++] = FindIndex(points, triangle.Points._0);
            resultPoints[idx++] = FindIndex(points, triangle.Points._1);
            resultPoints[idx++] = FindIndex(points, triangle.Points._2);
        }
        return resultPoints;
    }

    private int FindIndex(PolygonPoint[] points, TriangulationPoint toFind) {
        for (int i = 0; i < points.Length; i++){
			PolygonPoint p = points[i];
            if (p == toFind) return i;
			if (p.X != toFind.X) continue;
			if (p.Y != toFind.Y) continue;
			return i;
		}
        return -1;
    }
}

