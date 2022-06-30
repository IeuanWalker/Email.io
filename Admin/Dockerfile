#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /admin
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Admin/Admin.csproj", "Admin/"]
COPY ["Database/Database.csproj", "Database/"]
RUN dotnet restore "Admin/Admin.csproj"
COPY . .
WORKDIR "/src/Admin"
RUN dotnet build "Admin.csproj" -c Release -o /admin/build

FROM build AS publish
RUN dotnet publish "Admin.csproj" -c Release -o /admin/publish

FROM base AS final
WORKDIR /admin
COPY --from=publish /admin/publish .

ENTRYPOINT ["dotnet", "Admin.dll"]