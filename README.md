# Nitro
Nitro is a Clean Architecture Project for .Net 6 (API Project) that provides essential features that every project needs. These features include Architecture and connection between libraries, Data Access, Security, Log events, Cache, Localization, File storage, Setting, And unit test.

## Architecture
Nitro was built with a clear separation of concerns, following the clean architecture approach. The application layers are the following:

1. Presentation Layer: This layer is mainly responsible for handling requests from the user, and responding accordingly.
2. Application Layer: This layer is responsible for handling the business logic and orchestrating the calls between the different layers.
3. Domain Layer: This layer is responsible for containing the domain models, domain services, and domain events.
4. Infrastructure Layer: This layer is responsible for handling the data access, logging, caching and other technical concerns.

## Connection between libraries
Nitro is composed of multiple NuGet packages, each of them containing specific functionality. The connection between these libraries is managed by dependency injection, making it easy to switch out dependencies depending on the project's needs.

## Data Access
Nitro uses the Entity Framework Core to access the database. It also supports database migrations and seeding, making it easy to keep the database up to date.

## Security
Nitro features authentication and authorization using the IdentityServer4 library. It also supports JWT tokens, making it easy to secure the API.

## Logging
Nitro uses NLog for logging, making it easy to view the application's logs.

## Cache
Nitro uses the Redis cache for caching, making it easy to retrieve data quickly.

## Localization
Nitro supports localization using the .NET Core Localization library, making it easy to support multiple languages.

## File Storage
Nitro supports file storage using Azure Blob Storage, making it easy to store and retrieve files.

## Settings
Nitro supports configuration settings using the .NET Core Configuration library, making it easy to manage settings.

## Unit Testing
Nitro supports unit testing using the XUnit library, making it easy to write tests for the application.

## Conclusion
Nitro is a Clean Architecture Project for .Net 6 (API Project) that provides essential features that every project needs. These features include Architecture and connection between libraries, Data Access, Security, Log events, Cache, Localization, File storage, Setting, And unit test. It is built using the latest technologies, making it easy to maintain and develop.


<strong>A Clean Architecture Project for .Net 6 (API Project)</strong>

The essential features that every project needs:

1. Architecture and connection between libraries
2. Database Context
3. Migration
4. Repository
5. Security
6. Log events
7. Cache
8. Bulk Insert
9. Localization
10. Blob storage
11. Setting
