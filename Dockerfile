FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app

RUN mkdir /app/DB

COPY src/SpikeCore/SpikeCore.Web/bin/Release/netcoreapp2.2/publish /app
COPY src/SpikeCore/SpikeCore.Web/DB/SpikeCore.db /app/DB
ENTRYPOINT ["dotnet", "SpikeCore.Web.dll"]