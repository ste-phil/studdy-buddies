# syntax=docker/dockerfile:1.7
ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY StudyBuddies.slnx ./
COPY StudyBuddies.Core/StudyBuddies.Core.csproj StudyBuddies.Core/
COPY StudyBuddies.Web/StudyBuddies.Web.csproj StudyBuddies.Web/
RUN dotnet restore StudyBuddies.slnx

COPY StudyBuddies.Core/ StudyBuddies.Core/
COPY StudyBuddies.Web/ StudyBuddies.Web/
RUN dotnet publish StudyBuddies.Web/StudyBuddies.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
USER root
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__DefaultConnection="DataSource=/data/app.db;Cache=Shared"

EXPOSE 8080
VOLUME ["/data"]

ENTRYPOINT ["dotnet", "StudyBuddies.Web.dll"]
