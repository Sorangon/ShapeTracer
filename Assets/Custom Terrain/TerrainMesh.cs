using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainMesh : MonoBehaviour
{
    #region Attributes

    [SerializeField] private Material _material = null;
    private Mesh plane = null;
    [SerializeField] private Vector2Int _size = new Vector2Int(5, 5);

    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    #endregion

    #region Methods
    #region Lifecycle

    private void Start()
    {
        GenerateMesh(_size.x, _size.y);
        UpdateMesh();
    }

    private void Update()
    {
        
        DrawMesh();
        //plane = Plane(10, 10);
    }

    #endregion
    #region Private

    private void DrawMesh()
    {
        Graphics.DrawMesh(plane, transform.position, transform.rotation, _material,LayerMask.NameToLayer("Default"));
    }

    private void GenerateMesh(int xRes, int zRes)
    {
        vertices = new Vector3[(xRes + 1) * (zRes + 1)];
        uvs = new Vector2[vertices.Length];

        for(int i = 0, z = 0; z <= zRes; z ++)
        {
            for(int x = 0; x <= xRes; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * 2;
                vertices[i] = new Vector3(x, y, z);
                uvs[i] = new Vector2(x / xRes, z / zRes);

                i++;
            }          
        }

        triangles = new int[xRes * zRes * 6];

        int vert = 0;
        int tris = 0;

        for(int z = 0; z < zRes; z++)
        {
            for (int x = 0; x < xRes; x++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + xRes + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xRes + 1;
                triangles[tris + 5] = vert + xRes + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    private void UpdateMesh()
    {
        if(plane == null)
        {
            plane = new Mesh();
        }

        plane.Clear();
        plane.vertices = vertices;
        plane.uv = uvs;
        plane.triangles = triangles;
        plane.RecalculateNormals();
    }

    #endregion
    #endregion


}
