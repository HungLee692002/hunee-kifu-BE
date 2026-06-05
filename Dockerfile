FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EnglishCenter.Backend.sln .
COPY src/EnglishCenter.Domain/EnglishCenter.Domain.csproj src/EnglishCenter.Domain/
COPY src/EnglishCenter.Application/EnglishCenter.Application.csproj src/EnglishCenter.Application/
COPY src/EnglishCenter.Infrastructure/EnglishCenter.Infrastructure.csproj src/EnglishCenter.Infrastructure/
COPY src/EnglishCenter.Api/EnglishCenter.Api.csproj src/EnglishCenter.Api/

RUN dotnet restore EnglishCenter.Backend.sln

COPY src/ src/

RUN dotnet publish src/EnglishCenter.Api/EnglishCenter.Api.csproj -c Release --no-restore -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnglishCenter.Api.dll"]
