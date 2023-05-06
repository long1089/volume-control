using Microsoft.AspNetCore.SignalR;

namespace VolumeControl.SignalR.Server
{
    public class DeviceHub : Hub
    {
        ILogger<DeviceHub> logger;

        public DeviceHub(ILogger<DeviceHub> logger)
        {
            this.logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation($"OnConnectedAsync:{Context.UserIdentifier},{Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            logger.LogInformation($"OnDisconnectedAsync:{Context.UserIdentifier},{Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 用户发起 - 设置音量
        /// </summary>
        /// <returns></returns>
        public async Task SetDeviceVolume(int volume)
        {
            logger.LogInformation($"SetDeviceVolume:{Context.UserIdentifier},{Context.ConnectionId},{volume}");
            var userId = Context.UserIdentifier + ".client";
            await Clients.User(userId).SendAsync("SetDeviceVolume", volume);
        }

        /// <summary>
        /// 用户发起 - 设置静音
        /// </summary>
        /// <returns></returns>
        public async Task SetDeviceMute(bool muted)
        {
            logger.LogInformation($"SetDeviceMute:{Context.UserIdentifier},{Context.ConnectionId},{muted}");
            var userId = Context.UserIdentifier + ".client";
            await Clients.User(userId).SendAsync("SetDeviceMute", muted);
        }

        /// <summary>
        /// 用户发起 - 关机
        /// </summary>
        /// <returns></returns>
        public async Task Shutdown()
        {
            var userId = Context.UserIdentifier + ".client";
            await Clients.User(userId).SendAsync("Shutdown");
        }
    }
}
