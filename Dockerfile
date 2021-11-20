FROM mcr.microsoft.com/dotnet/sdk:5.0

WORKDIR /overseer

#Skopiowanie potrzebnych danych
COPY  Backend.Classes Backend.Classes/
COPY  Backend.Classes.Tests Backend.Classes.Tests/
COPY  Overseer Overseer/
COPY Overseer.sln .
RUN ls -l

#Zbudowanie
RUN dotnet build
RUN dotnet publish -o out

#Uruchomienie aplikacji
EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["dotnet", "out/OneClickDesktop.Overseer.dll"]