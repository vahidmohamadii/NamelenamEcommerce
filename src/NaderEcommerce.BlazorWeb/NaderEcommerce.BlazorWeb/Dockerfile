FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NaderEcommerce.sln", "./"]
COPY ["src/NaderEcommerce.Domain/NaderEcommerce.Domain.csproj", "src/NaderEcommerce.Domain/"]
COPY ["src/NaderEcommerce.Application/NaderEcommerce.Application.csproj", "src/NaderEcommerce.Application/"]
COPY ["src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb.Client/NaderEcommerce.BlazorWeb.Client.csproj", "src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb.Client/"]
COPY ["src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb.csproj", "src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb/"]
RUN dotnet restore "src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb.csproj"

COPY . .
RUN dotnet publish "src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NaderEcommerce.BlazorWeb.dll"]
