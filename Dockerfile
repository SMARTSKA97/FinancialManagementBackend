# --- build stage (.NET 10) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
# publish ONLY the API project
RUN dotnet restore ./src/Presentation/FinancialPlanner.API/FinancialPlanner.API.csproj
RUN dotnet publish ./src/Presentation/FinancialPlanner.API/FinancialPlanner.API.csproj -c Release -o /app /p:UseAppHost=false

# --- runtime stage (.NET 10 ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Render provides $PORT; bind Kestrel to it via a tiny entrypoint
CMD ["sh","-c","ASPNETCORE_URLS=http://0.0.0.0:${PORT} dotnet FinancialPlanner.API.dll"]
