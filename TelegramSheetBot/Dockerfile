FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TelegramSheetBot/TelegramSheetBot.csproj", "TelegramSheetBot/"]
RUN dotnet restore "TelegramSheetBot/TelegramSheetBot.csproj"
COPY . .
WORKDIR "/src/TelegramSheetBot"
RUN dotnet build "TelegramSheetBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramSheetBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramSheetBot.dll"]
