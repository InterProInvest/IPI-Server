#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["HES.Web/HES.Web.csproj", "HES.Web/"]
COPY ["HES.Infrastructure/HES.Infrastructure.csproj", "HES.Infrastructure/"]
COPY ["HES.Core/HES.Core.csproj", "HES.Core/"]
RUN dotnet restore "HES.Web/HES.Web.csproj"
COPY . .
WORKDIR "/src/HES.Web"
RUN dotnet build "HES.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HES.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HES.Web.dll"]