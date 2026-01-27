# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /App

# copy everything
COPY . ./

# database SQLite
RUN dotnet tool install --global dotnet-ef --version 7.0
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations add InitialCreate && dotnet ef database update

# copy csproj and restore as distinct layers
RUN (Get-Content MvcIngredient.csproj) | ForEach-Object { $_ -replace "</PackageReference>', '`t`t</PackageReference>`n`t`t<None Include='MvcIngredientContext-bf3483dd-9ed2-4ef1-a176-c35b4a988d79.db' CopyToOutputDirectory='PreserveNewest'/>" } | Set-Content MvcIngredient.csproj
RUN dotnet restore

# copy everything else and build app
RUN dotnet publish -c Release -o /out --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "MvcIngredient.dll"]