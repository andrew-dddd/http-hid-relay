using HttpHidRelay.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HttpHidRelay.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RelayController(IServiceProvider serviceProvider, IConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Returns a list of all connected HID devices
        /// </summary>
        /// <param name="vendorId">HID Device vendor ID</param>
        /// <param name="productId">HID Device product ID</param>
        /// <returns>Json with list of HID devices</returns>
        [HttpGet("/enumerate")]
        public IActionResult GetDevices([FromQuery] ushort vendorId = 0, [FromQuery] ushort productId = 0) => Ok(HidApi.Hid.Enumerate(vendorId, productId).ToList());

        /// <summary>
        /// Returns a list of relay names defined in configuration. 
        /// Names correspond to the order of relays on the device.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/relays")]
        public IActionResult GetRelays() => Ok(GetRelaysNames());

        /// <summary>
        /// Turns on relay
        /// </summary>
        /// <param name="relay">relay name</param>
        /// <returns></returns>
        [HttpPost("/set/{relay}")]
        public IActionResult SetRelay(string relay) => SetRelayState(relay, true);

        /// <summary>
        /// Turns off relay
        /// </summary>
        /// <param name="relay">relay name</param>
        /// <returns></returns>
        [HttpPost("/reset/{relay}")]
        public IActionResult UnsetRelay(string relay) => SetRelayState(relay, false);

        /// <summary>
        /// Clicks relay with specified click length
        /// </summary>
        /// <param name="relay">relay name</param>
        /// <param name="clickms">relay click length in milliseconds</param>
        /// <returns></returns>
        [HttpPost("/click/{relay}")]
        public async Task<IActionResult> Click(string relay, [FromQuery] int clickms = 200)
        {
            var result = SetRelayState(relay, true);
            if (result is BadRequestObjectResult)
            {
                return result;
            }

            if (clickms < 100 || clickms > 5000)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Click length out of range",
                    Detail = $"Click length must be between 100ms and 5000ms, got: {clickms}"
                });
            }

            await Task.Delay(TimeSpan.FromMilliseconds(clickms));
            return SetRelayState(relay, false);
        }


        private IActionResult SetRelayState(string relay, bool state)
        {
            var relayIndex = GetRelaysNames().IndexOf(relay, StringComparer.InvariantCultureIgnoreCase);

            return relayIndex switch
            {
                0 => SetRelay(0x01, state),
                1 => SetRelay(0x02, state),
                2 => SetRelay(0x03, state),
                3 => SetRelay(0x04, state),
                4 => SetRelay(0x05, state),
                5 => SetRelay(0x06, state),
                6 => SetRelay(0x07, state),
                7 => SetRelay(0x08, state),
                _ => NotFound(new ProblemDetails()
                {
                    Title = "Wrong relay",
                    Detail = $"Unable to find relay with id: {relay}"
                })
            };
        }

        private string[] GetRelaysNames() => configuration.GetValue<string>("RelayConfig:Relays")?.Split(',') ?? Array.Empty<string>();

        private IActionResult SetRelay(byte relay, bool state)
        {
            using var scope = serviceProvider.CreateScope();
            using var relayService = scope.ServiceProvider.GetRequiredService<RelayService>();

            relayService.SetRelayState(relay, state);
            return Ok();
        }
    }
}
