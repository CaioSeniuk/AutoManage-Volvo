# 🚛 AutoManage - Sistema de Gestão de Concessionária Volvo

O **AutoManage** é uma solução robusta de backend desenvolvida em **.NET 8** para gerenciar integralmente as operações de uma concessionária de caminhões Volvo. O sistema cobre desde o inventário de veículos e gestão de clientes até o controle complexo de peças e serviços.

---

## 🚀 Tecnologias Utilizadas

O projeto foi construído utilizando as melhores práticas do ecossistema Microsoft:

*   **Plataforma:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   **Linguagem:** C# 12
*   **ORM:** Entity Framework Core 8 (Code-First)
*   **Banco de Dados:** SQL Server
*   **Testes:** xUnit & Moq (com Banco em Memória)
*   **Documentação API:** Swagger / OpenAPI
*   **Padrões de Projeto:** MVC, Repository Pattern (Simplificado), **Chain of Responsibility**.

---

## 🏛️ Arquitetura e Design Patterns

O projeto segue uma arquitetura em camadas focada em manutenibilidade e escalabilidade:

### 1. Chain of Responsibility (Validações)
Para evitar controladores inchados e lógica condicional complexa (`if/else`), implementamos o padrão **Chain of Responsibility** no cadastro de veículos.
*   **Localização:** `AutoManage/Validation/`
*   **Funcionamento:** A requisição passa por uma corrente de validadores (`ChassiUnicoHandler` -> `ProprietarioExistenteHandler`). Se algum falhar, a execução é interrompida imediatamente (Fail Fast).
*   **Benefício:** Permite adicionar novas regras de negócio (ex: validação de ano de fabricação) sem alterar o código existente do Controller.

### 2. Entity Framework Core (Dados)
Utilizamos Migrations para versionamento do esquema do banco de dados, garantindo que a evolução do código C# seja refletida de forma segura no SQL Server.
*   Relacionamentos configurados via Fluent API (`AutoManageContext.cs`).
*   Uso de `Include` para Eager Loading (evitando queries N+1).

---

## 🛠️ Como Executar o Projeto

### Pré-requisitos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.
*   SQL Server (LocalDB ou Container Docker) ou configurar para usar In-Memory/SQLite para testes rápidos.

### Passos
1.  **Clone o repositório:**
    ```bash
    git clone https://github.com/seu-usuario/projeto-final-volvo.git
    cd projeto-final-volvo
    ```

2.  **Configure o Banco de Dados conforme seu ambiente:**
    O projeto está preparado para rodar tanto em **Windows** (via LocalDB) quanto em **macOS/Linux** (via Docker).

    *   **No Windows:**
        - Verifique se o `LocalDB` está instalado.
        - No arquivo `Program.cs`, certifique-se que a variável `connectionString` use `"DefaultConnection"`.
    *   **No macOS (Docker):**
        - Suba um container SQL Server (ex: `docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`).
        - No arquivo `Program.cs`, a variável `connectionString` deve usar `"DockerConnection"`.
        - Verifique a senha no `appsettings.json`.

3.  **Aplique as Migrations (Cria o Banco):**
    ```bash
    dotnet ef database update --project AutoManage
    ```

4.  **Execute a Aplicação:**
    ```bash
    dotnet run --project AutoManage
    ```
    A API estará disponível em: `http://localhost:5000` (ou porta configurada).

5.  **Acesse a Documentação (Swagger):**
    Abra o navegador em: `http://localhost:5000/swagger`

---

## ✅ Executando os Testes

O projeto possui uma suíte de testes unitários robusta cobrindo Controllers e Regras de Negócio.

```bash
dotnet test
```

### O que é testado?
*   **VeiculosController:** Valida se a criação de veículos respeita as regras de unicidade de Chassi e existência de Proprietário (testando a Chain of Responsibility).
*   **VendedoresController:** Testes de operações CRUD básicas.

---

## 📦 Estrutura do Projeto

```
/
├── AutoManage/                 # Aplicação Principal (API)
│   ├── Controllers/            # Endpoints da API (V1)
│   ├── Data/                   # Contexto do EF Core
│   ├── Migrations/             # Histórico de mudanças do Banco
│   ├── Models/                 # Entidades de Domínio (Veiculo, Peca, etc.)
│   │   └── Peca/               # Subdomínio de Peças Volvo
│   └── Validation/             # Regras de Negócio (Chain of Responsibility)
│
├── AutoManage.Tests/           # Projeto de Testes Unitários (xUnit)
└── README.md                   # Documentação do Projeto
```

---

## 🔌 API Endpoints (Principais)

### 🚛 Veículos (`/api/v1/Veiculos`)
*   `GET /`: Lista veículos (com paginação `?page=1&limit=10` e filtro `?versaoMotor=D13`).
*   `POST /`: Cria um novo veículo (valida Chassi e Dono).
*   `GET /{chassi}`: Detalhes do veículo e proprietário.

### 👥 Proprietários (`/api/v1/Proprietarios`)
*   Gerenciamento de clientes e frotistas.

### ⚙️ Peças (`/api/v1/Pecas`)
*   Gestão de inventário de peças genuínas.

---

## 📝 Status do Projeto
*   [x] CRUD de Veículos, Proprietários e Vendedores.
*   [x] Sistema de Vendas com integridade referencial.
*   [x] Módulo de Peças e Pedidos (Master-Detail).
*   [x] Implementação de Design Patterns (Chain of Responsibility).
*   [x] Testes Unitários.
*   [x] Documentação Swagger.

---
Desenvolvido como Projeto Final de Curso .NET.
