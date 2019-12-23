FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app

RUN mkdir /app/DB

COPY src/SpikeCore/SpikeCore.Web/bin/Release/netcoreapp3.1/publish /app
COPY src/SpikeCore/SpikeCore.Web/DB/SpikeCore.db /app/DB
ENTRYPOINT ["dotnet", "SpikeCore.Web.dll"]