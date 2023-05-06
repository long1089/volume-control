using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net;

namespace VolumeControl.SignalR.Client
{
    public class SignalRClient
    {
        private string uid = $"U{new Random().Next(1000000):0000000}.client";
        private string url = string.Empty ;
        private HubConnection? connection;
        private bool forceClose = false;

        public string Uid { get { return uid; } }

        public Action<int>? SetDeviceVolume { get; set; }

        public Action<bool>? SetDeviceMute { get; set; }

        public Action? Shutdown { get; set; }


        public void CreateConnection(string url)
        {
            this.url = url;
            StartNewConnection();
        }

        private async void StartNewConnection() {
            try
            {
                if(forceClose)
                {
                    return;
                }
                Console.WriteLine("StartNewConnection");
                connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect(new TimeSpan[] {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(5),
                })
                .Build();
                connection.Reconnecting += async (error) =>
                {
                    Console.WriteLine("reconnecting");
                    await Task.CompletedTask;
                };
                connection.Reconnected += async (error) =>
                {
                    Console.WriteLine("reconnected");
                    await Task.CompletedTask;
                };
                connection.Closed += async (error) =>
                {
                    if (forceClose)
                    {
                        return;
                    }
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    StartNewConnection();
                };
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

                await connection.StartAsync();
            }
            catch (Exception)
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                StartNewConnection();
            }

        }

        public async void CloseConnection()
        {
            forceClose = true;
            if (connection != null && connection.State != HubConnectionState.Disconnected)
            {
                await connection.StopAsync();
            }
        }
    }
}