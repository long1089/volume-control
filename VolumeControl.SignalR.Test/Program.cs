using VolumeControl.SignalR.Client;

namespace VolumeControl.SignalR.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            SignalRClient client = new SignalRClient();
            client.CreateConnection("http://localhost:5000/devices?uid=CLH.client");

            Console.ReadKey();
        }
    }
}