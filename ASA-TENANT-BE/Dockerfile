# Sử dụng .NET SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution (bao gồm tất cả projects)
COPY . .

# Restore packages và publish project chính
WORKDIR /src/ASA-TENANT-BE/ASA-TENANT-BE
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Sử dụng runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080

# Thiết lập entry point
ENTRYPOINT ["dotnet", "ASA-TENANT-BE.dll"]