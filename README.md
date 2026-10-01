# Cadastro de Produtos — CRUD com ADO.NET (WPF)

Aplicação WPF (.NET 8) para cadastrar e gerenciar produtos, usando **ADO.NET puro**
(`Microsoft.Data.SqlClient`) com **SQL Server / LocalDB**.

## Funcionalidades
- Inserir, listar, buscar por ID, atualizar e excluir produtos
- Comandos SQL **100% parametrizados** (prevenção contra SQL Injection)
- Tratamento de exceções de banco (`SqlException`) com mensagens amigáveis
- Log das operações em arquivo (`logs/operacoes.log`, ao lado do executável)

## Estrutura
```
ProdutosApp/
├── Models/Produto.cs               # entidade
├── Data/ProdutoRepository.cs       # todo o acesso ao banco (ADO.NET)
├── Data/RepositoryException.cs     # exceção da camada de dados
├── Services/LogService.cs          # log em arquivo
├── Database/script.sql             # script de criação do banco/tabela
├── MainWindow.xaml(.cs)            # interface (não contém SQL)
└── appsettings.json                # connection string
```

## Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (Windows, pois é WPF)
- SQL Server LocalDB (vem com o Visual Studio) **ou** SQL Server Express
- SSMS (ou Visual Studio / Azure Data Studio) para rodar o script

## Como configurar e executar
1. **Criar o banco e a tabela:** abra `Database/script.sql` no SSMS, conecte em
   `(localdb)\MSSQLLocalDB` e execute (F5). Isso cria o banco `LojaDB` e a tabela `Produtos`.
2. **Connection string:** confira o `appsettings.json`. O padrão usa LocalDB:
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LojaDB;Trusted_Connection=True;Encrypt=False;"
   ```
   Para SQL Server local use, por exemplo, `Server=localhost;Database=LojaDB;Trusted_Connection=True;Encrypt=False;`
   (ou `.\\SQLEXPRESS`).
3. **Executar:**
   ```bash
   dotnet restore
   dotnet run --project ProdutosApp
   ```
   Ou abra o `.csproj` no Visual Studio e pressione F5.

## Como usar
- **Inserir:** preencha Nome, Preço, Estoque e Categoria (o Id é gerado pelo banco) → *Inserir*.
- **Listar:** mostra todos os produtos na grade. Clicar em uma linha preenche os campos.
- **Buscar por ID:** digite o Id → *Buscar por ID*.
- **Atualizar:** informe o Id, altere os campos → *Atualizar*.
- **Excluir:** informe o Id (ou selecione na grade) → *Excluir* (pede confirmação).
- **Sair:** fecha a aplicação.

## Log
Cada operação (e cada erro) é gravada em `logs/operacoes.log`, por exemplo:
```
2026-09-30 21:14:03 [INFO] INSERIR produto 'Teclado' - 1 linha(s) afetada(s)
```

## Prints
Coloque os prints das operações na pasta `prints/` (inserir, listar, atualizar, etc.).
