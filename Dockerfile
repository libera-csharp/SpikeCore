FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY src/SpikeCore/SpikeCore.Web/bin/Release/netcoreapp2.2/publish /app
ENTRYPOINT ["dotnet", "SpikeCore.Web.dll"]