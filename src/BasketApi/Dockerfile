FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["BasketApi.csproj", "./"]
RUN dotnet restore "BasketApi.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "BasketApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasketApi.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasketApi.dll"]
