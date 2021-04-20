using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
/// GamePad Utility.
/// @author sanyo[JP]
/// @version 0.0.1
namespace SanyoLib.GamePad
{
  public class GamePadUtil
  {
    public static float[] AllAxis {get;private set;} = new float[6];

    private static Process process;

    public static void Init() => Init(25);

    public static void Init(int millsecounds)
    {
      if(process != null)
        return;
      process = new Process();
      process.StartInfo.FileName = Application.dataPath + "/../UserData/_scripts/SanyoLib/GamePad/JoystickCapture.exe";
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.Arguments = $"{millsecounds}";
      process.OutputDataReceived += (sender, e) => AllAxis = Array.ConvertAll(Array.ConvertAll(e.Data.Split(','), float.Parse),i => (i-0.5f)*2);
      process.Start();
      process.BeginOutputReadLine();
    }

    public static void Dispose()
    {
      process.Kill();
      process.Close();
    }

    private static Dictionary<string, Func<float>> AxisFuncs =  new Dictionary<string, Func<float>>(){
      {"Axis1", () => AllAxis[0]},
      {"Axis2", () => AllAxis[1]},
      {"Axis3", () => AllAxis[2]},
      {"Axis4", () => AllAxis[3]},
      {"Axis5", () => AllAxis[4]},
      {"Axis6", () => AllAxis[5]}
    };

    public static float GetAxis(string name) => AxisFuncs[name]();

    public static float DeadZone(float axis, float deadZone) => Mathf.Max(Mathf.Abs(axis)-deadZone, 0) / (1-deadZone) * (axis>0?1:-1);
  }
}