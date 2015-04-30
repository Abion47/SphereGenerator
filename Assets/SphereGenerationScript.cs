using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SphereGenerationScript : MonoBehaviour {
	public bool GenerateIcosphere = false;
	public bool GenerateUVSphere = false;
	public bool GenerateCuboidSphere = false;

	public GameObject prefab;
	public Vector3 origin = new Vector3(0, 0, 0);
	public Vector3 scale = new Vector3(1, 1, 1);

	public int icoNumberOfSubdivisions = 3;

	public int uvNumberOfLatitudes = 12;
	public int uvNumberOfLongitudes = 12;

	public int cubeNumberOfSubdivisions = 3;

	public GameObject debug1;
	public GameObject debug2;
	public GameObject debug3;
	public GameObject debug4;


	// Use this for initialization
	void Start () {
		origin = new Vector3 (2, 0, 0);
		uvNumberOfLatitudes = 32;
		uvNumberOfLongitudes = 32;
		generateUVSphere ();

		origin = new Vector3 (0, 0, 2);
		icoNumberOfSubdivisions = 3;
		generateIcosphere ();

		origin = new Vector3 (2, 0, 2);
		cubeNumberOfSubdivisions = 3;
		generateCuboidSphere ();
	}
	
	// Update is called once per frame
	void Update () {
		if (GenerateIcosphere) {
			generateIcosphere();
			GenerateIcosphere = false;
		}

		if (GenerateUVSphere) {
			generateUVSphere();
			GenerateUVSphere = false;
		}

		if (GenerateCuboidSphere) {
			generateCuboidSphere();
			GenerateCuboidSphere = false;
		}
	}

	void generateUVSphere() {
		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		
		verts.Add (new Vector3(0, -1, 0));
		verts.Add (new Vector3(0,  1, 0));

		// Special Case
		if (uvNumberOfLongitudes == 1) {
			for (int i = 0; i < uvNumberOfLatitudes; i++) {
				float angle = i == 0 ? 0f : Mathf.PI * 2 * ((float)i / uvNumberOfLatitudes);

				float x = Mathf.Cos (angle);
				float z = Mathf.Sin (angle);

				Vector3 v = new Vector3(x, 0, z);
				verts.Add(v);
				//uvs.Add(new Vector2(0, 0));
			}

			for (int i = 0; i < uvNumberOfLatitudes; i++) {
				int ip1 = (i + 1) % uvNumberOfLatitudes;

				// Top Triangle
				triangles.Add(0);
				triangles.Add(i + 2);
				triangles.Add(ip1 + 2);
				
				// Bottom Triangle
				triangles.Add(1);
				triangles.Add(ip1 + 2);
				triangles.Add(i + 2);
			}
		} else {
			List<int> lastRowIdx = new List<int> ();

			// Calculate vertices
			for (int i = 0; i < uvNumberOfLongitudes; i++) {
				List<int> thisRowIdx = new List<int>();

				float ratio = (float)(i + 1) / (float)(uvNumberOfLongitudes + 1);
				float axy = Mathf.PI * ratio;
				axy -= Mathf.PI / 2f;

				float rlo = Mathf.Cos(axy);
				float y = Mathf.Sin (axy);

				for (int j = 0; j < uvNumberOfLatitudes; j++) {
					float axz = j == 0 ? 0f : Mathf.PI * 2 * ((float)j / (float)uvNumberOfLatitudes);

					float x = Mathf.Cos (axz) * rlo;
					float z = Mathf.Sin (axz) * rlo;

					thisRowIdx.Add(verts.Count);
					verts.Add(new Vector3(x, y, z));
				}

				if (lastRowIdx.Count == 0) {
					for (int j = 0; j < thisRowIdx.Count; j++) {
						int jp1 = (j + 1) % thisRowIdx.Count;
						triangles.Add(0);
						triangles.Add(j + 2);
						triangles.Add(jp1 + 2);
					}
				} else {
					for (int j = 0; j < thisRowIdx.Count; j++) {
						int jp1 = (j + 1) % thisRowIdx.Count;
						triangles.Add(lastRowIdx[j]);
						triangles.Add(thisRowIdx[j]);
						triangles.Add(thisRowIdx[jp1]);

						triangles.Add(lastRowIdx[j]);
						triangles.Add(thisRowIdx[jp1]);
						triangles.Add(lastRowIdx[jp1]);
					}
				}

				lastRowIdx = thisRowIdx;
			}

			for (int j = 0; j < lastRowIdx.Count; j++) {
				int jp1 = (j + 1) % lastRowIdx.Count;
				triangles.Add(1);
				triangles.Add(lastRowIdx[jp1]);
				triangles.Add(lastRowIdx[j]);
			}
		}

		GameObject obj = (GameObject)Instantiate (prefab);
		obj.name = "UV Sphere";

		Mesh mesh = new Mesh ();
		
		mesh.vertices = verts.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateNormals ();

		for (int i = 0; i < mesh.vertices.Length; i++) {
			uvs.Add(Util.GenerateUV(mesh.vertices[i] - origin));
		}

		mesh.uv = uvs.ToArray ();
		
		MeshFilter filter = obj.GetComponent<MeshFilter> ();
		filter.mesh = mesh;
	}

	void generateIcosphere() {
		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();

		List<Face> faces = new List<Face> ();

		float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

		verts.Add (new Vector3 (-1,  t,  0).normalized);
		verts.Add (new Vector3 ( 1,  t,  0).normalized);
		verts.Add (new Vector3 (-1, -t,  0).normalized);
		verts.Add (new Vector3 ( 1, -t,  0).normalized);
		
		verts.Add (new Vector3 ( 0, -1,  t).normalized);
		verts.Add (new Vector3 ( 0,  1,  t).normalized);
		verts.Add (new Vector3 ( 0, -1, -t).normalized);
		verts.Add (new Vector3 ( 0,  1, -t).normalized);
		
		verts.Add (new Vector3 ( t,  0, -1).normalized);
		verts.Add (new Vector3 ( t,  0,  1).normalized);
		verts.Add (new Vector3 (-t,  0, -1).normalized);
		verts.Add (new Vector3 (-t,  0,  1).normalized);

		// 5 faces around point 0
		faces.Add (new Face (0, 11, 5));
		faces.Add (new Face (0, 5, 1));
		faces.Add (new Face (0, 1, 7));
		faces.Add (new Face (0, 7, 10));
		faces.Add (new Face (0, 10, 11));

		// 5 adjacent faces
		faces.Add (new Face (1, 5, 9));
		faces.Add (new Face (5, 11, 4));
		faces.Add (new Face (11, 10, 2));
		faces.Add (new Face (10, 7, 6));
		faces.Add (new Face (7, 1, 8)); 

		// 5 faces around point 3
		faces.Add (new Face (3, 9, 4));
		faces.Add (new Face (3, 4, 2));
		faces.Add (new Face (3, 2, 6));
		faces.Add (new Face (3, 6, 8));
		faces.Add (new Face (3, 8, 9));
		
		// 5 adjacent faces
		faces.Add (new Face (4, 9, 5));
		faces.Add (new Face (2, 4, 11));
		faces.Add (new Face (6, 2, 10));
		faces.Add (new Face (8, 6, 7));
		faces.Add (new Face (9, 8, 1));

		Dictionary<long, int> cache = new Dictionary<long, int> ();

		//Subdivide
		for (int i = 0; i < icoNumberOfSubdivisions; i++) {
			List<Face> faces2 = new List<Face>();

			foreach (var face in faces) {
				int i0 = Util.addMidpointToCache(face.q0, face.q1, ref verts, ref cache);
				int i1 = Util.addMidpointToCache(face.q1, face.q2, ref verts, ref cache);
				int i2 = Util.addMidpointToCache(face.q2, face.q0, ref verts, ref cache);

				faces2.Add(new Face(face.q0, i0, i2));
				faces2.Add(new Face(face.q1, i1, i0));
				faces2.Add(new Face(face.q2, i2, i1));
				faces2.Add(new Face(i0, i1, i2));
			}

			faces = faces2;
		}

		for (int i = 0; i < verts.Count; i++) {
			verts[i] = new Vector3(
				verts[i].normalized.x * scale.x, 
				verts[i].normalized.y * scale.y, 
				verts[i].normalized.z * scale.z) + origin;
		}

		foreach (var face in faces) {
			triangles.Add(face.q0);
			triangles.Add(face.q1);
			triangles.Add(face.q2);
		}

		GameObject obj = (GameObject)Instantiate (prefab);
		obj.name = "Icosphere";

		Mesh mesh = new Mesh ();

		mesh.vertices = verts.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateNormals ();
		
		for (int i = 0; i < mesh.vertices.Length; i++) {
			uvs.Add(Util.GenerateUV(mesh.vertices[i] - origin));
		}

		mesh.uv = uvs.ToArray ();
		MeshFilter filter = obj.GetComponent<MeshFilter> ();
		filter.mesh = mesh;
	}

	private void generateCuboidSphere() {
		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();

		// 8 Cube Vertices
		verts.Add (new Vector3 (-1, -1, -1));
		verts.Add (new Vector3 (-1, -1,  1));
		verts.Add (new Vector3 (-1,  1, -1));
		verts.Add (new Vector3 (-1,  1,  1));
		verts.Add (new Vector3 ( 1, -1, -1));
		verts.Add (new Vector3 ( 1, -1,  1));
		verts.Add (new Vector3 ( 1,  1, -1));
		verts.Add (new Vector3 ( 1,  1,  1));

		// 6 Cube Faces
		List<SquareFace> faces = new List<SquareFace> ();
		// North, South
		faces.Add (new SquareFace (7, 3, 5, 1));
		faces.Add (new SquareFace (2, 6, 0, 4));
		// East, West
		faces.Add (new SquareFace (3, 2, 1, 0));
		faces.Add (new SquareFace (6, 7, 4, 5));
		// Up, Down
		faces.Add (new SquareFace (7, 6, 3, 2));
		faces.Add (new SquareFace (4, 5, 0, 1));

		Dictionary<long, int> cache = new Dictionary<long, int> ();
		for (int i = 0; i < cubeNumberOfSubdivisions; i++) {
			List<SquareFace> faces2 = new List<SquareFace>();

			foreach (var face in faces) {
				int i01 = Util.addMidpointToCache(face.q0, face.q1, ref verts, ref cache);
				int i02 = Util.addMidpointToCache(face.q0, face.q2, ref verts, ref cache);
				int i13 = Util.addMidpointToCache(face.q1, face.q3, ref verts, ref cache);
				int i23 = Util.addMidpointToCache(face.q2, face.q3, ref verts, ref cache);
				int icenter = Util.addMidpointToCache(face.q1, face.q2, ref verts, ref cache);

				faces2.Add(new SquareFace(face.q0, i01, i02, icenter));
				faces2.Add(new SquareFace(i01, face.q1, icenter, i13));
				faces2.Add(new SquareFace(i02, icenter, face.q2, i23));
				faces2.Add(new SquareFace(icenter, i13, i23, face.q3));
			}

			faces = faces2;
		}

		for (int i = 0; i < verts.Count; i++) {
			verts[i] = new Vector3(
				verts[i].normalized.x * scale.x + origin.x, 
				verts[i].normalized.y * scale.y + origin.y,
				verts[i].normalized.z * scale.z + origin.z);
		}

		foreach (var face in faces) {
			triangles.Add(face.q0);
			triangles.Add(face.q1);
			triangles.Add(face.q2);
			triangles.Add(face.q3);
			triangles.Add(face.q2);
			triangles.Add(face.q1);
		}


		GameObject obj = (GameObject)Instantiate (prefab);
		obj.name = "Cuboid Sphere";
		
		Mesh mesh = new Mesh ();
		
		mesh.vertices = verts.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateNormals ();
		
		for (int i = 0; i < mesh.vertices.Length; i++) {
			uvs.Add(Util.GenerateUV(mesh.vertices[i] - origin));
		}
		
		mesh.uv = uvs.ToArray ();
		
		MeshFilter filter = obj.GetComponent<MeshFilter> ();
		filter.mesh = mesh;
	}

	private struct Face {
		public int q0;
		public int q1;
		public int q2;

		public Face (int q0, int q1, int q2) {
			this.q0 = q0;
			this.q1 = q1;
			this.q2 = q2;
		}
	}

	private struct SquareFace {
		public int q0, q1, q2, q3;

		// q0 --- q1
		// |       |
		// |       |
		// q2 --- q3

		public SquareFace(int q0, int q1, int q2, int q3) {
			this.q0 = q0;
			this.q1 = q1;
			this.q2 = q2;
			this.q3 = q3;
		}
	}

	private static class Util {
		public static Vector3 getMidpoint(Vector3 a, Vector3 b) {
			return new Vector3(
				(a.x + b.x) / 2.0f,
				(a.y + b.y) / 2.0f,
				(a.z + b.z) / 2.0f);
		}

		public static int addMidpointToCache(int v1, int v2, ref List<Vector3> verts, ref Dictionary<long, int> cache) {
			bool firstIsSmaller = v1 < v2;
			long smaller = firstIsSmaller ? v1 : v2;
			long greater = firstIsSmaller ? v2 : v1;
			long key = (smaller << 32) + greater;

			int ret;
			if (cache.TryGetValue (key, out ret)) {
				return ret;
			}

			Vector3 vert1 = verts [v1];
			Vector3 vert2 = verts [v2];
			Vector3 middle = getMidpoint (vert1, vert2);

			int i = verts.Count; verts.Add (middle);
			cache.Add (key, i);
			return i;
		}

		public static Vector2 GenerateUV(Vector3 vert) {
			float u, v;

			u = Mathf.Atan2(vert.z, vert.x) / (2 * Mathf.PI);
			v = Mathf.Atan2(vert.y, Mathf.Sqrt(vert.x * vert.x + vert.z * vert.z)) / Mathf.PI + 0.5f;

			/*if (vert.x == 0 && vert.z == 0) {
				v = vert.y > 0 ? 0 : 1;
			} else {
				v = Mathf.Atan2(vert.y, Mathf.Sqrt(vert.x * vert.x + vert.z * vert.z)) / Mathf.PI + 0.5f;
			}*/

            return new Vector2 (u, v);
		}
	}
}
