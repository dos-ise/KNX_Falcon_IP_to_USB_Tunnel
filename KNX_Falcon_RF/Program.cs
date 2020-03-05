using System;

namespace KNX_Falcon_RF
{
  using System.Collections.Generic;
  using System.Linq;

  using Knx.Bus.Common.Configuration;
  using Knx.Bus.Common.KnxIp;
  using Knx.Falcon.Sdk;

  class Program
  {
    static void Main(string[] args)
    {
      var ip = new DiscoveryClient(AdapterTypes.All).Discover().FirstOrDefault();
      var usb = UsbDeviceEnumerator.GetAvailableInterfaces(TimeSpan.FromSeconds(2));
      ConnectorParameters selectedConnector = usb.First();
      using (var bus = new Bus(selectedConnector))
      {
        using (var bus2 = new Bus(new KnxIpTunnelingConnectorParameters(ip.IpAddress.ToString(), 0x0e57, false)))
        {
          bus.Connect();
          bus2.Connect();

          WriteLine("BUS1: Connected to " + bus.OpenParameters);
          WriteLine("BUS2: Connected to " + bus2.OpenParameters);


          bus.GroupValueReceived += args =>
            {
              if (bus.LocalIndividualAddress != args.IndividualAddress)
              {
                WriteLine(
                  "BUS1: " + "IndividualAddress: " + args.IndividualAddress + " Value: " + args.Value + " Address:"
                  + args.Address); 
                bus2.WriteValue(args.Address, args.Value, args.TelegramPriority);
              }
            };

          bus2.GroupValueReceived += args =>
          {
            if (bus2.LocalIndividualAddress != args.IndividualAddress)
            {
              WriteLine(
                "BUS2: " + "IndividualAddress: " + args.IndividualAddress + " Value: " + args.Value + " Address:"
                + args.Address);
              bus.WriteValue(args.Address, args.Value, args.TelegramPriority);
            }
          };

          while (true) { }
        }
      }
    }

    private static void WriteLine(string message = "")
    {
#if !DEBUG
      Console.WriteLine(message);
#endif
    }
  }
}
