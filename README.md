# app_compras_v2

Segunda versao do projeto [app_compras](https://github.com/yedsrjr/app_compras).

Sistema web para consulta e analise de pedidos de compra, com autenticacao de usuarios, controle administrativo e integracao com uma API externa para busca de dados de compras.

## Evolução do projeto

Este repositorio representa a evolucao da primeira versao do sistema `app_compras`.

Nesta versao, o projeto foi organizado para consolidar funcionalidades como:

- autenticacao de usuarios com senha criptografada
- controle administrativo de usuarios
- consulta de pedidos por diferentes criterios
- dashboard de apoio para analise de compras
- integracao com API externa para busca de dados

## Sobre o projeto

O `app_compras_v2` foi desenvolvido para apoiar a consulta de pedidos de compra e a comparacao de informacoes como menor valor, media e ultima compra de itens.

O sistema possui dois perfis principais:

- `Comprador`
- `Administrador`

Tambem contempla controle de status de usuario:

- `Ativo`
- `Inativo`
- `Bloqueado`

## Funcionalidades

- login de usuarios com senha criptografada em `BCrypt`
- cadastro de novos usuarios
- alteracao e redefinicao de senha
- gerenciamento de usuarios pela area administrativa
- consulta de pedidos de compra por:
  - numero do pedido
  - codigo do item
  - descricao do item
- dashboard com dados de menor valor
- validacao de permissao para alteracoes com base no perfil do usuario

## Tecnologias utilizadas

- `ASP.NET Core MVC`
- `.NET 9`
- `Entity Framework Core`
- `SQLite`
- `HttpClient`
- `BCrypt.Net`
- `Bootstrap`

## Estrutura principal

- [Program.cs](C:\Dev\AppComprasPROD\AppCompras-V1\Program.cs)
  Configuracao da aplicacao, sessao, servicos e rotas.

- [Controllers](C:\Dev\AppComprasPROD\AppCompras-V1\Controllers)
  Controladores de login, compras, administracao e home.

- [Services](C:\Dev\AppComprasPROD\AppCompras-V1\Services)
  Regras de autenticacao, usuarios e integracao com a API externa.

- [Data](C:\Dev\AppComprasPROD\AppCompras-V1\Data)
  Contexto de banco de dados com `AppDbContext`.

- [Models](C:\Dev\AppComprasPROD\AppCompras-V1\Models)
  Entidades, enums, objetos de configuracao e view models.

- [Views](C:\Dev\AppComprasPROD\AppCompras-V1\Views)
  Telas Razor da aplicacao.

- [wwwroot](C:\Dev\AppComprasPROD\AppCompras-V1\wwwroot)
  Arquivos estaticos como CSS, JavaScript, imagens e bibliotecas.

## Banco de dados

O projeto utiliza `SQLite` com o arquivo local `Db.db`, configurado em [AppDbContext.cs](C:\Dev\AppComprasPROD\AppCompras-V1\Data\AppDbContext.cs).

Atualmente a base local e usada para armazenar os usuarios do sistema.

## Integracao externa

As consultas de compras sao feitas por meio de uma API configurada na secao `ApiLiberali`.

Configuracao esperada:

```json
{
  "ApiLiberali": {
    "BaseUrl": "URL_DA_API",
    "AuthToken": "TOKEN_OU_HEADER_DE_AUTORIZACAO"
  }
}
```

Essas configuracoes podem ser adicionadas em `appsettings.json` ou em secrets/variaveis de ambiente, conforme o ambiente utilizado.

## Como executar

1. Restaurar as dependencias:

```bash
dotnet restore
```

2. Configurar a conexao da API externa na secao `ApiLiberali`

3. Aplicar as migrations, se necessario:

```bash
dotnet ef database update
```

4. Executar o projeto:

```bash
dotnet run
```

## Fluxo principal

- o usuario realiza login no sistema
- a aplicacao valida as credenciais e o status do usuario
- o modulo de compras envia a consulta para a API externa
- os dados retornados sao exibidos na tela de consulta e no dashboard
- administradores podem gerenciar usuarios pela area administrativa

## Requisitos representados no sistema

O projeto implementa, de forma geral, requisitos relacionados a:

- cadastro e gerenciamento de usuarios
- autenticacao e login
- consulta e analise de pedidos de compra
- interface web responsiva
- seguranca no armazenamento de senha

## Observacao

Este projeto representa uma aplicacao academica/pratica voltada ao estudo e implementacao de um sistema web para apoio a processos de consulta e analise de compras.
