FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY MediTrack.Mvc/MediTrack.Mvc.csproj MediTrack.Mvc/
RUN dotnet restore MediTrack.Mvc/MediTrack.Mvc.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish MediTrack.Mvc/MediTrack.Mvc.csproj -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MediTrack.Mvc.dll"]
