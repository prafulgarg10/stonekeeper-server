FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY . . 

EXPOSE 3001

CMD ["dotnet", "run", "--urls", "http://+:3001"]