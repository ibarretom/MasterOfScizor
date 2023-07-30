# Introduction

TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project.

# Getting Started

TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:

1. Installation process
2. Software dependencies
3. Latest releases
4. API references

# Build and Test

TODO: Describe and show how to build your code and run the tests.

# Contribute

TODO: Explain how other users and developers can contribute to make your code better.

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:

- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)

## Funcionalidades

### Registrar barbearia

- Nome do estabelecimento
- CNPJ
- Endereço
- Horário de funcionamento
- Logotipo
- Telefone
- Email

### Gerenciar barbearia

- Adicionar funcionário (somente owner ou admin)
- Gerenciar horário de funcionamento (owner ou admin)
- Fila/Agendamento (owner, admin)
  - Fila até que o horário estimado seja preenchido ou número de serviços?
- Gerenciar agendamentos
- Relatório de agendamentos

### Registro de funcionários

- Nome
- Email
- Senha
- CPF
- Função
- Foto perfil

### Registrar serviço

- Nome do serviço
- Categoria
- Preço
- Descrição
- Duração

### Registrar cliente

- Email
- Senha
- Telefone
- Nome

### Realizar agendamento

- User
- Serviço

## Security tests level

- Um usuário não pode criar um serviço para uma branch que não é dele.
- Um usuário não pode criar um serviço se não tiver autorização para criar.
-

## Lembretes

### Criação de serviços

[] A branch deve ser pega pelo userId que está vindo no request.
[] Validar se a branch é passada é vazia (Use case ou serviço?)
