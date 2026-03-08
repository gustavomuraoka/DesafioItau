# CompraAutomatizada

Sistema automatizado de execução de ordens de compra de ativos financeiros, com distribuição por cliente, cálculo de resíduos e publicação de eventos de IR via Kafka.

---

## Visão Geral

A aplicação orquestra o fluxo completo de compra automatizada:

1. Consulta a cesta de ativos recomendados (Top 5)
2. Calcula ordens de compra consolidadas
3. Distribui proporcionalmente entre clientes
4. Publica eventos de Imposto de Renda no Kafka
5. Expõe os resultados via painel web (Next.js)

---

## Arquitetura

O projeto segue os princípios de **Clean Architecture** combinados com **Domain-Driven Design (DDD)**, separando responsabilidades em camadas bem definidas:

### Camadas

| Camada | Responsabilidade |
|---|---|
| **Domain** | Regras de negócio, Aggregates, interfaces de repositório |
| **Application** | Orquestração via CQRS (Commands/Handlers), DTOs |
| **Infrastructure** | EF Core + MySQL, Kafka producer, implementações de repositório |
| **API** | Controllers REST, injeção de dependência, pipeline HTTP |

---

## Decisões Técnicas

### CQRS com MediatR
Commands e Handlers isolam cada caso de uso, facilitando testes unitários e evolução independente de funcionalidades.

### Domain-Driven Design
Aggregates como `Cliente`, `OrdemDeCompra`, `CestaTopFive` e `EventoIR` encapsulam regras de negócio e invariantes, evitando lógica vazando para a camada de aplicação.

### Entity Framework Core + MySQL
ORM com mapeamento via `IEntityTypeConfiguration`, mantendo o domínio livre de anotações de persistência. Migrations versionadas para rastreabilidade do schema.

### Kafka para Eventos de IR
Publicação assíncrona de eventos de Imposto de Renda desacopla o processamento fiscal do fluxo principal de compra, permitindo consumidores independentes.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 8, C#, ASP.NET Core |
| ORM | Entity Framework Core 8 |
| Banco de dados | MySQL 8.0 |
| Mensageria | Apache Kafka (Confluent) |
| Frontend | Next.js 14, TypeScript |
| Testes | xUnit, Moq, FluentAssertions |
| Infraestrutura | Docker, Docker Compose |

---

## Executando o Projeto

### Pré-requisitos
- Docker e Docker Compose instalados

### Subindo o ambiente completo

```bash
docker compose up --build -d
```

## Testes
# Roda os testes com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Gera relatório HTML
reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

