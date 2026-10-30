# Coding Standards

These standards apply to all code in this repository. Developers and AI tools (e.g. Claude) must follow them when implementing features and when reviewing Pull Requests. A change that violates a rule below should be fixed or flagged in review.

## 1. Architecture
### 1.1 Layers and dependency rules

There are five layers: **Api**, **Business**, **Domain**, **Infrastructure** and **Shared**.

| Layer | Responsibility | May depend on |
|---|---|---|
| Api | Controllers, filters, middlewares, API helpers. Only logic that is specific to this layer. | Business (through injected `*Manager` interfaces), Domain |
| Business | All business logic, covered by unit tests. | Other Business classes (through their `*Manager` interfaces), Infrastructure (through its related `*Repository` and `IUnitOfWork`), Domain, Shared |
| Domain | DTOs, requests/responses, application entities, enums, custom exceptions, constants. No logic. | Shared |
| Infrastructure | Connectors to external services (database, cache, other APIs, cloud services, etc.). No business logic. | Domain, Shared |
| Shared | Stateless helper methods and common utilities (e.g. `DateHelper`, `EnumHelper`). No business logic. | Nothing (base .NET libraries only) |

Rules:

- Api classes can only talk to Business, by injecting `*Manager` interfaces. They must never use Infrastructure directly.
- A Business class can talk to other Business classes only through their `*Manager` interfaces.
- A Business class accesses Infrastructure through its related `*Repository` class. When coordinating atomic transactions across multiple repositories without distributed transaction (MSDTC) escalation, Business classes inject `IUnitOfWork`.
- `*Repository` classes are the only access point to external services, and share the connection and transaction provided by `IUnitOfWork`.
- Each layer owns its own interfaces and implementations. An interface and its implementation always live in the same layer: `IXxxManager` and `XxxManager` in Business, `IXxxRepository` and `XxxRepository` in Infrastructure (as well as `IUnitOfWork` and `UnitOfWork`). Other layers consume the interface but never define it.

### 1.2 Naming

- Business classes are named `XxxManager`, with interface `IXxxManager` (e.g. `UsersManager`, `IUsersManager`).
- Infrastructure classes that connect to an external service are named `XxxRepository`, with interface `IXxxRepository` (e.g. `UsersRepository`, `IUsersRepository`). Transaction coordination is handled by `UnitOfWork` (`IUnitOfWork`).
- Test data helpers are static classes named `XxxHelper` in `ModularNet.UnitTests/Helpers`, with `GetXxx()` methods (e.g. `UsersHelper.GetUser()`, `AppSettingsHelper.GetAppSettings()`, `UserSettingsHelper.GetUserSetting()`).
- Do not add the `Async` suffix to method names (see 2.3).

### 1.3 Differences from Onion Architecture

ModularNet is inspired by, and very similar to, Onion Architecture (and Clean Architecture), but differs in these ways:

- **Repository abstractions.** In classic Onion, the Business layer defines the abstractions (`IXxxRepository`) and Infrastructure implements them. In ModularNet, the **Infrastructure layer defines `IXxxRepository`** and the Business layer consumes it.
- **Anemic Domain Model.** ModularNet does not follow DDD. The Domain layer contains only POCOs, with **no constructors and no logic**. This is the Anemic Domain Model: cleaner and with less "hidden magic".
- **Shared layer.** ModularNet includes a Shared layer (`ModularNet.Shared`), which Onion Architecture does not define.
- **DTO placement.** All DTOs, requests, and application entities are always placed in the **Domain** layer. In Onion, these application-specific objects would live in the Business layer.

### 1.4 Where logic lives

- **Api:** little logic. Only what is specific to that layer.
- **Domain:** no logic at all. It contains only POCOs (DTOs, requests, and application entities), enums, custom exceptions and constants, and it is consumed by all the other layers.
- **Infrastructure:** no business logic. It is a connector only.
- **Shared:** stateless, generic helper utilities only.
- **Business:** all the logic, covered by proper unit tests.

### 1.5 Forbidden libraries and patterns

- **AutoMapper must not be used.** Map objects explicitly.
- **CQRS must not be used** (no command/query handlers, no MediatR-style pipelines).

## 2. Code conventions

### 2.1 General

- ModularNet follows most of the standard .NET and C# conventions. The guiding principles are **Simplicity, Reusability, Clarity**.
- Instantiate objects with `var`.
- **Name every variable after its class**, so it is always clear, in every part of the code and in every Pull Request, which class the variable is. This applies to local variables, method parameters, fields and lambda parameters: the name is the class name in camelCase (`User` → `user`, `UserSetting` → `userSetting`, `IUsersRepository` → `usersRepository`).

  ```csharp
  var user = new User();
  ```

- When a variable holds a single object of a class, do not give it a generic or shortened name (`item`, `obj`, `data`, `result`, `u`, `us`). If you need more than one variable of the same class, keep the class name and add the role to it (e.g. `sourceUser` and `targetUser`).
- When a method or constructor takes two or more parameters of different classes (or interfaces), each parameter takes the name of its class (in camelCase), and the call passes variables that carry the same names. The reader can then tell which class each argument is, both in the method signature and at the call site.

  ```csharp
  // Real example in constructor injection (UsersSettingsManager.cs):
  public UsersSettingsManager(IUsersSettingsRepository usersSettingsRepository, ICacheManager cacheManager)
  {
      _usersSettingsRepository = usersSettingsRepository;
      _cacheManager = cacheManager;
  }

  // Real example in middleware (UserIdExtractionMiddleware.cs):
  public async Task InvokeAsync(HttpContext context, IUsersManager usersManager, ITokenHelper tokenHelper, ICacheManager cacheManager)
  {
      // ...
  }
  ```

  Similarly, when implementing new methods that accept domain entities or requests:

  ```csharp
  public async Task CreateAuditForUser(User user, ModularNetAudit modularNetAudit)
  {
      // ...
  }

  // Call site: the arguments have the same names as the parameters
  var user = await usersRepository.GetUserById(userId);
  var modularNetAudit = new ModularNetAudit { UserId = user.Id, AuditType = AuditType.Login };

  await auditsManager.CreateAuditForUser(user, modularNetAudit);
  ```

  Not allowed, because the names do not say which class each one is:

  ```csharp
  public async Task CreateAuditForUser(User u, ModularNetAudit a) { ... }

  var result = await auditsManager.CreateAuditForUser(item, data);
  ```

- Variables, parameters and fields must have clear, descriptive names. Do not use abbreviations (e.g. `userId`, not `uId`; `registerUserRequest`, not `req`). A value that is not a class instance (an id, a name, a count) is named after what it represents (`userId`, `secretName`).
- Method names must prefer clarity over brevity, and say what the method does and what it uses to do it. Use the pattern `Verb` + `Something` + `By` + `Criteria`, for example `GetUserById(Guid userId)` or `GetUserByEmail(string email)`.

### 2.2 Logging and exceptions

- Every method has its own log entry, and every exception is logged.
- Controllers catch exceptions, log the error details with the injected `ILogger`, and return appropriate HTTP error responses (e.g. `500 Internal Server Error`).
- Business and Infrastructure methods throw specific or domain exceptions (custom exceptions live in `ModularNet.Domain/CustomExceptions`, such as `EncryptionException` and `ValidationException`). Catch an exception only when the code can handle it, wrap it in a domain exception, or perform cleanup, and always log it when you do. If a centralized exception-handling middleware is added in the future, it should catch unhandled exceptions globally and map them to HTTP responses.

### 2.3 Async

- Almost everything is asynchronous: use `async`/`await` and return `Task`/`Task<T>` all the way from the controller down to the repository. Write synchronous code only when it is really necessary.
- Do not block on async code with `.Result` or `.Wait()`.
- **Do not use the `Async` suffix.** Because async is the default in this architecture, the suffix carries no information. This intentionally differs from the standard .NET convention. Name methods after what they do (e.g. `GetUserById`, not `GetUserByIdAsync`), except where a framework requires a specific name (e.g. `InvokeAsync` in middleware).

### 2.4 Warning signs

If any of these is true, the class is not following ModularNet principles and should be refactored:

- The class is approaching 1,000 lines of code.
- The class injects more than 5 dependencies.
- The class is specific to a single problem or tightly coupled to a single case.

## 3. Configuration

- The solution has **one** `appsettings.json`. It contains all the configuration needed to run the app, and the values that are the same in every environment.
- **Secrets must never be stored in `appsettings.json`.**
- A dedicated settings class (`AppSettings` via `AppSettingsManager`) retrieves the configuration. Values that differ between environments are overwritten through `SecretsManager`.

## 4. Tests

### 4.1 Tools

- Tests use **xUnit** and **Moq**. Do not introduce other test or mocking frameworks.
- Business logic must have unit tests (see 1.4).

### 4.2 Naming in tests

- Name each mock after the class or interface it replaces, followed by `Mock`, e.g. `usersRepositoryMock` for `IUsersRepository`. Do not use generic names such as `mock` or `sut`.
- Name the instance under test after its class, e.g. `usersManager`. Do not call it `sut`.

  ```csharp
  var usersRepositoryMock = new Mock<IUsersRepository>();
  var usersManager = new UsersManager(usersRepositoryMock.Object, ...);
  ```

### 4.3 Test data helpers

- Unit tests use a **helper** (e.g. `UsersHelper.GetUser()`, `AppSettingsHelper.GetAppSettings()`) that creates the object required by the test, so that you don't have to populate every property of the class each time. This makes tests more reliable: a populated object often contains data you are not aware of, and in a test those values are easy to miss.
- Every test calls the helper to get an instance of the needed object and overwrites **only** the properties relevant to that test. This keeps the test clear and self-descriptive.

### 4.4 BDD

- If the team uses BDD, each section of the test (Arrange, Act, Assert) contains exactly the corresponding lines of the BDD text. See [Why Your BDD Requirements Should Match Your Unit Tests Line by Line](https://mindrefactor.com/en/bdd-requirements-unit-test-alignment/).

## 5. How to add a feature

Work from the inside out, following the dependency rules in section 1:

1. **Domain:** add the DTOs, requests, entities and enums as POCOs in `ModularNet.Domain`, with no constructors and no logic.
2. **Infrastructure:** add `IXxxRepository` and `XxxRepository` in `ModularNet.Infrastructure` for each external service the feature needs. The repository only connects, with no business logic. Register it in `InfrastructureServiceRegistration.cs`.
3. **Business:** add `IXxxManager` and `XxxManager` in `ModularNet.Business` with all the logic. The manager uses its own `*Repository` and, if needed, other managers. Register it in `BusinessServiceRegistration.cs`.
4. **Api:** add or update the controller in `ModularNet.Api`. It injects the `IXxxManager`, with little or no logic, and returns the response.
5. **Tests:** add a helper in `ModularNet.UnitTests/Helpers` for each new Domain object the tests need, and unit tests for the manager in `ModularNet.UnitTests` that cover all the new logic.
6. Check the result against sections 1 to 4: logging in every method, async all the way, no `Async` suffix, no AutoMapper or CQRS, no secrets in `appsettings.json`.
