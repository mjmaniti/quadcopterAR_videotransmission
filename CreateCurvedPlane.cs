using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCurvedPlane : MonoBehaviour {
	
	private GameObject curved_plane;
	private GameObject left_plane;

	public float hori_angle = 90f;
	public float vert_angle = 60f;

	// Use this for initialization
	void Start () {
		
		Vector3[] cur_plane_v; // vertices
		int[] cur_plane_t; // triangles
		Vector2[] cur_plane_uv; // UV
		int n_row = 11;
		int n_col = 11;

		

		float r = 1f;

		curved_plane = new GameObject("Right_Curved_Plane");
		curved_plane.transform.parent = this.transform;
		curved_plane.layer = 10;
		MeshRenderer mr = curved_plane.AddComponent<MeshRenderer>();
		mr.material.shader = Shader.Find("Standard");
		//Texture2D tex = new Texture2D(1, 1);
		//tex.SetPixel(0, 0, Color.green);
		//tex.Apply();
		//mr.material.mainTexture = tex;
		mr.material.color = Color.white;
		//mr.material.EnableKeyword("_EMISSION");
		//mr.material.SetColor("_EmissionColor", Color.white);

		cur_plane_v = new Vector3 [n_row * n_col];
		cur_plane_t = new int[(n_row - 1) * (n_col - 1) * 2 * 3];
		cur_plane_uv = new Vector2[n_row * n_col];

		int t_count = 0;

		for (int i = 0; i < n_row; i++)
		{
			for (int j = 0; j < n_col; j++)
			{
				int v_index = i * n_col + j;
				float x = Mathf.Sin((j * hori_angle / (n_col - 1) - hori_angle / 2) / 180f * Mathf.PI) * r;
				float z_x = Mathf.Cos((j * hori_angle / (n_col - 1) - hori_angle / 2) / 180f * Mathf.PI) * r;
				float y = Mathf.Sin((i * vert_angle / (n_row - 1) - vert_angle / 2) / 180f * Mathf.PI) * r;
				float z_y = Mathf.Cos((i * vert_angle / (n_row - 1) - vert_angle / 2) / 180f * Mathf.PI) * r;
				cur_plane_v[v_index] = new Vector3(x, y, z_x * z_y);

				cur_plane_uv[v_index] = new Vector2((float)j / (float)(n_col-1), (float)i / (float)(n_row-1));
				//Debug.Log(v_index);
				//Debug.Log((float)j / (float)n_col);
				//Debug.Log((float)i / (float)n_row);
				// Debug.Log(cur_plane_uv[v_index]);
				//cur_plane_v[v_index] = new Vector3(j, i, 1);

				if (i != n_row - 1 && j != n_col - 1)
				{
					cur_plane_t[t_count++] = v_index;
					cur_plane_t[t_count++] = v_index + 1 + n_col;
					cur_plane_t[t_count++] = v_index + 1;
					cur_plane_t[t_count++] = v_index;
					cur_plane_t[t_count++] = v_index + n_col;
					cur_plane_t[t_count++] = v_index + 1 + n_col;
				}
			}
		}


		Mesh mesh = new Mesh();
		mesh.vertices = cur_plane_v;
		mesh.triangles = cur_plane_t;
		mesh.uv = cur_plane_uv;
		// mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(.5f, 0), new Vector2(1, 0), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(1, .5f), new Vector2(0, 1), new Vector2(.5f, 1), new Vector2(1, 1) };

		MeshFilter mf = curved_plane.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		curved_plane.transform.localScale = new Vector3(100, 100, 100);

		left_plane = Instantiate(curved_plane);
		left_plane.name = "Left_Curved_Plane";
		left_plane.transform.parent = this.transform;
		left_plane.layer = 9;
	}
	
	// Update is called once per frame
	void Update () {
		// left_plane.transform.position = curved_plane.transform.position;
	}
}
