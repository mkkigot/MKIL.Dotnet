### Setup and usage instructions

### Architecture overview and key decisions
- chose clean architecture instead of ntier
 > given my experience, i have always used ntier and the natural choice for me is to use ntier as well. but as i have prompted ai, it gave me this clean architecture approach to which i realized my team has also started using, i just didnt know that was it. i've learned and have asked ai as well to explain why i should use this. i dont just blindly followe every suggestion. i tend to pause and think and use it as an opportunity to learn. also, for the size of this application, multiple libraries isnt suited. better to combine it such that it would also be easier to do tdd
 


### AI tools usage (prompts/approach if applicable)
- this was the prompt that i use as a main drive for the application 

 > im having a backend developer test assessment in dotnet. i want you to guide me what's the structure i need to implement to show best industry practice, dont give me the full code. i want to do the implementation myself. just give me some check lists and some test cases that i can do. here are some guidelines on what they are looking for and ensure that i show that: # Senior .NET Developer Take-Home Test Welcome! This take-home test is designed to evaluate your skills in building microservices with .NET Core, event-driven architecture, and modern development practices. **Key Requirements**: - ✅ Use Entity Framework Core with **in-memory database** - ✅ Publish a `UserCreated` event to Kafka when a user is created - ✅ Implement appropriate validation and error handling **Demonstrate the system working**: 1. Create a user via User Service 2. Create an order for that user via Order Service 3. Show that services communicate via events 4. Include any automated tests you create ## ✅ What We're Looking For ### Architecture & Design (40%) - **System Design**: How you structure the microservices - **Event-Driven Patterns**: Effective use of Kafka for service communication - **Code Organization**: Clean, maintainable code structure - **Problem-Solving**: How you handle ambiguity and make decisions ### Technical Implementation (40%) - **Modern .NET Practices**: Effective use of .NET Core, EF Core, and ecosystem - **Infrastructure**: Docker setup and service orchestration - **API Design**: Well-designed service interfaces - **Event Handling**: Robust event publishing and consumption ### Communication & Documentation (20%) - **Code Clarity**: Self-documenting code and meaningful comments - **README Quality**: Clear setup instructions and architectural decisions - **AI Usage Documentation**: How you leveraged AI tools (if applicable) - **Testing Strategy**: Your approach to ensuring code quality ## 💡 Bonus Considerations (Optional) Areas where you can demonstrate additional expertise: - **Observability**: Health checks, logging, monitoring - **Resilience**: Retry policies, circuit breakers, graceful degradation - **Configuration Management**: Environment-specific settings - **Testing**: Comprehensive test coverage and strategies ## ⏱️ Time Expectation This test is designed to take approximately **4-6 hours** for an experienced .NET developer. Focus on: 1. **Core functionality first** (working APIs + Kafka integration) 2. **Then add robustness** (error handling, testing, documentation) 3. **Finally, demonstrate expertise** (architectural choices, bonus features if time permits) **Remember**: We value working software and clear thinking over perfect implementation.
 
 > I used that initial prompt but then asked for specific code snippets on implementing features with best practices (logging, seq, etc). I don't blindly copy everything though I make sure to code the core stuff myself (domain, infra) and stay involved with anything I do use. It helps me think through things, being intentional and reinforce concepts to my brain.  For example, it suggested putting Validators in the API layer, but I moved them to the domain instead. I see the API as just a gateway to business logic, and validators are business logic to me. That way if I ever need to run UserService from a different gateway (desktop app, background service, whatever), the business logic is already contained and I can just reference the library.
 


 > the thought is that i want ai to guide me in my implementation and let me focus on the essentials. i recognize my tendency to over complicate things and not implementing the essentials. so i gave that prompt as a general guide so that i can spend reasonable time with this test while achieving the essentials, knowing 

 > i have assumptions or approach to do things, then i ask ai to give me a new perspective. if it's better i use it. if it doesn't make sense and i would think that my approach is better then i use that. 
 
 > helps me understand and learn new things. 



### Any assumptions or trade-offs made