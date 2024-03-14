using Microsoft.AspNetCore.Mvc;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Refueler.Controllers
{
    [ApiController]
    public class PlaneRefuelController : ControllerBase
    {
        private static IPEndPoint? FrontendAddress;
        private static Dictionary<Guid, string> ServiceStatus = new();
        private static Dictionary<Guid, int> PlaneFuel = new();
        private static Dictionary<Guid, string> PlaneFuelType = new();

        private readonly ILogger<PlaneRefuelController> _logger;

        public PlaneRefuelController(ILogger<PlaneRefuelController> logger)
        {
            _logger = logger;
        }

/*        [HttpPost]
        [Route("Refuel")]
        public async Task<IActionResult> Refuel(Guid id, double fuelNow, double fuelMax)
        {
            if(FrontendAddress == null)
            {
                return BadRequest("Fronted address is not set!");
            }

            await Task.Delay(10000);

            double fueled = fuelMax - fuelNow;

            return Ok($"Plane {id} has been fueled by {fueled} successfully.");
        }*/

        [HttpPost]
        [Route("Refuel")]
        public async Task<IActionResult> RefuelNoFront(Guid id, int fuelNow, int fuelMax, string fuelType = "Simple")
        {
            if(!ServiceStatus.ContainsKey(id))
            {
                ServiceStatus.Add(id, "");
            }
            if (!PlaneFuel.ContainsKey(id))
            {
                PlaneFuel.Add(id, 0);
            }
            if (!PlaneFuelType.ContainsKey(id))
            {
                PlaneFuelType.Add(id, fuelType);
            }

            ServiceStatus[id] = "working";
            PlaneFuel[id] = fuelNow;

            while (PlaneFuel[id] < fuelMax)
            {
                PlaneFuel[id] = Math.Min(PlaneFuel[id] + 5, fuelMax);
                await Task.Delay(500);
            }

            double fueled = fuelMax - fuelNow;
            ServiceStatus[id] = "waiting";

            return Ok($"Plane {id} has been fueled by {fueled} successfully.");
        }


        [HttpGet]
        [Route("{id}/refueler/features")]
        public IActionResult FuelAmount(Guid id)
        {
            if (!PlaneFuel.TryGetValue(id, out int fuelAmount))
                return BadRequest($"No such id ({id}) for fuel amount.");
            if (!PlaneFuelType.TryGetValue(id, out string? fuelType))
                return BadRequest($"No such id ({id}) for fuel type.");

            JObject status = new()
            {
                { "quantity", fuelAmount },
                { "type", fuelType }
            };
            return Ok(status.ToString());
        }


        [HttpGet]
        [Route("{id}/refueler/status")]
        public IActionResult Status(Guid id)
        {
            if (!ServiceStatus.TryGetValue(id, out string? value))
                return BadRequest($"No such id ({id})");

            JObject status = new()
            {
                { "status", value }
            };
            return Ok(status.ToString());
        }


        [HttpPost]
        [Route("SetAddress")]
        public IActionResult SetAddress(string address, int port)
        {
            
            if (IPAddress.TryParse(address, out IPAddress ip))
            {
                FrontendAddress = new(ip, port);
                return Ok(FrontendAddress.ToString() + " is set successfully!");
            }
            else
            {
                return BadRequest(new FormatException($"address ({address}) is not a valid IP addresss.").Message);
            }
        }
    }
}
