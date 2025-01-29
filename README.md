**Project Structure**
1) _Mimo.Challenge.AppHost_: Orchestrates Mimo.Challenge.API project, Sqlite database and Sqlite Web UI container, provides a convenient Dashboard for viewing all telemetry of the distributed system
2) _Mimo.Challenge.API_: Entrypoint for all the incoming HTTP requests to the application
3) _Mimo.Challenge.DAL_: Contains the Data Access Layer (DAL) including the MimoContext and repository implementations.
4) _Mimo.Challenge.Domain_: Contains main business logic entities and service interfaces.
5) _Mimo.Challenge.Features_: Contains handlers for all features in the API (in this case feature = endpoint)

**System requirements**
1) .NET 9.0
2) An OCI compliant container runtime, such as:Docker Desktop or Podman.
3) An Integrated Developer Environment (IDE) or code editor, such as:
  3.1) Visual Studio 2022 version 17.9 or higher 
  3.2) Visual Studio Code
  3.3) C# Dev Kit: Extension
  3.4)JetBrains Rider with .NET Aspire plugin

**How to run**
1) Open the solution in one of the above mentioned IDEs
2) Run Mimo.Challenge.AppHost project
3) .NET Aspire dashboard website will open (if not, you can find a link in output console)
4) From this dashboard you can access the url to interactive Scalar API document of Mimo.Challenge.API, Sqlite Web UI, and also view logs, metrics and traces of the system

**How to add new achievements to the system**
All the developer needs to do is provide an implementation of IAchievementDeclaration interface and put in into Mimo.Challenge.Domain/Achievements folder
