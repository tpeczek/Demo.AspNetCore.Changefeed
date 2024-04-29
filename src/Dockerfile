ARG SDK_VERSION=8.0-jammy
ARG ASPNET_VERSION=8.0-jammy-chiseled

FROM mcr.microsoft.com/dotnet/sdk:$SDK_VERSION AS build-env
WORKDIR /src

COPY . .
RUN dotnet restore /src/Demo.AspNetCore.Changefeed \
  --runtime linux-x64

RUN dotnet publish /src/Demo.AspNetCore.Changefeed \
  --no-restore \
  --runtime linux-x64 \
  --configuration Release \
  --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:$ASPNET_VERSION
USER $APP_UID
WORKDIR /app
COPY --from=build-env /app/publish .
EXPOSE 8080
ENTRYPOINT ["./Demo.AspNetCore.Changefeed"]