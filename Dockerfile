# Estágio 1: Build da Aplicação
# Usamos a imagem completa do SDK para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos de projeto e restaura as dependências primeiro
# Isso aproveita o cache do Docker. Se os pacotes não mudarem, ele não baixa de novo.
COPY *.csproj .
RUN dotnet restore

# Copia o resto do código fonte e publica o projeto
COPY . .
RUN dotnet publish -c Release -o out

# Estágio 2: Imagem Final de Produção
# Usamos a imagem leve do ASP.NET Runtime, que é menor e mais segura
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia apenas os arquivos publicados do estágio de build
COPY --from=build /app/out .

# Expõe a porta 80 do contêiner para o mundo exterior (a ser mapeada pelo Docker Compose)
EXPOSE 80

# Define o comando para iniciar a aplicação quando o contêiner rodar
ENTRYPOINT ["dotnet", "RelatoriosTI.API.dll"]