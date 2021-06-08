#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["App/ScratchPad/ScratchPad.csproj", "App/ScratchPad/"]
COPY ["Gum.IR0Evaluator/Gum.IR0Evaluator.csproj", "Gum.IR0Evaluator/"]
COPY ["Gum.Infra/Gum.Infra.csproj", "Gum.Infra/"]
COPY ["Gum.IR0/Gum.IR0.csproj", "Gum.IR0/"]
COPY ["Gum.IR0Translator/Gum.IR0Translator.csproj", "Gum.IR0Translator/"]
COPY ["Gum.Syntax/Gum.Syntax.csproj", "Gum.Syntax/"]
COPY ["Gum.ModuleInfo/Gum.ModuleInfo.csproj", "Gum.ModuleInfo/"]
COPY ["Gum.TextAnalysis/Gum.TextAnalysis.csproj", "Gum.TextAnalysis/"]
RUN dotnet restore "App/ScratchPad/ScratchPad.csproj"
COPY . .
WORKDIR "/src/App/ScratchPad"
RUN dotnet build "ScratchPad.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScratchPad.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScratchPad.dll"]