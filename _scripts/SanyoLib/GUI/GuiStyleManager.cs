using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SanyoLib;

/// GUIStyleManager class.
/// @author sanyo[JP]
/// @version 0.0.1
namespace SanyoLib
{
  public class GUIStyleManager : Dictionary<string, GUIStyle>
  {
    public void LoadStyles(string path){
      this.Dispose();
      
      foreach(string dirName in DirUtil.GetDirs(path)){
        if(string.IsNullOrEmpty(dirName))
          continue;
        string dir = Path.Combine(path,dirName);
        try{
          GUIStyle style = new GUIStyle();
          
          Dictionary<string,Color> textColors = new Dictionary<string, Color>();
          string settingFile = string.Format("{0}/.style", dir);
          if(File.Exists(settingFile)){
            using (StreamReader sr = new StreamReader(settingFile)){
              while (sr.Peek() >= 0)
              {
                string line = sr.ReadLine();
                string[] element = line.Split('=');
                int[] param = new int[0];
                try{param = Array.ConvertAll<string, int>(element[1].Split(','), int.Parse);}catch{}
                float[] fparam = new float[0];
                try{fparam = Array.ConvertAll<string, float>(element[1].Split(','), float.Parse);}catch{}
                
                if(element[0].Equals("border")){
                  style.border = new RectOffset(param[0], param[1], param[2], param[3]);
                }else if(element[0].Equals("padding")){
                  style.padding = new RectOffset(param[0], param[1], param[2], param[3]);
                }else if(element[0].Equals("margin")){
                  style.margin = new RectOffset(param[0], param[1], param[2], param[3]);
                }else if(element[0].Equals("overflow")){
                  style.overflow = new RectOffset(param[0], param[1], param[2], param[3]);
                }else if(element[0].Equals("stretchHeight")){
                  style.stretchHeight = bool.Parse(element[1]);
                }else if(element[0].Equals("stretchWidth")){
                  style.stretchWidth = bool.Parse(element[1]);
                }else if(element[0].Equals("fixedWidth")){
                  style.fixedWidth = float.Parse(element[1]);
                }else if(element[0].Equals("fixedHeight")){
                  style.fixedHeight = float.Parse(element[1]);
                }else if(element[0].Equals("clipOffset")){
                  style.clipOffset = new Vector2(fparam[0], fparam[1]);
                }else if(element[0].Equals("contentOffset")){
                  style.contentOffset = new Vector2(fparam[0], fparam[1]);
                }else if(element[0].Equals("clipping")){
                  string name = element[1].ToLower();
                  if(name.Equals("clip"))
                    style.clipping = TextClipping.Clip;
                  else if(name.Equals("cverflow"))
                    style.clipping = TextClipping.Overflow;
                }else if(element[0].Equals("alignment")){
                  style.alignment = (TextAnchor) Enum.Parse(typeof(TextAnchor), element[1]);
                }else if(element[0].Equals("imagePosition")){
                  style.imagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition), element[1]);
                }else if(element[0].Contains(".")){
                  string[] kv = element[0].Split('.');
                  if(kv[1].ToLower().Equals("textcolor"))
                    textColors[kv[0]] = new Color(fparam[0], fparam[1], fparam[2]);
                }
              }
            }
          }
          // normal
          if(File.Exists(Path.Combine(dir, "normal.png")) || textColors.ContainsKey("normal")){
            style.normal = new GUIStyleState();
            style.normal.background = ReadTexture2D(Path.Combine(dir, "normal.png"));
            style.normal.textColor = textColors.ContainsKey("normal") ? textColors["normal"] : new Color(1, 1, 1);
          }
          if(File.Exists(Path.Combine(dir, "onNormal.png")) || textColors.ContainsKey("onNormal")){
            style.onNormal = new GUIStyleState();
            style.onNormal.background = ReadTexture2D(Path.Combine(dir, "onNormal.png"));
            style.onNormal.textColor = textColors.ContainsKey("onNormal") ? textColors["onNormal"] : new Color(1, 1, 1);
          }
          // hover
          if(File.Exists(Path.Combine(dir, "hover.png"))){
            style.hover = new GUIStyleState();
            style.hover.background = ReadTexture2D(Path.Combine(dir, "hover.png"));
            style.hover.textColor = new Color(1, 1, 1);
          }
          if(File.Exists(Path.Combine(dir, "onHover.png"))){
            style.onHover = new GUIStyleState();
            style.onHover.background = ReadTexture2D(Path.Combine(dir, "onHover.png"));
            style.onHover.textColor = new Color(1, 1, 1);
          }
          // active
          if(File.Exists(Path.Combine(dir, "active.png"))){
            style.active = new GUIStyleState();
            style.active.background = ReadTexture2D(Path.Combine(dir, "active.png"));
            style.active.textColor = new Color(1, 1, 1);
          }
          if(File.Exists(Path.Combine(dir, "onActive.png"))){
            style.onActive = new GUIStyleState();
            style.onActive.background = ReadTexture2D(Path.Combine(dir, "onActive.png"));
            style.onActive.textColor = new Color(1, 1, 1);
          }
          // focused
          if(File.Exists(Path.Combine(dir, "focused.png"))){
            style.focused = new GUIStyleState();
            style.focused.background = ReadTexture2D(Path.Combine(dir, "focused.png"));
            style.focused.textColor = new Color(1, 1, 1);
          }
          if(File.Exists(Path.Combine(dir, "onFocused.png"))){
            style.onFocused = new GUIStyleState();
            style.onFocused.background = ReadTexture2D(Path.Combine(dir, "onFocused.png"));
            style.onFocused.textColor = new Color(1, 1, 1);
          }
          style.name = SanyoLib.DirUtil.GetName(dirName);
          this[style.name] = style;
        }catch(Exception e){
        }
      }
    }
    
  
    public static Texture2D ReadTexture2D(string file)
    {
      if(File.Exists(file) == false)
        return null;

      try
      {
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(File.ReadAllBytes(file));
        return texture;
      } catch {
        return null;
      }
    }
    
    
    public GUIStyleState Create(Action<Texture2D> action,int width,int height, Color color){
      GUIStyleState state = new GUIStyleState();
      Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
      action.Invoke(texture);
      texture.Apply();
      state.background = texture;
      state.textColor = color;
      return state;
    }
    
    public void Dispose()
    {
      foreach(KeyValuePair<string, GUIStyle> pair in this){
        GUIStyle style = pair.Value;
        DisposeTexture2D(style.active, style.onActive);
        DisposeTexture2D(style.focused, style.onFocused);
        DisposeTexture2D(style.hover, style.onHover);
        DisposeTexture2D(style.normal, style.onNormal);
      }
    }
    
    private void DisposeTexture2D(params GUIStyleState[] states){
      foreach(GUIStyleState state in states)
        if(state != null)
          MonoBehaviour.Destroy(state.background);
    }
  }
}