namespace WebApi.Utilities
{
    /// <summary>
    /// 表示 API 响应的结果类。
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 获取或设置 API 的执行状态码。
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获取或设置 API 的消息内容。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置 API 返回的数据。
        /// </summary>
        public object Data { get; set; }

    }
}
