#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["/elefanti.video.backend/elefanti.video.backend.csproj", "."]
RUN dotnet restore "elefanti.video.backend.csproj"
COPY /elefanti.video.backend .
WORKDIR "/src/."
RUN dotnet build "elefanti.video.backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "elefanti.video.backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "elefanti.video.backend.dll"]