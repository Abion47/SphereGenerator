using UnityEngine;
using System.Collections;

public class SphereStatScript : MonoBehaviour {
	public int vertexCount;
	public int triangleCount;

	MeshFilter filter;
	Mesh mesh;

	// Use this for initialization
	void Start () {
		filter = GetComponent<MeshFilter> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (mesh == null) {
			mesh = filter.sharedMesh;

			if (mesh == null) {
				return;
			}
		}

		vertexCount = mesh.vertices.Length;
		triangleCount = mesh.triangles.Length / 3;
	}
}
