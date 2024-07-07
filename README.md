# Projeto de microserviçoes de um projeto de loja

Serviço de notificações

![technology ASP.NET Core](https://img.shields.io/badge/techonolgy-ASP.NET_core-blue)
![techonolgy CSharp](https://img.shields.io/badge/techonolgy-CSharp-blueviolet)
![technology Docker](https://img.shields.io/badge/techonolgy-Docker-blue)
![technology MongoDB](https://img.shields.io/badge/techonolgy-MongoDB-brightgreen)
![technology RabbitMQ](https://img.shields.io/badge/techonolgy-RabbitMQ-orange)

## Projetos que compoe a arquitetura

 * [Orders](https://github.com/juniorjrjl/AwesomeShop.Services.Orders);
 * [Customers](https://github.com/juniorjrjl/AwesomeShop.Services.Customers);
 * [Payments](https://github.com/juniorjrjl/AwesomeShop.Services.Payments);
 * [Products](https://github.com/juniorjrjl/AwesomeShop.Services.Products);
 * [API Gateway](https://github.com/juniorjrjl/AwesomeShop.Services.APIGateway);

## Getting Started

## Pré-requisitos

- Docker
- .NET SDK
- RabbitMQ

## Executando a aplicação

crie a network do container usando o seguinte comando:

```
    docker network create awesome-shop-services-net
```


inicie a aplicação

```
    docker-compose up
```

## Configuração de debug via docker

dentro da pasta .vscode/lauch.json coloque a seguinte configuração

```
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Docker Attach",
            "type": "coreclr",
            "request": "attach",
            "processId": "${command:pickRemoteProcess}",
            "pipeTransport": {
                "pipeCwd": "${workspaceRoot}",
                "pipeProgram": "docker",
                "pipeArgs": [ "exec", "-i", "awesomeshopservicesnotifications-notificationapi-1" ],
                "debuggerPath": "/root/vsdbg/vsdbg",
                "quoteArgs": false
            },
            "sourceFileMap": {
                "/DevFreela": "${workspaceRoot}"
            }
        }
    ]
}

```

depois que a aplicação subir é necessario selecionar o processo correto para vincular seu debug ao container, como é mostrado na imagem a seguir
