# 构建最新镜像
docker build -t plm_api:latest .

# 停止旧容器（如果存在）
docker stop plm_api_container

# 删除旧容器（如果存在）
docker rm plm_api_container

# 启动新容器
docker run -d -p 5000:80 --name plm_api_container plm_api:latest

Write-Host 'success'
