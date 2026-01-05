### Setup and usage instructions
> to access kafka ui dashboard, go to the link:  http://127.0.0.1:8080


### Architecture overview and key decisions
- chose clean architecture instead of ntier
 > given my experience, i have always used ntier and the natural choice for me is to use ntier as well. but as i have prompted ai, it gave me this clean architecture approach to which i realized my team has also started using, i just didnt know that was it. i've learned and have asked ai as well to explain why i should use this. i dont just blindly followe every suggestion. i tend to pause and think and use it as an opportunity to learn. also, for the size of this application, multiple libraries isnt suited. better to combine it such that it would also be easier to do tdd
 


### AI tools usage (prompts/approach if applicable)
- this was the prompt that i use as a main drive for the application 

 > im having a backend developer test assessment in dotnet. i want you to guide me what's the structure i need to implement to show best industry practice, dont give me the full code. i want to do the implementation myself. just give me some check lists and some test cases that i can do. here are some guidelines on what they are looking for and ensure that i show that: # Senior .NET Developer Take-Home Test Welcome! This take-home test is designed to evaluate your skills in building microservices with .NET Core, event-driven architecture, and modern development practices. **Key Requirements**: - ✅ Use Entity Framework Core with **in-memory database** - ✅ Publish a `UserCreated` event to Kafka when a user is created - ✅ Implement appropriate validation and error handling **Demonstrate the system working**: 1. Create a user via User Service 2. Create an order for that user via Order Service 3. Show that services communicate via events 4. Include any automated tests you create ## ✅ What We're Looking For ### Architecture & Design (40%) - **System Design**: How you structure the microservices - **Event-Driven Patterns**: Effective use of Kafka for service communication - **Code Organization**: Clean, maintainable code structure - **Problem-Solving**: How you handle ambiguity and make decisions ### Technical Implementation (40%) - **Modern .NET Practices**: Effective use of .NET Core, EF Core, and ecosystem - **Infrastructure**: Docker setup and service orchestration - **API Design**: Well-designed service interfaces - **Event Handling**: Robust event publishing and consumption ### Communication & Documentation (20%) - **Code Clarity**: Self-documenting code and meaningful comments - **README Quality**: Clear setup instructions and architectural decisions - **AI Usage Documentation**: How you leveraged AI tools (if applicable) - **Testing Strategy**: Your approach to ensuring code quality ## 💡 Bonus Considerations (Optional) Areas where you can demonstrate additional expertise: - **Observability**: Health checks, logging, monitoring - **Resilience**: Retry policies, circuit breakers, graceful degradation - **Configuration Management**: Environment-specific settings - **Testing**: Comprehensive test coverage and strategies ## ⏱️ Time Expectation This test is designed to take approximately **4-6 hours** for an experienced .NET developer. Focus on: 1. **Core functionality first** (working APIs + Kafka integration) 2. **Then add robustness** (error handling, testing, documentation) 3. **Finally, demonstrate expertise** (architectural choices, bonus features if time permits) **Remember**: We value working software and clear thinking over perfect implementation.
 
 > I used that initial prompt, then asked for specific code snippets on implementing features with best practices (logging, Seq, etc). I don't blindly copy everything—I code the core stuff myself (domain, infrastructure) and stay hands-on with whatever I do use. It helps me think through things deliberately and reinforces concepts. Plus, my experience gives me preferences that help with troubleshooting and keeping code clean. I'd like to think of myself as a code reviewer when working with AI, not letting it be the captain of the ship. I want to stay involved and actually understand what I'm building. Also, in knowing myself more, 

 > I want AI to guide my implementation so I can focus on the essentials. I tend to overcomplicate things instead of nailing the basics, so I use prompts as guardrails to spend reasonable time on this test while hitting what actually matters.

 > I come in with my own assumptions and approaches, then ask AI for a different perspective. If it's better, I use it. If my approach makes more sense, I stick with that. It helps me learn new things and make better decisions.








### Any assumptions or trade-offs made
 > i used clean architecture instead of ntier

 > some code files, combined interface and class