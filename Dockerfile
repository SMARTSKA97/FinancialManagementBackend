# --- build stage (.NET 10) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
# Framework-dependent publish (small image, fast boots)
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# --- runtime stage (.NET 10 ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# tiny entrypoint binds to the PORT Render provides
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh
ENTRYPOINT ["./entrypoint.sh"]
