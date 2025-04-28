namespace WebApi.Utilities
{
    public class ConnStringManager
    {
        private readonly IConfiguration _configuration;

        public ConnStringManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取指定名称的连接字符串
        /// </summary>
        /// <param name="name">连接字符串的名称</param>
        /// <returns>连接字符串</returns>
        public string GetConnString(string name = null)
        {
            try
            {
                string connString = _configuration.GetConnectionString(name);
                if (string.IsNullOrEmpty(connString))
                {
                    throw new InvalidOperationException($"未查询到：'{name}'");
                }
                return connString;
            }
            catch (Exception ex)
            {
                throw new Exception($"请确认编号：'{name}' 是否正确！", ex);
            }
        }
    }
}
