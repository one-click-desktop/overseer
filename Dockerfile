FROM mcr.microsoft.com/dotnet/sdk:5.0

WORKDIR /overseer

#Skopiowanie potrzebnych danych
COPY  Overseer Overseer/
COPY Overseer.sln .
COPY Overseer/config config

#Zbudowanie
RUN dotnet build
RUN dotnet publish -o out

#Uruchomienie aplikacji
EXPOSE 5000
EXPOSE 5001

#Dodac tutaj from runtime i kopia binarek z poprzedniej warstwy!!!
ENV ASPNETCORE_ENVIRONMENT="Production"
ENTRYPOINT ["dotnet", "out/OneClickDesktop.Overseer.dll", "-c", "config"]