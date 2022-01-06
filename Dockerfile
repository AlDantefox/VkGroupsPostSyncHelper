FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["VkGroupsPostSyncHelper/VkGroupsPostSyncHelper.csproj", "VkGroupsPostSyncHelper/"]
COPY ["VkGroupsPostSyncHelper.DAL/VkGroupsPostSyncHelper.DAL.csproj", "VkGroupsPostSyncHelper.DAL/"]
RUN dotnet restore "VkGroupsPostSyncHelper/VkGroupsPostSyncHelper.csproj"
COPY . .
WORKDIR "/src/VkGroupsPostSyncHelper"
RUN dotnet build "VkGroupsPostSyncHelper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VkGroupsPostSyncHelper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VkGroupsPostSyncHelper.dll"]