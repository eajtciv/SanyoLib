using System;
using System.Diagnostics;
using System.IO;
using System.Text;  
using System.Text.RegularExpressions;
/// Dir Utility class.
/// @author sanyo[JP]
/// @version 0.0.6
namespace SanyoLib
{
  public static class DirUtil
  {
    //フォルダ作成
    public static void Create(string path)
    {
      DosCommand(string.Format("mkdir \"{0}\"", path));
    }

    //フォルダ列挙
    public static string[] GetDirs(string path)
    {
      string results = DosCommand(string.Format("dir \"{0}\" /AD /B", Path.GetFullPath(path)));
      return Regex.Split(results, System.Environment.NewLine);
    }

    //フォルダの存在を確認
    public static bool Exists(string path)
    {
      path = Path.GetFullPath(path);
      string[] pathSpl = path.Split('\\');
      int pos = pathSpl.Length - (path.EndsWith("\\") ? 2 : 1);
      string name = pathSpl[pos];
      Array.Resize<string>(ref pathSpl, pos);
      string parnent = string.Join("\\", pathSpl);
      return (Array.IndexOf(GetDirs(parnent), name) >= 0);
    }
    
    //親フォルダを取得
    public static string GetParent(string path)
    {
      path = Path.GetFullPath(path);
      string[] pathSpl = path.Split('\\');
      Array.Resize<string>(ref pathSpl, pathSpl.Length - (path.EndsWith("\\") ? 2 : 1));
      return string.Join("\\", pathSpl);
    }
    
    //フォルダ名を取得
    public static string GetName(string path)
    {
      path = Path.GetFullPath(path);
      string[] pathSpl = path.Substring(0, path.Length - Convert.ToInt32(path.EndsWith("\\"))).Split('\\');
      return pathSpl[pathSpl.Length - 1];
    }

    //相対的なパスを取得
    public static string GetRelativePath(string basePath, string targetPath){
      string[] baseDirs = Path.GetFullPath(basePath).Split('\\');
      string[] targetDirs = Path.GetFullPath(targetPath).Split('\\');
      int index = 0;
      for(int i=0; i < Math.Min(baseDirs.Length, targetDirs.Length); i++){
        if(baseDirs[i].Equals(targetDirs[i], StringComparison.OrdinalIgnoreCase))
          index = i +   1;
        else
          break;
      }
      StringBuilder builder = new StringBuilder();
      builder.Insert(0, "../", baseDirs.Length - index);
      builder.Append(string.Join("/", targetDirs, index, targetDirs.Length - index));
      return builder.ToString();  
    }
    
    //Dosコマンドを実行
    private static string DosCommand(string command){
      using (Process process = new Process())
      {
        process.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.Arguments = string.Format("/c {0}", command);
        process.Start();
        string results = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return results;
      }
    }
  }
}