FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PhoneBookApi/PhoneBookApi.csproj", "PhoneBookApi/"]
RUN dotnet restore "PhoneBookApi/PhoneBookApi.csproj"
COPY . .
WORKDIR "/src/PhoneBookApi"
RUN dotnet publish "PhoneBookApi.csproj" -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "PhoneBookApi.dll"]
