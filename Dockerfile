FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Ms1.Api/Ms1.Api.csproj src/Ms1.Api/
RUN dotnet restore src/Ms1.Api/Ms1.Api.csproj
COPY src/ src/
RUN dotnet publish src/Ms1.Api/Ms1.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Ms1.Api.dll"]
