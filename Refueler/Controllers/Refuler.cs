using Microsoft.AspNetCore.Mvc;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace Refueler.Controllers
{
    [ApiController]
    public class PlaneRefuelController : ControllerBase
    {
        /// <summary>
        /// Service status for planes (key is the plane id, and value is a status).
        /// </summary>
        private static Dictionary<string, string> ServiceStatus = new();
        /// <summary>
        /// Fuel levels for planes (key is the plane id, and value is fuel levels).
        /// </summary>
        private static Dictionary<string, int> PlaneFuel = new();
        /// <summary>
        /// Fuel type for planes (key is the plane id, and value is a fuel type).
        /// </summary>
        private static Dictionary<string, string> PlaneFuelType = new();

        private readonly ILogger<PlaneRefuelController> _logger;

        public PlaneRefuelController(ILogger<PlaneRefuelController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Fueles specified plane depending on its current fuel level with a specified fuel type.
        /// </summary>
        /// <param name="id">Plane id.</param>
        /// <param name="fuelNow">Current fuel level.</param>
        /// <param name="fuelMax">Max fuel.</param>
        /// <param name="fuelType">Fuel name.</param>
        /// <returns>Ok response if everything went fine.</returns>
        [HttpPost]
        [Route("Refuel")]
        public async Task<IActionResult> RefuelNoFront(string id, int fuelNow, int fuelMax, string fuelType = "Simple")
        {
            if(!ServiceStatus.ContainsKey(id))
            {
                ServiceStatus.Add(id, "");
                await Console.Out.WriteLineAsync($"Couldn't find {id} plane in the ServiceStatus dictionary, adding.");
            }
            if (!PlaneFuel.ContainsKey(id))
            {
                PlaneFuel.Add(id, 0);
                await Console.Out.WriteLineAsync($"Couldn't find {id} plane in the PlaneFuel dictionary, adding.");
            }
            if (!PlaneFuelType.ContainsKey(id))
            {
                PlaneFuelType.Add(id, fuelType);
                await Console.Out.WriteLineAsync($"Couldn't find {id} plane in the PlaneFuelType dictionary, adding.");
            }

            ServiceStatus[id] = "running";
            await Console.Out.WriteLineAsync($"Setting service status to \"running\" for {id}.");

            PlaneFuel[id] = fuelNow;

            while (PlaneFuel[id] < fuelMax)
            {
                PlaneFuel[id] = Math.Min(PlaneFuel[id] + 5, fuelMax);
                await Task.Delay(500);
            }

            double fueled = fuelMax - fuelNow;
            ServiceStatus[id] = "waiting";
            await Console.Out.WriteLineAsync($"Setting service status to \"waiting\" for {id}.");
            await Console.Out.WriteLineAsync($"Plane {id} has been fueled by {fueled} successfully, sending result.");
            return Ok($"Plane {id} has been fueled by {fueled} successfully.");
        }

        /// <summary>
        /// Gets current fuel levels and type for the specified plane.
        /// </summary>
        /// <param name="id">Plane id.</param>
        /// <returns>Current fuel level and type in a OK response.</returns>
        [HttpGet]
        [Route("{id}/refueler/features")]
        public IActionResult FuelAmount(string id)
        {
            if (!PlaneFuel.TryGetValue(id, out int fuelAmount))
            {
                Console.WriteLine($"No such id ({id}) for fuel amount.");
                return BadRequest($"No such id ({id}) for fuel amount.");
            }
            if (!PlaneFuelType.TryGetValue(id, out string? fuelType))
            {
                Console.WriteLine($"No such id ({id}) for fuel type.");
                return BadRequest($"No such id ({id}) for fuel type.");
            }

            var status = new
            {
                quantity = fuelAmount,
                type = fuelType,
            };
            Console.WriteLine($"Returning fuel status for {id}\nquantity: {fuelAmount};\ntype: {fuelType}");
            return Ok(status);
        }

        /// <summary>
        /// Gets service status for the specified plane.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/refueler/status")]
        public IActionResult Status(string id)
        {
            if (!ServiceStatus.TryGetValue(id, out string? value))
            {
                Console.WriteLine($"No such id ({id})");
                return BadRequest($"No such id ({id})");
            }

            var status = new { status = value };
            Console.WriteLine(status);
            Console.WriteLine($"Returning service status for {id} \nstatus: {value};");
            return Ok(status);
        }
    }
}
