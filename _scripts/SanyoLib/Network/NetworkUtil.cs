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
  public static class NetworkUtil
  {
    public static string GetLocalIP(){
      NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
      List<String> LocalIpAddress = new List<String>();
      foreach (NetworkInterface adapter in adapters)
      {
        if(adapter.OperationalStatus != OperationalStatus.Up)
          continue;
        if(adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
          continue;
        if(adapter.Description.Contains("Virtual"))
          continue;
        IPInterfaceProperties properties = adapter.GetIPProperties();
        for(int i=0; i < properties.UnicastAddresses.Count;i++){
          var address = properties.UnicastAddresses[i].Address;
          if (address.AddressFamily == AddressFamily.InterNetwork )//onry v4
            LocalIpAddress.Add(address.ToString());
        }
      }
      if(LocalIpAddress.Count == 1)
        return LocalIpAddress[0];
      return null;
    }
  }
}