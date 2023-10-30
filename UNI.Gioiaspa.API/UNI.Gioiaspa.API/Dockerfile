#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UNI.Gioiaspa.API/UNI.Gioiaspa.API.csproj", "UNI.Gioiaspa.API/"]
RUN dotnet restore "UNI.Gioiaspa.API/UNI.Gioiaspa.API.csproj"
COPY . .
WORKDIR "/src/UNI.Gioiaspa.API"
RUN dotnet build "UNI.Gioiaspa.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UNI.Gioiaspa.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UNI.Gioiaspa.API.dll"]