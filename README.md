### Setup and usage instructions
1. ensure you are the docker-compose.yml path ..\test\MKIL.DotnetTest\MKIL.DotnetTest\
2. start your docker daemon (open docker desktop)
3. run this cmd: docker compose up
4. to access the sites, you may refer to this links:
    > links:
    - Kakfa UI Dashboard        http://127.0.0.1:8080
    - UserService               http://127.0.0.1:5001/swagger
    - OrderService              http://127.0.0.1:5002/swagger
5. to test the application:
 > normal flow:

### Architecture overview and key decisions
- chose clean architecture instead of ntier
 > given my experience, i have always used ntier and the natural choice for me is to use ntier as well. but when i asked for the folder structure as a guide for implementation it gave me a different one and gave me this clean architecture approach to which i realized my team has also started using, i just didnt know that was it. i asked ai why it chose this architecture and it seemed reasonable and i also wanted to use practicing this as well.i dont just blindly follow every suggestion. i tend to pause and think and use it as an opportunity to learn. also, for the size of this application, multiple libraries isnt suited. better to combine it such that it would also be easier to do tdd

 
- features:
 > there's an api gateway for ease of user testing 
 > i have added a middleware (http request & reponse, correlation Id) for logging and to further improve the logging process and added a correlation id. I was intentional in having logs with a correlationId being passed neatly cause I believe this is important in dealing with microservices so that you can trace the flow 
 > using polly for service resiliency (during publish and consuming)
 > verify everything happening in the application through the logs

  **normal flow scenarios:**
    a. UserService API:
     - create a new user >> POST /api/Users 
     - test validation of user dto (it has validation for valid email address, duplicate email address)
     - go to Order Service API and verify that the new user is in the user cache (test by calling >> GET /api/Order/check/usercaches)
```
mkildotnettest-user-service-1   | [14:08:41 INF] UserService [11bdbc79-275f-47a0-ac8d-46a5c31c050b] HTTP Request POST /api/Users | Headers: {"Accept": "*/*", "Connection": "keep-alive", "Host": "127.0.0.1:5001", "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36", "Accept-Encoding": "gzip, deflate, br, zstd", "Accept-Language": "en-US,en;q=0.9", "Content-Type": "application/json", "Cookie": "***REDACTED***", "Origin": "http://127.0.0.1:5001", "Referer": "http://127.0.0.1:5001/swagger/index.html", "Content-Length": "97", "sec-ch-ua-platform": "\"Windows\"", "sec-ch-ua": "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"", "sec-ch-ua-mobile": "?0", "Sec-Fetch-Site": "same-origin", "Sec-Fetch-Mode": "cors", "Sec-Fetch-Dest": "empty"} | Body: {
mkildotnettest-user-service-1   |   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
mkildotnettest-user-service-1   |   "name": "string",
mkildotnettest-user-service-1   |   "email": "string@a.com"
mkildotnettest-user-service-1   | }
mkildotnettest-user-service-1   | [14:08:42 INF] UserService [11bdbc79-275f-47a0-ac8d-46a5c31c050b] Successfully published message 11bdbc79-275f-47a0-ac8d-46a5c31c050b to user-created-events at partition 0, offset 7
mkildotnettest-user-service-1   | [14:08:42 INF] UserService [11bdbc79-275f-47a0-ac8d-46a5c31c050b] HTTP Response POST /api/Users 200 | ContentType: application/json; charset=utf-8 | Body: "2cdfecda-4349-4931-b373-1d7a05d6615d"
mkildotnettest-order-service-1  | [14:08:42 INF] OrderService [11bdbc79-275f-47a0-ac8d-46a5c31c050b] Received message from user-created-events
mkildotnettest-order-service-1  | [14:08:43 INF] OrderService [11bdbc79-275f-47a0-ac8d-46a5c31c050b] Successfully processed and committed message for UserId: 2cdfecda-4349-4931-b373-1d7a05d6615d

...

mkildotnettest-order-service-1  | [14:10:20 INF] OrderService [7bbaa6c4-5c7e-450b-8320-71f9b678e741] HTTP Request POST /api/Order | Headers: {"Accept": "*/*", "Connection": "keep-alive", "Host": "127.0.0.1:5002", "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36", "Accept-Encoding": "gzip, deflate, br, zstd", "Accept-Language": "en-US,en;q=0.9", "Content-Type": "application/json", "Cookie": "***REDACTED***", "Origin": "http://127.0.0.1:5002", "Referer": "http://127.0.0.1:5002/swagger/index.html", "Content-Length": "162", "sec-ch-ua-platform": "\"Windows\"", "sec-ch-ua": "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"", "sec-ch-ua-mobile": "?0", "Sec-Fetch-Site": "same-origin", "Sec-Fetch-Mode": "cors", "Sec-Fetch-Dest": "empty"} | Body: {
mkildotnettest-order-service-1  |   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
mkildotnettest-order-service-1  |   "userId": "2cdfecda-4349-4931-b373-1d7a05d6615d",
mkildotnettest-order-service-1  |   "productName": "string",
mkildotnettest-order-service-1  |   "quantity": 1,
mkildotnettest-order-service-1  |   "price": 110
mkildotnettest-order-service-1  | }
mkildotnettest-order-service-1  | [14:10:20 INF] OrderService [7bbaa6c4-5c7e-450b-8320-71f9b678e741] Successfully published message 7bbaa6c4-5c7e-450b-8320-71f9b678e741 to order-created-events at partition 0, offset 5
mkildotnettest-user-service-1   | [14:10:20 INF] UserService [7bbaa6c4-5c7e-450b-8320-71f9b678e741] Received message from order-created-events
mkildotnettest-order-service-1  | [14:10:20 INF] OrderService [7bbaa6c4-5c7e-450b-8320-71f9b678e741] HTTP Response POST /api/Order 200 | ContentType: application/json; charset=utf-8 | Body: "7ce18122-698d-4920-b8e1-f03de75dffe4"
mkildotnettest-user-service-1   | [14:10:20 INF] UserService [7bbaa6c4-5c7e-450b-8320-71f9b678e741] Successfully processed saved user order:- 2

```

    b. OrderService API:
     - create a new order >> POST /api/Order
     - test validation of order dto (it has validation for quantity must be greater than 0, and userId must exists in the user cache)
     - go to User Service API and verify that the order is saved in this service (test by calling >> GET /api/User/{userId}/orders)

**error flow scenarios:**
    a. - call this endpoint /api/Users (or Order) /try-fail/permanent-error/consumer 
        > expectation: 
            - endpoint should successfully publish the message. 
            - when consumer receives it, it should receive a permanent error and should not retry
            - this will publish 2 failed messages. 
                a) "TEST PERMANENT ERROR" > will be caught in one of the conditions in the consumer 
                ``` throw new JsonException("From TEST PERMANENT ERROR") ``` 
                b) "TEST error msg" > will be caught inside the method **ProcessNewUser** when it tries to deserialize the message
            - it should show in the logs the flow from the api to the passing of the msg to the consumer. 
            - you should see 2 received messages and those messages should be published to the Dead Letter Queue
            
    b. - call this endpoint /api/Users (or Order) /try-fail/permanent-error/producer 
        > expectation: 
            - endpoint should NOT publish the message. 
            - endpoint should NOT retry in publishing the message 
        
    c. - call this endpoint /api/Users (or Order) /try-fail/transient-error/consumer 
        > expectation: 
            - endpoint should successfully publish the message. 
            - when consumer receives it, it should receive a permanent error and SHOULD RETRY
            - it should show in the logs the flow from the api to the passing of the msg to the consumer. 
            - verify in the logs that it should retry to process the Message
            - after the 3rd retry. it should publish the message to the Dead Letter Queue
            
    d. - call this endpoint /api/Users (or Order) /try-fail/transient-error/producer 
        > expectation: 
            - endpoint should NOT publish the message. 
            - endpoint should retry in publishing the message 
            - it should show in the logs the retry events 
            

    d. resiliency scenarios
     1) stop docker containers
     2) run docker compose up user-service order-service
       > expectation: services should still run but show something like:
```
  example log:
[14:14:22 ERR] UserService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:24 ERR] UserService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:33 ERR] UserService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
...

[14:14:21 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:22 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:25 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:35 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
``` 
     3) run docker compose up kafka kafka-topic-init
     4) call endpoints for user-service or order-service
        > expectation: services should resume 
       
```
 example log:
[14:14:22 ERR] UserService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:24 ERR] UserService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:33 ERR] UserService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:46 INF] UserService [] Partitions assigned: [0]
[14:16:36 INF] UserService [be928c2a-4c40-41e3-bcd3-a6ed7198b5f4] HTTP Request POST /api/Users | Headers: {"Accept": "*/*", "Host": "localhost:7125", "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36", "Accept-Encoding": "gzip, deflate, br, zstd", "Accept-Language": "en-US,en;q=0.9", "Content-Type": "application/json", "Cookie": "***REDACTED***", "Origin": "https://localhost:7125", "Referer": "https://localhost:7125/swagger/index.html", "Content-Length": "97", "sec-ch-ua-platform": "\"Windows\"", "sec-ch-ua": "\"Google Chrome\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"", "sec-ch-ua-mobile": "?0", "sec-fetch-site": "same-origin", "sec-fetch-mode": "cors", "sec-fetch-dest": "empty", "priority": "u=1, i"} | Body: {
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "string",
  "email": "string@a.com"
}
[14:16:36 INF] UserService [be928c2a-4c40-41e3-bcd3-a6ed7198b5f4] KafkaEventPublisher initialized with broker: 127.0.0.1:9092
%4|1768025797.071|GETPID|rdkafka#producer-2| [thrd:main]: Failed to acquire idempotence PID from broker localhost:9092/1: Broker: Coordinator load in progress: retrying
[14:16:45 INF] UserService [be928c2a-4c40-41e3-bcd3-a6ed7198b5f4] Successfully published message be928c2a-4c40-41e3-bcd3-a6ed7198b5f4 to user-created-events at partition 0, offset 8
[14:16:45 INF] UserService [be928c2a-4c40-41e3-bcd3-a6ed7198b5f4] HTTP Response POST /api/Users 200 | ContentType: application/json; charset=utf-8 | Body: "9ccc7772-e9b7-415b-b9c6-98c4e07e22e7"

----------------------------------------------------------------------------------------------------------

[14:14:22 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:25 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:35 ERR] OrderService [] Kafka Consumer Error: 1/1 brokers are down - Local_AllBrokersDown
[14:14:48 INF] OrderService [] Partitions assigned: [0]
[14:16:45 INF] OrderService [be928c2a-4c40-41e3-bcd3-a6ed7198b5f4] Received message from user-created-events
[14:16:45 INF] OrderService [be928c2a-4c40-41e3-bcd3-a6ed7198b5f4] Successfully processed and committed message for UserId: 9ccc7772-e9b7-415b-b9c6-98c4e07e22e7

```



### AI tools usage (prompts/approach if applicable)
- i am using claude and i used this is the first prompt i used. i picked some of the lines from the instructions file to help me focus on what i need to achieve:
 > im having a backend developer test assessment in dotnet. i want you to guide me what's the structure i need to implement to show best industry practice, dont give me the full code. i want to do the implementation myself. just give me some check lists and some test cases that i can do. here are some guidelines on what they are looking for and ensure that i show that: # Senior .NET Developer Take-Home Test Welcome! This take-home test is designed to evaluate your skills in building microservices with .NET Core, event-driven architecture, and modern development practices. **Key Requirements**: - ✅ Use Entity Framework Core with **in-memory database** - ✅ Publish a `UserCreated` event to Kafka when a user is created - ✅ Implement appropriate validation and error handling **Demonstrate the system working**: 1. Create a user via User Service 2. Create an order for that user via Order Service 3. Show that services communicate via events 4. Include any automated tests you create ## ✅ What We're Looking For ### Architecture & Design (40%) - **System Design**: How you structure the microservices - **Event-Driven Patterns**: Effective use of Kafka for service communication - **Code Organization**: Clean, maintainable code structure - **Problem-Solving**: How you handle ambiguity and make decisions ### Technical Implementation (40%) - **Modern .NET Practices**: Effective use of .NET Core, EF Core, and ecosystem - **Infrastructure**: Docker setup and service orchestration - **API Design**: Well-designed service interfaces - **Event Handling**: Robust event publishing and consumption ### Communication & Documentation (20%) - **Code Clarity**: Self-documenting code and meaningful comments - **README Quality**: Clear setup instructions and architectural decisions - **AI Usage Documentation**: How you leveraged AI tools (if applicable) - **Testing Strategy**: Your approach to ensuring code quality ## 💡 Bonus Considerations (Optional) Areas where you can demonstrate additional expertise: - **Observability**: Health checks, logging, monitoring - **Resilience**: Retry policies, circuit breakers, graceful degradation - **Configuration Management**: Environment-specific settings - **Testing**: Comprehensive test coverage and strategies ## ⏱️ Time Expectation This test is designed to take approximately **4-6 hours** for an experienced .NET developer. Focus on: 1. **Core functionality first** (working APIs + Kafka integration) 2. **Then add robustness** (error handling, testing, documentation) 3. **Finally, demonstrate expertise** (architectural choices, bonus features if time permits) **Remember**: We value working software and clear thinking over perfect implementation.
 
- then i saved the response of claude and created a new project in claude and added that as attachment. 
- i also attached the backend takehome assessment instructions file so that it has the context
- i added this as instructions:
        the techstack used:
        - dotnet
        - kafka
        - docker

        do not give me the full implementation, i want to practice also
        . just give me important code snippets how i can implement. 

 
- I used that initial prompt, then asked for specific code snippets on implementing features with best practices (logging, polly, etc). I don't blindly copy everything—I code the core stuff myself (domain, infrastructure) and stay hands-on with whatever I do use. It helps me think through things deliberately and reinforces concepts. Plus, my experience gives me preferences that help with troubleshooting and keeping code clean. I'd like to think of myself as a code reviewer when working with AI, not letting it be the captain of the ship. I want to stay involved and actually understand what I'm building. Also, in knowing myself more, 

- I want AI to guide my implementation so I can focus on the essentials. I tend to overcomplicate things instead of nailing the basics, so I use prompts as guardrails to spend reasonable time on this test while hitting what actually matters.

- also, since I didn't have experience implementing kafka i let it give me the code snippets how to implement it including the docker compose setup. despite that, i still edited some of what it gave me. The implementation of the features i wanted under MKIL.DotnetTest.Shared.Lib I let ai do, but I still did the overall design and the flow and was being intentional on how the flow is showing in the logs. i made changes to ensure that the correlation Id was passed properly (set or unset from the X-Correlation-ID field in swagger) and that the important details are showing in the logs 

- I come in with my own assumptions and approaches, then ask AI for a different perspective. If it's better, I use it. If my approach makes more sense, I stick with that. It helps me learn new things and make better decisions.


### Any assumptions or trade-offs made
 > i used clean architecture instead of ntier

 > some code files, combined interface and class


 > Resilience Strategy: Currently using Polly retry policy with exponential backoff. When I initially prompted in ai for this, it gave me a custom retry handler. Then I discovered Polly, which is open source and widely used. Better to use a reliable library then rely on custom ai generated retry handler

**Future Improvements:**
- Add circuit breaker pattern to prevent cascade failures
- Implement bulkhead isolation for thread pool protection
- Add timeout policies for long-running operations

**Why not circuit breaker now?**
- Application scope is limited
- Retry policy sufficient for transient failures
- Would add in production for high-traffic scenarios
