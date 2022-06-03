#### Onion Architecture

- A form of layered architecture and we can visualize these layers as concentric circles;
- First introduced by Jeffrey Palermo, to overcome the issues of the traditional N-layered architecture approach;
- It's split the architecture into 4 layers;
	  • Domain Layer
	  • Service Layer
	  • Infrastructure Layer
	  • Presentation Layer
- Conceptually, we can consider that the Infrastructure and Presentation layers are on the same level of the hierarchy;

<img src="https://www.macoratti.net/20/05/net_onion11.jpg" alt="">

#### Advantages of the Onion Architecture

- All of the layers interact with each other strictly through the interfaces defined in the layers below.
- The flow of dependencies is towards the core of the Onion.
- Using dependency inversion throughout the project, depending on abstractions (interfaces) and not the implementations,
  allows us to switch out the implementation at runtime transparently.
- We are depending on abstractions at compile-time, which gives us strict contracts to work with, and we are being
  provided with the implementation at runtime;
- Testability is very high with the Onion architecture because everything depends on abstractions.
- The abstractions can be easily mocked with a mocking library such as Moq.
- We can write business logic without concern about any of the implementation details;

#### Flow of Dependencies

- The main idea behind the Onion architecture is the flow of dependencies, or rather how the layers interact with each
  other.
- The Domain layer does not have any direct dependencies on the outside layers. It is isolated, in a way, from the
  outside world;
- The outer layers are all allowed to reference the layers that are directly below them in the hierarchy.
- The flow of dependencies dictates what a certain layer in the Onion
  architecture can do. Because it depends on the layers below it in the
  hierarchy, it can only call the methods that are exposed by the lower
  layers.
- We can use lower layers of the Onion architecture to define contracts or
  interfaces. The outer layers of the architecture implement these
  interfaces. This means that in the Domain layer, we are not concerning
  ourselves with infrastructure details such as the database or external
  services.
- Using this approach, we can encapsulate all of the rich business logic in
  the Domain and Service layers without ever having to know any
  implementation details. In the Service layer, we are going to depend only
  on the interfaces that are defined by the layer below, which is the Domain
  layer.

---

Links:

[Common web application architectures](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

[The Onion Architecture](https://jeffreypalermo.com/blog/the-onion-architecture-part-1/)

[The Repository Pattern](https://deviq.com/repository-pattern/)

[Architecting Microservices e-book](https://aka.ms/MicroservicesEbook)

[DDD (Domain-Driven Design)](https://docs.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)

[Onion Architecture in ASP.NET Core](https://code-maze.com/onion-architecture-in-aspnetcore/)


---
