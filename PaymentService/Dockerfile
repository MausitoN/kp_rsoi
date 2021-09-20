#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Payment/PaymentService.csproj", "Payment/"]
COPY ["ShareService/ShareService.csproj", "ShareService/"]
RUN dotnet restore "Payment/PaymentService.csproj"
COPY . .
WORKDIR "/src/Payment"
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "PaymentService.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet PaymentService.dll