using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoLib
{
  /// @author sanyo[JP]
  /// @version 0.0.1
  public static class Utility
  {
    // Get username
    public static string GetUserNameMCSD()
    {
      using (StreamReader stream = new StreamReader(Application.dataPath + "/../UserData/User.mcsd"))
        return Regex.Unescape(Regex.Match(stream.ReadToEnd(), "\"userName\":\"(?<name>.*?)\"").Groups["name"].Value);
    }

    // Get own machine name
    public static string GetMachineName() => GetMachine().parent.name;

    // Get own machine core object
    public static Transform GetMachineCore() => Array.Find(GameObject.FindGameObjectsWithTag("self"), i => (i.name == "Core"))?.transform;

    // Get own machine object
    public static Transform GetMachine() => Array.Find(SceneManager.GetActiveScene().GetRootGameObjects(), i => (i.transform.childCount > 0 && (i.transform.GetChild(0).name.StartsWith(i.name) || i.transform.GetChild(0).name.Equals("Limb0"))))?.transform?.GetChild(0);
    
    // Get player root objects
    public static GameObject[] GetPlayers() => Array.FindAll(SceneManager.GetActiveScene().GetRootGameObjects(), i => (i.transform.Find("model_1") != null));
    
    // Get player tags
    public static TagUtil[] GetTags() => Array.ConvertAll(Array.FindAll(SceneManager.GetActiveScene().GetRootGameObjects(), i => (i.name == "Tag(Clone)")), i => new TagUtil(i));

    // Degree and radian conversion
    public static double ToDeg(double rad) => (rad / Math.PI * 180);
    public static double ToRad(double deg) => (deg * Math.PI / 180);
    
    // Angle difference
    public static float DiffAngle(float a, float b)
    {
      float c = b - a;
      c -= (float)Math.Floor(c / 360f) * 360f;
      c -= (c > 180f ? 360f : 0);
      return c;
    }
  }
}