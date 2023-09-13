# Cooking recipe web page

Development of a cooking recipe web page.

## Project

[ASP.NET MVC Pattern](https://dotnet.microsoft.com/en-us/apps/aspnet/mvc) version 7.0.400. Database with SQLite and EF Core.

## Database

Install EF Core with command lines `dotnet tool uninstall --global dotnet-ef` then `dotnet tool install --global dotnet-ef`.
Create database with command lines `dotnet ef migrations add InitialCreate` then `dotnet ef database update`.

## Run application

Run application with command line `dotnet run`.

## Run tests

Run tests with command line `dotnet test`.