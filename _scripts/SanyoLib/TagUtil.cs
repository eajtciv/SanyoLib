using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoLib
{
  public class TagUtil {
    private TextMesh name;
    private TextMesh hpen;

    public TagUtil(GameObject gameObject){
      this.name = gameObject.transform.Find("Text").GetComponent<TextMesh>();
      this.hpen = gameObject.transform.Find("HpEn").GetComponent<TextMesh>();
    }

    private int GetInt(string text) => int.Parse(Regex.Match(text, "(<.*>)?(?<integer>[0-9]+)(<.*>)?").Groups["integer"].Value);
    //FFFFFF nomal
    //80FF80 script 
    //FFCC33 script+atacck 
    //FF8080 atacck
    public Color GetTagColor() => this.name.color;
    public string GetUserName() => this.name.text.Split('\n')[1];
    public string GetMachineName() => this.name.text.Split('\n')[0];
    public int GetHP() => GetInt(this.hpen.text.Split('\n')[0]);
    public int GetEnergy() => GetInt(this.hpen.text.Split('\n')[1]);
  }
}