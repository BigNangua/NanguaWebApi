using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.IService;
using WebApi.Utilities;


namespace WebApi.Controllers
{
    /// <summary>
    ///  测试接口
    /// </summary>
    [ApiController]
    [Route("api/Test/")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;

        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        /// <summary>
        /// get测试接口
        /// </summary>
        /// <returns></returns>
        [HttpGet("getTest")]
        [SwaggerOperation(Summary = "get测试接口", Description = "get测试接口")]
        public ApiResult getTest()
        {
            string data = _testService.getTest();
            return new ApiResult
            {
                Status = 200,
                Message = "",
                Data = data,
            };
        }

        /// <summary>
        /// post测试接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("postTest")]
        [SwaggerOperation(Summary = "post测试接口", Description = "post测试接口")]
        public ApiResult postTest(string boby)
        {
            string data = _testService.postTest(boby);
            return new ApiResult
            {
                Status = 200,
                Message = "",
                Data = data,
            };
        }
    }
}
