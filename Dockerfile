# syntax=docker/dockerfile:1-labs
FROM badgerati/pode:latest
SHELL ["pwsh", "-c"]

RUN apt-get update
RUN apt-get install git -y

RUN git clone https://github.com/helpimnotdrowning/Mizumiya --branch v0.2.0 /tmp/Mizumiya_repo
RUN mkdir -p /usr/local/share/powershell/Modules/Mizumiya
RUN cp -r /tmp/Mizumiya_repo/Mizumiya/* /usr/local/share/powershell/Modules/Mizumiya

RUN Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
RUN Install-Module -Name PSParseHTML -RequiredVersion 2.0.2

COPY . /app/NKK/

EXPOSE 8081
WORKDIR /app/NKK
CMD [ "pwsh", "-c", "./Server.ps1" ]
