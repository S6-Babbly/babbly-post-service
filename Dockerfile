FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["babbly-post-service/babbly-post-service.csproj", "babbly-post-service/"]
RUN dotnet restore "babbly-post-service/babbly-post-service.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/babbly-post-service"

# Build the application
RUN dotnet build "babbly-post-service.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "babbly-post-service.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "babbly-post-service.dll"] 