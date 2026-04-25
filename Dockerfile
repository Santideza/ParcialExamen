FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ParcialExamen.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish ParcialExamen.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 10000
ENTRYPOINT ["dotnet", "ParcialExamen.dll"]
