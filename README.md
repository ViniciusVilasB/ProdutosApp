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

# Prints de funcionamento
 
Evidências das operações do CRUD de produtos executadas na aplicação WPF
(ADO.NET + SQL Server LocalDB, banco `LojaDB`, tabela `Produtos`).
 
| # | Operação | Arquivo |
|---|----------|---------|
| 1 | Inserir (INSERT) | `Print 1.png` |
| 2 | Buscar por ID (SELECT ... WHERE Id = @Id) | `Print 2.png` |
| 3 | Atualizar (UPDATE) | `Print 3.png` |
| 4 | Excluir (DELETE) | `Print 4.png` |
 
---
 
## Print 1 — Inserir produto
Foram cadastrados três produtos pelo menu **1. Inserir**: Teclado Mecânico,
Mouse Gamer e Monitor 24. A grade exibe os três registros e a barra de status
confirma a inserção. O `Id` é gerado automaticamente pelo banco (`IDENTITY`).
 
![Print 1 - Inserir](Print%201.png)
 
## Print 2 — Buscar produto por ID
Busca do produto de **Id 2** pelo menu **3. Buscar por ID**. A grade passa a
mostrar somente o registro encontrado e os campos do formulário são preenchidos
com seus dados. A consulta usa `ExecuteReader` com o parâmetro `@Id`.
 
![Print 2 - Buscar por ID](Print%202.png)
 
## Print 3 — Atualizar produto
Atualização do produto de **Id 2** pelo menu **4. Atualizar**, alterando preço
e estoque. A grade mostra os valores novos após a operação, executada com
`ExecuteNonQuery` e comando `UPDATE` parametrizado (com `WHERE Id = @Id`).
 
![Print 3 - Atualizar](Print%203.png)
 
## Print 4 — Excluir produto
Exclusão do produto de **Id 3** pelo menu **5. Excluir**, após confirmação do
usuário. A grade deixa de exibir o registro removido. A operação usa
`ExecuteNonQuery` com `DELETE ... WHERE Id = @Id`.
 
![Print 4 - Excluir](Print%204.png)
 
---
 
Todas as operações (e eventuais erros) também são registradas em
`logs/operacoes.log`.
