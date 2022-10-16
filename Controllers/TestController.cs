using elefanti.video.backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;

namespace elefanti.video.backend.Controllers {
    [ApiController]
    [Route("controller")]
    public class TestController : ControllerBase {

        private readonly IElasticClient _elasticClient;

        public TestController(IElasticClient elasticClient) {
            _elasticClient = elasticClient;
        }

        [HttpGet("{text}")]
        public ActionResult Get(string text) {
            var result = _elasticClient.Search<Video>(r =>
                r.Query(q =>
                    q.Match(m => 
                        m.Field(f => f.Title)
                        .Fuzziness(Fuzziness.AutoLength(1, 5))
                        .Query(text)
                    )
                )
            );

            return Ok(result.Documents);
        }

        [HttpPost]
        public ActionResult Get([FromBody] VideoPost video) {
            var newVideo = new Video() {
                Title = video.Title,
                CategoryId = video.CategoryId,
                Id = video.Id,
                Description = video.Description,
            };
            var indexedDocument = _elasticClient.IndexDocument(newVideo);
            if (!indexedDocument.IsValid) {
                return BadRequest(indexedDocument.DebugInformation);
            }
            return Ok(indexedDocument.DebugInformation);
        }
    }
}
