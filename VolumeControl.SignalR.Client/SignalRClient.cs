using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace VolumeControl.SignalR.Client
{
    public class SignalRClient
    {
        private string uid = $"U{new Random().Next(1000000):0000000}.client";
        private HubConnection? connection;

        public string Uid { get { return uid; } }

        public Action<int>? SetDeviceVolume { get; set; }

        public Action<bool>? SetDeviceMute { get; set; }

        public Action? Shutdown { get; set; }


        public void CreateConnection(string url)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
            connection.On<int>("SetDeviceVolume", (v) =>
            {
                try
                {
                    SetDeviceVolume?.Invoke(v);
                }
                catch (Exception)
                {

                }
            }); 
            connection.On<bool>("SetDeviceMute", (v) =>
            {
                try
                {
                    SetDeviceMute?.Invoke(v);
                }
                catch (Exception)
                {

                }
            });
            connection.On("Shutdown", () =>
            {
                Shutdown?.Invoke();
            });
            connection.StartAsync();
        }
    }
}