

docker build -f MKIL.DotnetTest.OrderService.Dockerfile -t mkil.dotnettest.orderservice.api:latest .

docker build -f MKIL.DotnetTest.UserService.Dockerfile -t mkil.dotnettest.userservice.api:latest .

kafka-consumer-groups.sh --bootstrap-server localhost:9092 \
  --group order-service-user-consumer \
  --reset-offsets --to-latest \
  --topic user-created-events --execute