FROM mcr.microsoft.com/dotnet/sdk:5.0

WORKDIR /overseer

#Skopiowanie potrzebnych danych
COPY Overseer Overseer/
COPY Overseer.sln .
COPY Overseer/config config

#Zbudowanie
RUN dotnet build
RUN dotnet publish -c Release -o out

#Uruchomienie aplikacji
EXPOSE 5000
EXPOSE 5001

#Dodac tutaj from runtime i kopia binarek z poprzedniej warstwy!!!
ENV ASPNETCORE_ENVIRONMENT="Production"
COPY ["assets/entry_point.sh", "entry_point.sh"]
RUN curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh -o /wait-for-it.sh && chmod +x /wait-for-it.sh

CMD ["/bin/bash", "entry_point.sh"]