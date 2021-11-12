using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Security.Cryptography;

namespace SanyoLib.Network
{
  public class HttpServer
  {
    private IPEndPoint ServerIPEndPoint;
    private Thread ServerThread;
    private Socket ServerSocket;
    public string ServerPath = "./";
    public string ServerMimePath = Application.dataPath + "/../UserData/_scripts/SanyoLib/Network/mime.csv";
    Dictionary<string, Regex> MIME;
    public event Action<string, int> WebSocketEvent;
    public event Func<HttpHeader, HttpHeader, string> GetEvent;
    private string ServerName = "Machinecraft C#";


    private HashSet<HttpServerSession> Sessions = new HashSet<HttpServerSession>();

    public HttpServer(int port){
      this.ServerIPEndPoint = new IPEndPoint(IPAddress.Any, port);
    }

    public void Start()
    {
      if(this.ServerMimePath != null){
        this.MIME = Regex.Split(File.ReadAllText(ServerMimePath), $"\r\r\n|\r\n|\r")
          .Select(i => i.Split(','))
          .Where(i => i.Length >= 2)
          .ToDictionary(
              i => i[0],
              i => new Regex(string.Format("\\.({0})$", string.Join("|", i[1].Split(' '))))
          );
      }
      this.ServerThread = new Thread(ThreadRun);
      this.ServerThread.Start();
    }

    public void Stop()
    {
      this.ServerThread.Abort();
      this.ServerSocket.Close();
      foreach(HttpServerSession session in this.Sessions)
        session.Stop();
    }

    private void ThreadRun()
    {
      using (this.ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
      {
        this.ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        this.ServerSocket.Bind(this.ServerIPEndPoint);
        this.ServerSocket.Listen(32);
        Accept(this.ServerSocket.Accept());
      }
    }
 
    private void Accept(Socket socket){
      this.Sessions.Add(new HttpServerSession(this, socket));
      Accept(this.ServerSocket.Accept());
    }

    public class HttpHeader {
      public readonly Dictionary<int, string[]> StatusCodes = new Dictionary<int, string[]>(){
        {100, new string[]{"HTTP/1.1","100","Continue"}},
        {101, new string[]{"HTTP/1.1","101","Switching Protocol"}},
        {102, new string[]{"HTTP/1.1","102","Early Hints"}},

        {200, new string[]{"HTTP/1.1","200","OK"}},
        {201, new string[]{"HTTP/1.1","201","Created"}},
        {202, new string[]{"HTTP/1.1","202","Accepted"}},
        {203, new string[]{"HTTP/1.1","203","Non-Authoritative Information"}},
        {204, new string[]{"HTTP/1.1","204","No Content"}},
        {205, new string[]{"HTTP/1.1","205","Reset Content"}},
        {206, new string[]{"HTTP/1.1","206","Partial Content"}},
        {207, new string[]{"HTTP/1.1","207","Multi-Status"}},
        {208, new string[]{"HTTP/1.1","208","Already Reported "}},
        {226, new string[]{"HTTP/1.1","226","IM Used"}},

        {300, new string[]{"HTTP/1.1","300","Multiple Choice"}},
        {301, new string[]{"HTTP/1.1","301","Moved Permanently"}},
        {302, new string[]{"HTTP/1.1","302","Found"}},
        {303, new string[]{"HTTP/1.1","303","See Other"}},
        {304, new string[]{"HTTP/1.1","304","Not Modified"}},
        {305, new string[]{"HTTP/1.1","305","Use Proxy"}},
        {306, new string[]{"HTTP/1.1","306","unused"}},
        {307, new string[]{"HTTP/1.1","307","Temporary Redirect"}},
        {308, new string[]{"HTTP/1.1","308","Permanent Redirect"}},

        {400, new string[]{"HTTP/1.1","400","Bad Request"}},
        {401, new string[]{"HTTP/1.1","401","Unauthorized"}},
        {402, new string[]{"HTTP/1.1","402","Payment Required"}},
        {403, new string[]{"HTTP/1.1","403","Forbidden"}},
        {404, new string[]{"HTTP/1.1","404","Not Found"}},
        {405, new string[]{"HTTP/1.1","405","Method Not Allowed"}},
        {406, new string[]{"HTTP/1.1","406","Not Acceptable"}},
        {407, new string[]{"HTTP/1.1","407","Proxy Authentication Required"}},
        {408, new string[]{"HTTP/1.1","408","Request Timeout"}},
      };
      public int Status {get;set;}
      public string Method {get;private set;}
      public string Path {get;private set;}
      public string Protocol {get;private set;}
      public Dictionary<string, string> Header{get;set;}
      
      public HttpHeader(string[] method, Dictionary<string, string> header){
        this.Method = method[0];
        this.Path = Uri.UnescapeDataString(method[1]);
        if(method.Length > 2)
          this.Protocol = method[2];
        this.Header = header;
      }

      public HttpHeader(int code, Dictionary<string, string> header){
        this.Status = code;
        this.Header = header;
      }

      public void Send(Socket socket ,byte[] content){
        Dictionary<string, string> fixed2 = new Dictionary<string, string>(){{"Content-length", content.Length.ToString()}};
        Dictionary<string, string> headerDict = fixed2.Concat(this.Header)
        .GroupBy(i => i.Key,(_, j) => j.First()).ToDictionary(i => i.Key,i => i.Value);
        string[] header1 = new string[]{string.Join(" ",this.StatusCodes[this.Status])}.Concat(headerDict.Select((i, j) => $"{i.Key}: {i.Value}")).Concat(new string[]{"",""}).ToArray();
        socket.Send(Encoding.UTF8.GetBytes(string.Join("\r\n", header1)));
        socket.Send(content);
      }
      public void Send(Socket socket ,byte[] content, int code){
        this.Status = code;
        this.Send(socket, content);
      }
    }

    public class HttpServerSession {
      private Socket socket;
      public Thread SessionThread {get;private set;}
      private HttpServer server;
      public bool IsWebSocket {get;private set;} = false;
      public HttpServerSession(HttpServer server, Socket socket){
        this.server = server;
        this.socket = socket;
        this.SessionThread = new Thread(ThreadRun);
        this.SessionThread.Start();
      }

      public void Stop(){
        this.server.Sessions.Remove(this);
        this.SessionThread.Abort();
        this.socket.Close();
      }

      private void ThreadRun()
      {
        byte[] buffer = new byte[4096];
        int recvLen = socket.Receive(buffer);
        if (recvLen <= 0)
          return;
        string[] request = Encoding.ASCII.GetString(buffer, 0, recvLen).Split('\n');
        string[] method = request[0].Split(' ');
        Regex HttpKV = new Regex("(?<key>[a-zA-Z\\-]*):(?<value>.*)( *?)");
        Dictionary<string, string> dict = request.Skip(1).Where(i => (i != "")).Select(i => HttpKV.Match(i)).ToDictionary(i => i.Groups["key"].Value.Trim().ToLower(), i =>  i.Groups["value"].Value.Trim());
        if(method[0] == "GET"){ 
          HttpHeader requestheader = new HttpHeader(method, dict);
          if(dict.ContainsKey("upgrade")){
            if(dict["upgrade"].ToLower() == "websocket")
              HttpWebSocket(socket, requestheader);
          }else{
            HttpGet(socket, requestheader);
          }
        }else{
          socket.Send(Encoding.UTF8.GetBytes(
            string.Join("\r\n", new string[]{
              "HTTP/1.1 400 Bad Request",
              "Content-type: text/html; charset=UTF-8",
              $"Server: {this.server.ServerName}",
              "Content-length: -1",
              "",""
            })
          ));
          socket.Send(Encoding.UTF8.GetBytes("<title>400 Bad Request</title>400 Bad Request"));
        }
        socket.Close();
        this.server.Sessions.Remove(this);
      }

      private void HttpWebSocket(Socket socket, HttpHeader request){
        this.IsWebSocket = true;
        socket.Send(Encoding.UTF8.GetBytes(
          string.Join("\r\n", new string[]{
            "HTTP/1.1 101 Switching Protocols",
            "Connection: Upgrade",
            "Upgrade: websocket",
            string.Format("Sec-WebSocket-Accept: {0}", Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(request.Header["sec-websocket-key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")))),
            "",""
          })
        ));
        for(;;){
          if(socket.Connected == false)
            break;
          try{
          byte[] buffer = new byte[8192];
          int recvLen = socket.Receive(buffer);
          if (recvLen <= 0)
            return;
            
          int opcode = (buffer[0] & (1<<4)-1);
          if(opcode == 0x8)// Close
            break;
          if(opcode == 0x9)// Ping
            continue;
          if(opcode == 0xA)// Pong
            continue;

          int peyload = (buffer[1]-128);
          buffer = buffer.Skip(2).ToArray();
          recvLen -= 2;
          if(peyload == 126){
            buffer = buffer.Skip(2).ToArray();
            recvLen -= 2;
          }
          if(peyload == 127){
            buffer = buffer.Skip(8).ToArray();
            recvLen -= 8;
          }

          recvLen -= 4;
          byte[] mask = buffer.Take(4).ToArray();
          byte[] encoded = buffer.Skip(4).ToArray();
          string response = WebSocketMaskDecode(encoded, mask, recvLen);
          this.server.WebSocketEvent.Invoke(response, this.GetHashCode());
          }catch(Exception e){}
        }
      }

      private string WebSocketMaskDecode(byte[] bytes, byte[] mask, int size){
        for (int i = 0; i < bytes.Length; i++) {
          bytes[i] = (byte)(bytes[i] ^ mask[i % 4]);
        }
        return Encoding.UTF8.GetString(bytes, 0, size);
      }

      private byte[] WebSocketMaskEncode(string text, byte[] mask){
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        for (int i = 0; i < bytes.Length; i++) {
          bytes[i] = (byte)(bytes[i] ^ mask[i % 4]);
        }
        return bytes;
      }

      private void HttpGet(Socket socket, HttpHeader request){
        if(request.Method.Length < 3)
          return;
        string path = string.Join("", new string[]{server.ServerPath, request.Path});

        HttpHeader response = new HttpHeader(200,
          new Dictionary<string, string>(){
            {"Server", this.server.ServerName},
            {"Content-type", "text/html; charset=UTF-8"},
          }
        );

        string eventResponce = this.server.GetEvent.Invoke(request, response);
        if(eventResponce != null){
          response.Send(socket, Encoding.UTF8.GetBytes(eventResponce));
          return;
        }

        if(!Path.GetFullPath(path).StartsWith(Path.GetFullPath(server.ServerPath)))
          return;

        if(!File.Exists(path))
          foreach(string adder in new string[]{"index.htm", "index.html"})
            if(File.Exists(path + adder)){
              path = path + adder;
              break;
            }
        if(!File.Exists(path)){
          byte[] content = Encoding.UTF8.GetBytes("<!DOCTYPE html><html><head><meta charset=\"UTF-8\" /><meta name=\"robots\" content=\"noindex\" /><title>404 Not Found</title><style>html,body{display:flex;height:100%;width:100%;padding:0;margin:0;}body{align-items:center;justify-content:center;background-color:#983535;color:#ffffff;font-size:10vw;white-space:nowrap;font-family:sans-serif;}span,a{display:block;text-align:center;}footer{width:100%;position:absolute;bottom:0;}a{padding: 8px 0 8px 0;background-color:#ffffff;color:#000000;font-size:16px;font-weight:bold;}</style></head><body><div><span>404 Not Found :(</span></div><footer><a href=\"/\">Machinecraft C# SanyoLib.Network.HttpServer</a></footer></body></html>");
          response.Send(socket, content, 404);
        }else{
          string MIME = "application/octet-stream";
          try{MIME = server.MIME.First(i => i.Value.IsMatch(path)).Key;}catch(Exception e){}
          response.Header["Content-type"] = $"{MIME}; charset=UTF-8";
          response.Send(socket, File.ReadAllBytes(path), 200);
        }
      }
    }
  }
}