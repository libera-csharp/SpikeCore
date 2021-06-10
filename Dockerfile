FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

RUN mkdir /app/DB

COPY src/SpikeCore/SpikeCore.Web/bin/Release/net5.0/publish /app
COPY src/SpikeCore/SpikeCore.Web/DB/SpikeCore.db /app/DB
ENTRYPOINT ["dotnet", "SpikeCore.Web.dll"]