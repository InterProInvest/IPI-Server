# Use base image Alpine Linux + Git
FROM alpine/git as base
WORKDIR /app
# Cloning Project
RUN git clone https://github.com/HideezGroup/HES /app/HES
RUN ls /app
# Use dotnet code sdk 2.2 image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2-bionic as build
WORKDIR /app
COPY --from=base /app .
WORKDIR /app/HES/HES.Web/
# Create directory and compiling application Hes
RUN mkdir -p /opt/HES/localhost && dotnet publish -c release -v d -o "/opt/HES/localhost" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
# Copy all config file
RUN cp Crypto_linux.dll /opt/HES/localhost/Crypto.dll
COPY appsettings.json /opt/HES/localhost/

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-bionic as final
WORKDIR /opt/HES/localhost
COPY --from=build /opt/HES/localhost .
# Lissening port
EXPOSE 5000-5001
CMD ./HES.Web
