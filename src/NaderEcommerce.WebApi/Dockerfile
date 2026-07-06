FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NaderEcommerce.sln", "./"]
COPY ["src/NaderEcommerce.Domain/NaderEcommerce.Domain.csproj", "src/NaderEcommerce.Domain/"]
COPY ["src/NaderEcommerce.Application/NaderEcommerce.Application.csproj", "src/NaderEcommerce.Application/"]
COPY ["src/NaderEcommerce.Infrastructure/NaderEcommerce.Infrastructure.csproj", "src/NaderEcommerce.Infrastructure/"]
COPY ["src/NaderEcommerce.WebApi/NaderEcommerce.WebApi.csproj", "src/NaderEcommerce.WebApi/"]
RUN dotnet restore "src/NaderEcommerce.WebApi/NaderEcommerce.WebApi.csproj"

COPY . .
RUN dotnet publish "src/NaderEcommerce.WebApi/NaderEcommerce.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NaderEcommerce.WebApi.dll"]
