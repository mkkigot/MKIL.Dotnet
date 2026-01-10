

docker build -f MKIL.DotnetTest.OrderService.Dockerfile -t mkil.dotnettest.orderservice.api:latest .

docker build -f MKIL.DotnetTest.UserService.Dockerfile -t mkil.dotnettest.userservice.api:latest .



docker compose up kafka kafka-topic-init