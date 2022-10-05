﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace elefanti.video.backend.Controllers {
    [ApiController]
    [Route("controller")]
    [Authorize]
    public class TestController : ControllerBase {

        [HttpGet]
        public ActionResult Get() {
            return Ok(JsonConvert.SerializeObject(new { content = "Hello World - Elefanti Video - DevOps Finished 10/4/2022 02:31" }));
        }

        [HttpGet("{text}")]
        public ActionResult Get(string text) {
            return Ok(new { request = text });
        }
    }
}
