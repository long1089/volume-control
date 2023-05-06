using Microsoft.AspNetCore.SignalR;

namespace VolumeControl.SignalR.Server
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Identity?.Name;
        }
    }
}
