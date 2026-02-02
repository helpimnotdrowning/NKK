FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
ENV ASPNETCORE_HTTP_PORTS=8081
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/NKK
COPY NKK.csproj .
RUN dotnet restore NKK.csproj
COPY . .
RUN dotnet publish -o /app/build

FROM base AS final
WORKDIR /app
COPY --from=build /app/build /app/NKK
WORKDIR /app/NKK
CMD [ "./NKK" ]
