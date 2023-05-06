using Microsoft.AspNetCore.Mvc;
using VolumeControl.SignalR.Server.Models;

namespace VolumeControl.SignalR.Server.Controllers
{
    public class RemoteDeviceController : Controller
    {
        public IActionResult Index(string uid="")
        {
            RemoteDeviceModel model = new RemoteDeviceModel();
            model.Uid = uid;
            return View(model);
        }
    }
}
