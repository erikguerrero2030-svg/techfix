FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TechFix/TechFix.csproj TechFix/
RUN dotnet restore TechFix/TechFix.csproj
COPY TechFix/ TechFix/
RUN dotnet publish TechFix/TechFix.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "TechFix.dll"]
