# Sử dụng .NET SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file project từ thư mục con và restore dependencies
COPY ASA-TENANT-BE/*.csproj ./ASA-TENANT-BE/
RUN dotnet restore ASA-TENANT-BE/ASA-TENANT-BE.csproj

# Copy toàn bộ source code và build
COPY ASA-TENANT-BE/. ./ASA-TENANT-BE/
WORKDIR /src/ASA-TENANT-BE
RUN dotnet publish -c Release -o /app/publish

# Sử dụng runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy từ build stage
COPY --from=build /app/publish .

# Thiết lập entry point - THAY TÊN FILE CỦA BẠN VÀO ĐÂY
ENTRYPOINT ["dotnet", "ASA-TENANT-BE.dll"]