#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt update && apt -y upgrade
RUN apt install -y wget
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb
RUN apt install -y \
                fontconfig \
                libfreetype6 \
                libjpeg62-turbo \
                libpng16-16 \
                libx11-6 \
                libxcb1 \
                libxext6 \
                libxrender1 \
                xfonts-75dpi \
                xfonts-base
 
RUN dpkg -i wkhtmltox_0.12.6-1.buster_amd64.deb
WORKDIR /api
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /api/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /api/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /api
COPY --from=publish /api/publish .
ENTRYPOINT ["dotnet", "Api.dll"]