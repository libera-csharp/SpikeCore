FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY src/SpikeCore/SpikeCore.Web/bin/Release/netcoreapp2.1/publish /app
ENTRYPOINT ["dotnet", "SpikeCore.Web.dll"]