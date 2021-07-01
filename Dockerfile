FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

RUN mkdir /app/DB

COPY src/SpikeCore/SpikeCore.Web/bin/Release/net5.0/publish /app
COPY src/SpikeCore/SpikeCore.Web/DB/SpikeCore.db /app/DB

# Step down from UID 0
RUN groupadd -g 1001 app && \
    useradd -u 1001 -g app app && \
    chown -R app:app /app

USER app

ENTRYPOINT ["dotnet", "SpikeCore.Web.dll"]