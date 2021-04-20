using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
/// Wavefront .obj file Utility class.
/// @author sanyo[JP]
/// @version 0.0.2
namespace SanyoLib
{
  public static class ObjFormatUtil
  {
    public static Mesh ToMesh(string filePath)
    {
      List<Vector3> vertices = new List<Vector3>();
      List<Color> colors = new List<Color>();
      List<Vector3> normalIndexs = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<Vector2> uvIndexs = new List<Vector2>();
      List<Vector2> uvs = new List<Vector2>();
      List<int> triangles = new List<int>();
      //Read Obj
      using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
      {
        while (sr.Peek() >= 0)
        {
          string line = sr.ReadLine();
          string[] elements = line.Split(' ');
          float[] floats = Array.ConvertAll<string, float>(elements, i => { float v; float.TryParse(i, out v); return v; });

          if (elements[0].Equals("v"))
          {
            vertices.Add(new Vector3(floats[1], floats[2], floats[3]));
            if(elements.Length > 6)
              colors.Add(new Color(floats[4], floats[5], floats[6]));
          }
          else if (elements[0].Equals("vn")){
            normalIndexs.Add(new Vector3(floats[1], floats[2], floats[3]));
            normals.Add(new Vector3(floats[1], floats[2], floats[3]));
          }else if (elements[0].Equals("vt")){
            uvIndexs.Add(new Vector2(floats[1], floats[2]));
            uvs.Add(new Vector2(floats[1], floats[2]));
          }
          else if (elements[0].Equals("f"))
          {
            if (elements.Length > 4)
              throw new System.FormatException("not a triangle..");

            for(int i=3; i > 0; i--){
              string[] elem1 = elements[i].Split('/');
              int index = int.Parse(elem1[0])-1;
              triangles.Add(index);
              if(elem1.Length > 1 && index < uvIndexs.Count)
                uvs[index] = uvIndexs[int.Parse(elem1[1])-1];
              if(elem1.Length > 2 && index < normalIndexs.Count)
                normals[index] = normalIndexs[int.Parse(elem1[2])-1];
            }
          }
        }
      }
      //to Mesh
      Mesh mesh = new Mesh();
      mesh.name = filePath;
      mesh.vertices = vertices.ToArray();
      mesh.uv = uvs.ToArray();
      mesh.triangles = triangles.ToArray();
      mesh.normals = normals.ToArray();
      mesh.colors = colors.ToArray();
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
      return mesh;
    }

    public static void ToObjFile(Mesh mesh, string filePath)
    {
      Vector3[] vertices = mesh.vertices;
      Color[] colors = mesh.colors;
      Vector3[] normals = mesh.normals;
      Vector2[] uvs = mesh.uv;
      int[] triangles = mesh.triangles;

      StringBuilder sb = new StringBuilder("# Created by SanyoLib.ObjFormatUtil v0.0.1\n");
      //Vertex
      for (int i = 0; i < vertices.Length; i++)
      {
        Vector3 vector = vertices[i];
        if (colors != null && colors.Length > i)
        {
          Color color = colors[i];
          sb.AppendFormat("v {0} {1} {2} {3} {4} {5}\n", vector.x, vector.y, vector.z, color.r, color.g, color.b);
        }
        else sb.AppendFormat("v {0} {1} {2}\n", vector.x, vector.y, vector.z);
      }
      
      //Normal
      if(normals != null)
        foreach (Vector3 vector in normals)
          sb.AppendFormat("vn {0} {1} {2}\n", vector.x, vector.y, vector.z);
      
      //UV
      for (int i = 0; i < uvs.Length; i++)
        sb.AppendFormat("vt {0} {1}\n", uvs[i].x, uvs[i].y);
      
      //Triangle
      for (int i = 0; i < triangles.Length; i += 3)
        sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i + 2]+1, triangles[i + 1]+1, triangles[i]+1);
      
      //Save File
      using (StreamWriter streamWriter = new StreamWriter(filePath))
        streamWriter.Write(sb.ToString());
    }
  }
}