# Sử dụng .NET SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file project và restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy toàn bộ source code và build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Sử dụng runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy từ build stage
COPY --from=build /app/publish .

# Thiết lập entry point - THAY TÊN FILE CỦA BẠN VÀO ĐÂY
ENTRYPOINT ["dotnet", "ASA-TENANT-BE.dll"]