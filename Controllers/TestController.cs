using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace elefanti.video.backend.Controllers {
    [ApiController]
    [Route("controller")]
    public class TestController : ControllerBase {

        [HttpGet]
        public ActionResult Get() {
            return Ok(JsonConvert.SerializeObject(new { content = "Hello World - Elefanti Video" }));
        }

        [HttpGet("{text}")]
        public ActionResult Get(string text) {
            return Ok(new { request = text });
        }
    }
}
