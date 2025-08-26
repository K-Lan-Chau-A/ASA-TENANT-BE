FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

# Restore từ solution file (cách an toàn hơn)
RUN dotnet restore ASA-TENANT-BE.sln

# Publish bằng cách chỉ định rõ đường dẫn csproj
RUN dotnet publish ASA-TENANT-BE/ASA-TENANT-BE/ASA-TENANT-BE.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ASA-TENANT-BE.dll"]