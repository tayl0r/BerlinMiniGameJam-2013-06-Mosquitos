using UnityEngine;

namespace Com.Freakow.RageTools.Extensions {

	public static class RageSplineExtension {

		public static int Redraw(this RageSpline rageSpline, bool triangulate, bool calculateNormals, bool calculatePhysics, int startNormals, float percentNormals) {

			rageSpline.SetVertexCount(rageSpline.GetVertexCount());
 
			if (Mathf.Abs(rageSpline.gameObject.transform.localScale.x) <= 0f ||
				Mathf.Abs(rageSpline.gameObject.transform.localScale.y) <= 0f ||
				Mathf.Abs(rageSpline.gameObject.transform.localScale.z) <= 0f) 
				return startNormals;
 
			int points = rageSpline.GetVertexCount() + 1;
			int end = (int) (points*percentNormals);
			if(end > points) end = points;
 
			if (calculateNormals)
				ProgressivePrecalcNormals(rageSpline, startNormals, end);
 
			GenerateMesh(rageSpline, triangulate);
 
			if(calculatePhysics) rageSpline.RefreshPhysics();
  
			return end >= points ? 0 : end;
		}

		public static void ProgressivePrecalcNormals(RageSpline rageSpline, int start, int end) {

			int points = rageSpline.GetVertexCount() + 1;
			if(rageSpline.spline.precalcNormals == null
			|| rageSpline.spline.precalcNormals.Length != points)
				rageSpline.spline.precalcNormals = new Vector3[points];

			Vector3 up = new Vector3(0f, 0f, -1f);

			for(int i = start; i < end; i++) 
				rageSpline.spline.precalcNormals[i] = CalculateNormal(rageSpline, i / (float)(points - 1), up);
		}

		private static void GenerateMesh(RageSpline rs, bool refreshTriangulation) {

			if(rs.GetFill() != RageSpline.Fill.None) 
				rs.ShiftOverlappingControlPoints();

			bool fillAntialiasing = false;
			float aaWidth = rs.GetAntialiasingWidth();
			if (aaWidth > 0f)
				if (rs.inverseTriangleDrawOrder)
					fillAntialiasing = true;
				else
					if (rs.GetOutline() == RageSpline.Outline.None ||
						Mathf.Abs(rs.GetOutlineNormalOffset()) > (rs.GetOutlineWidth() + (aaWidth)))
						fillAntialiasing = true;

			bool outlineAntialiasing = rs.GetAntialiasingWidth() > 0f;

			bool multipleMaterials = false;
			var renderer = rs.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
			if(renderer != null) {
				if(renderer.sharedMaterials.GetLength(0) > 1) {
					multipleMaterials = true;
				}
			}

			RageSpline.RageVertex[] outlineVerts = rs.GenerateOutlineVerts(outlineAntialiasing, multipleMaterials);
			RageSpline.RageVertex[] fillVerts = rs.GenerateFillVerts(fillAntialiasing, multipleMaterials);

			int vertexCount = outlineVerts.Length + fillVerts.Length;
			Vector3[] verts = new Vector3[vertexCount];

			Vector2[] uvs = null;
			Vector2[] uvs2 = null;

			if(rs.FixUvs) {
				uvs = new Vector2[vertexCount];
				uvs2 = new Vector2[vertexCount];
			}

			Color[] colors = new Color[vertexCount];

			for(int i = 0; i < fillVerts.Length; i++) {
				verts[i] = fillVerts[i].position;
				colors[i] = fillVerts[i].color;

				if(uvs == null) continue;
				uvs[i] = fillVerts[i].uv1;
				uvs2[i] = fillVerts[i].uv2;
			}

			for(int i = 0; i < outlineVerts.Length; i++) {
				int v = i + fillVerts.Length;
				verts[v] = outlineVerts[i].position;
				colors[v] = outlineVerts[i].color;

				if(uvs == null) continue;
				uvs[v] = outlineVerts[i].uv1;
				uvs2[v] = outlineVerts[i].uv2;
			}

			var mFilter = rs.GetComponent(typeof(MeshFilter)) as MeshFilter;

			if(verts.Length > 0) 
				verts[0] += new Vector3(0f, 0f, -0.001f);

			if (mFilter != null) {
				var mesh = mFilter.sharedMesh ?? new Mesh();

				if(refreshTriangulation) mesh.Clear();

				mesh.vertices = verts;

				if(refreshTriangulation) 
					rs.GenerateTriangles(mesh, fillVerts, new RageSpline.RageVertex[0], outlineVerts, fillAntialiasing, false, outlineAntialiasing, multipleMaterials);

				if(rs.FixUvs){
					mesh.uv = uvs;
					mesh.uv2 = uvs2;                    
				}

				mesh.colors = colors;
				mesh.RecalculateBounds();
				mFilter.sharedMesh = mesh;
			}
		}

		public static Vector3 CalculateNormal(RageSpline rs, float t, Vector3 up) {

			if(rs.spline.points.Length > 0) {

				t = Mathf.Clamp01(t);
				float t1 = t - 0.001f;
				t1 = (t1 % 1f + 1f) % 1f;

				int i1 = rs.spline.GetFloorIndex(t1);
				int i2 = rs.spline.GetCeilIndex(t1);
				float f1 = rs.spline.GetSegmentPosition(t1);

				RageSplinePoint p1 = rs.spline.points[i1];
				RageSplinePoint p2 = rs.spline.points[i2];

				float t2 = t + 0.001f;
				t2 = (t2 % 1f + 1f) % 1f;

				int i3 = rs.spline.GetFloorIndex(t2);
				int i4 = rs.spline.GetCeilIndex(t2);
				float f2 = rs.spline.GetSegmentPosition(t2);

				RageSplinePoint p3 = rs.spline.points[i3];
				RageSplinePoint p4 = rs.spline.points[i4];

				Vector3 tangent1 = (-3f * p1.point + 9f * (p1.point + p1.outCtrl) - 9f * (p2.point + p2.inCtrl) + 3f * p2.point) * f1 * f1
					+ (6f * p1.point - 12f * (p1.point + p1.outCtrl) + 6f * (p2.point + p2.inCtrl)) * f1
					- 3f * p1.point + 3f * (p1.point + p1.outCtrl);

				Vector3 tangent2 = (-3f * p3.point + 9f * (p3.point + p3.outCtrl) - 9f * (p4.point + p4.inCtrl) + 3f * p4.point) * f2 * f2
					+ (6f * p3.point - 12f * (p3.point + p3.outCtrl) + 6f * (p4.point + p4.inCtrl)) * f2
					- 3f * p3.point + 3f * (p3.point + p3.outCtrl);

				return Vector3.Cross((tangent1.normalized + tangent2.normalized) * 0.5f, up).normalized;
			}

			return new Vector3(1f, 0f, 0f);
		}
	}
}
