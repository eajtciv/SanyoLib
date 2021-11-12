using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// Json .json file Utility class.
/// @author sanyo[JP]
/// @version 0.0.2
namespace SanyoLib  
{ 
  public class JsonUtil {
    public object Object {get;private set;} 

    // json file => object
    public static JsonUtil FromFile(string path){
      using (StreamReader stream = new StreamReader(path))
        return FromString(stream.ReadToEnd());
    }

    // json text => object
    public static JsonUtil FromString(string json){
      using (JsonParser parser = new JsonParser(json)){
        parser.Wrapper = true;
        return parser.Parse() as JsonUtil;
      }
    }


    // object => json text
    public static string ToJson(object obj, string spacer = null){
      JsonSerializer serializer = new JsonSerializer(spacer);
      serializer.Serialize(obj);
      return serializer.ToString();    
    }

    // object => json file
    public static void ToFile(object obj, string path, string spacer = null){
      using(StreamWriter stream = new StreamWriter(path))
        stream.Write(ToJson(obj, spacer));
    }

    public JsonUtil this[string key]
    {
      get{
        Dictionary<string, JsonUtil> dict = this.GetDictionary();
        return dict.ContainsKey(key) ? dict[key] : null;
      }  
    }


    public JsonUtil(object obj){
      this.Object = obj;
    }

    public Vector3 GetVector3(){
      Dictionary<string, JsonUtil> dict = this.GetDictionary();
      return new Vector3(dict["x"].GetFloat(), dict["y"].GetFloat(), dict["z"].GetFloat());
    }

    public Vector2 GetVector2(){
      Dictionary<string, JsonUtil> dict = this.GetDictionary();
      return new Vector2(dict["x"].GetFloat(), dict["y"].GetFloat());
    }

    public Color GetColor(){
      Dictionary<string, JsonUtil> dict = this.GetDictionary();
      return new Color(dict["r"].GetFloat()/255, dict["g"].GetFloat()/255, dict["b"].GetFloat()/255);
    }

    public KeyCode GetKeyCode() => (KeyCode)Enum.ToObject(typeof(KeyCode), this.GetInt());

    public Dictionary<string, JsonUtil> GetDictionary() => (Object as Dictionary<string, object>).ToDictionary(kvp => kvp.Key, kvp => (kvp.Value as JsonUtil));

    public List<JsonUtil> GetList() => (Object as List<object>).ConvertAll(i => i as JsonUtil);

    public string GetString() => Object as string;

    public bool GetBool() => (bool) Object;

    public int GetInt() => (int) this.GetLong();

    public float GetFloat() => (float) this.GetDouble();

    public double GetDouble(){
      if(Object is long)
        return (double)(long) Object;
      return (double) Object;
    }

    public long GetLong(){
      if(Object is double)
        return (long)(double) Object;
      return (long) Object;
    }

    public class JsonSerializer {
      private StringBuilder builder = new StringBuilder();
      private string spacer = null;
      private int indentation = 0;

      private Dictionary<char, string> escapes = new Dictionary<char, string>(){
        {'"', "\\\""},
        {'\\', "\\\\"},
        {'\b', "\\b"},
        {'\f', "\\f"},
        {'\n', "\\n"},
        {'\r', "\\r"},
        {'\t', "\\t"}
      };

      public override string ToString() => this.builder.ToString();

      public JsonSerializer(string spacer = null) {
        this.spacer = spacer;
      }

      public void Serialize(object obj) {
        if(obj is JsonUtil)
          obj = (obj as JsonUtil).Object;
        if (obj == null   )
          this.builder.Append("null");
        else if (obj is string)
          this.SerializeString(obj as string);      
        else if (obj is IDictionary)
          this.SerializeObject(obj as IDictionary);
        else if (obj is IList)
          this.SerializeArray(obj as IList);
        else if (obj is Enum)
          this.builder.Append((int) obj);
        else if (obj is Vector3)
          this.SerializeVector3((Vector3) obj);
        else if (obj is Vector2)
          this.SerializeVector2((Vector2) obj);
        else if (obj is Color)
          this.SerializeColor((Color) obj);
        else if (obj is bool)
          this.builder.Append((bool) obj ? "true" : "false");
        else {  
          if (obj is int || obj is uint
            || obj is long || obj is ulong
            || obj is byte || obj is sbyte
            || obj is short || obj is ushort) {
            this.builder.Append(obj);
          } else if (obj is float) {
            this.builder.Append(((float) obj).ToString("R"));
          } else if (obj is double || obj is decimal) {
            this.builder.Append(Convert.ToDouble(obj).ToString("R"));
          } else {
            this.SerializeString(obj.ToString());
          }
        }
      }

      private void SerializeColor(Color color){
        Dictionary<string, float> dict = new Dictionary<string, float>();
        dict["r"] = (int)(color.r*255);
        dict["g"] = (int)(color.g*255);
        dict["b"] = (int)(color.b*255);
        this.SerializeObject(dict as IDictionary);
      }

      private void SerializeVector2(Vector2 vector2){
        Dictionary<string, float> dict = new Dictionary<string, float>();
        dict["x"] = vector2.x;
        dict["y"] = vector2.y;
        this.SerializeObject(dict as IDictionary);
      }

      private void SerializeVector3(Vector3 vector3){
        Dictionary<string, float> dict = new Dictionary<string, float>();
        dict["x"] = vector3.x;
        dict["y"] = vector3.y;
        dict["z"] = vector3.z;
        this.SerializeObject(dict as IDictionary);
      }

      private void SerializeArray(IList array) {
        this.builder.Append('[');
        if(array.Count > 0){
          this.indentation++;
          if(this.spacer != null)
            this.builder.Append(Environment.NewLine).Insert(this.builder.Length, this.spacer, this.indentation);
          bool splitter = false;
          foreach (object obj in array) {
            if(splitter) {
              this.builder.Append(',');
              if(this.spacer != null)
                this.builder.Append(Environment.NewLine).Insert(this.builder.Length, this.spacer, this.indentation);
            } else
              splitter = true;
            this.Serialize(obj);
          }
          this.indentation--;
          if(this.spacer != null)
            this.builder.Append(Environment.NewLine).Insert(this.builder.Length, this.spacer, this.indentation);
        }
        this.builder.Append(']');
      }


      private void SerializeObject(IDictionary dict) {
        this.builder.Append('{');
        if(dict.Count > 0){
          this.indentation++;
          bool splitter = false;
          if(this.spacer != null)
            this.builder.Append(Environment.NewLine).Insert(this.builder.Length, this.spacer, this.indentation);
          foreach(DictionaryEntry entry in dict){
            if(splitter){
              this.builder.Append(',');
              if(this.spacer != null)
                this.builder.Append(Environment.NewLine).Insert(this.builder.Length, this.spacer, this.indentation);
            }
            else
              splitter = true;
            this.SerializeString(entry.Key.ToString());
            this.builder.Append(':');
            if(this.spacer != null)
              this.builder.Append(' ');
          
            this.Serialize(entry.Value);
          }
          this.indentation--;
          if(this.spacer != null)
            this.builder.Append(Environment.NewLine).Insert(this.builder.Length, this.spacer, this.indentation);
        }
        this.builder.Append('}');
      }

      private void SerializeString(string str) {
        this.builder.Append('\"');
        char[] chars = str.ToCharArray();
        foreach (char current in chars) {
          if(escapes.ContainsKey(current)){
            this.builder.Append(escapes[current]);
          } else {
            int code = Convert.ToInt32(current);
            if ((code >= 32) && (code <= 126)) {
              builder.Append(current);
            } else {
              this.builder.Append("\\u").Append(code.ToString("x4"));
            }
          }
        }
        this.builder.Append('\"');
      }
    }

    public class JsonParser : IDisposable {
      private StringReader stream;
      public bool Wrapper {get;set;} = false;

      public JsonParser(string json){
        this.stream = new StringReader(json);
      }

      public object Parse() => Parse(this.GetNextToken(true));

      private object Parse(char token) {
        if(this.Wrapper)
          return new JsonUtil(this._Parse(token));
        else
          return this._Parse(token);
      }

      private object _Parse(char token) {
        if(token == '"')
          return this.ParseString();
        else if(token == '{')
          return this.ParseObject();
        else if(token == '[')
          return this.ParseArray();
        else  if((token >= '0' && token <= '9') || token == '.' || token == '-')
          return this.ParseNumber(token);
        string raw = this.GetString(token);
        if(raw.Equals("true"))
          return true;
        if(raw.Equals("false"))
          return false;
        return null;
      }

      private object ParseNumber(char token) {
        string raw = this.GetString(token);
        if (raw.IndexOf('.') != -1) {
          double number;
          Double.TryParse(raw, out number);
          return number;
        } else {
          long number;    
          Int64.TryParse(raw, out number);
          return number;
        }
      }

      private List<object> ParseArray() {
        List<object> array = new List<object>();
        while (true) {
          char token = this.GetNextToken(true);
          if(token == ',')
            continue;
          if(token == ']' || this.IsNextble() == false)
            break;
          array.Add(this.Parse(token));
        }
        return array;
      }

      private Dictionary<string, object> ParseObject() {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        while (true) {
          char token = this.GetNextToken(true);
          if(token == ',')
            continue;
          if(token == '}' || this.IsNextble() == false)
            break;
          string key = this.ParseString();
          if(this.GetNextToken(true) != ':')
            return null;
          dict[key] = this.Parse();
        }
        return dict;
      }

      private string ParseString() {
        StringBuilder builder = new StringBuilder();
        while (true) {
          char current = this.GetNext();
          if(current == '"' || this.IsNextble() == false)
            break;
          else if(current == '\\')
            current = this.GetUnescapedChar();
          builder.Append(current);
        }
        return builder.ToString();
      }

      private char GetUnescapedChar() {
        char current = this.GetNext();
        if(current == 'b')
          return '\b';
        else if(current == 'f')
          return '\f';
        else if(current == 'n')
          return '\n';
        else if(current == 'r')
          return '\r';
        else if(current == 't')
          return '\t';
        else if(current == 'u'){
          char[] hex = new char[4];
          for (int i=0; i< 4; i++)
            hex[i] = this.GetNext();
          return (char) Convert.ToInt32(new string(hex), 16);
        }
        return current;
      }

      private char GetNextToken(bool advance = false) {
        char current;
        while (Char.IsWhiteSpace(current = this.Peek())) {
          this.stream.Read();
          if (this.IsNextble() == false)
            return current;
        }
        if(advance)
          this.GetNext();
        return current;
      }

      private bool IsNextble() => (this.stream.Peek() != -1);

      private char Peek() => Convert.ToChar(this.stream.Peek());

      private char GetNext() => Convert.ToChar(this.stream.Read());

      private string GetString(char token){
        StringBuilder builder = new StringBuilder();
        builder.Append(token);
        while ((",: {}[]".IndexOf(this.Peek()) != -1) == false) {
          char current = this.GetNext();
          if(current == '"' || this.IsNextble() == false)
            break;
          else if(current == '\\')
            current = this.GetUnescapedChar();
          builder.Append(current);
        }
        return builder.ToString();
      }

      public void Dispose() {
        this.stream.Dispose();
        this.stream = null;
      }
    }
  }
}