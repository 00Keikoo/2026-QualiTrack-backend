FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY QualiTrack.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish QualiTrack.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y \ 
libgssapi-krb5-2\ 
&& rm -rf /var/lib/apt/lists

COPY --from=build /app/publish .

RUN mkdir -p /app/uploads

EXPOSE 5144
ENTRYPOINT ["dotnet", "QualiTrack.dll"]
