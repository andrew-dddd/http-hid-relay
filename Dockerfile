#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y \
    libhidapi-hidraw0 \
    libhidapi-libusb0 \
    libhidapi-dev \
    && rm -rf /var/lib/apt/lists/*

RUN ln -s /usr/lib/$(uname -m)-linux-gnu/libhidapi-hidraw.so.0 /usr/lib/libhidapi.so

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["/src/HttpHidRelay/", "HttpHidRelay/"]
RUN dotnet restore "HttpHidRelay/HttpHidRelay.csproj"
COPY . .
WORKDIR "/src/HttpHidRelay"
RUN dotnet build "HttpHidRelay.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HttpHidRelay.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Container
ENTRYPOINT ["dotnet", "HttpHidRelay.dll"]