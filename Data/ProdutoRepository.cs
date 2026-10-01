using System.Data;
using Microsoft.Data.SqlClient;
using ProdutosApp.Models;
using ProdutosApp.Services;

namespace ProdutosApp.Data;

/// <summary>
/// Responsável por TODO o acesso ao banco (ADO.NET puro, comandos parametrizados).
/// A interface (MainWindow) nunca escreve SQL.
/// </summary>
public class ProdutoRepository
{
    private readonly string _connectionString;
    private readonly LogService _log;

    public ProdutoRepository(string connectionString, LogService log)
    {
        _connectionString = connectionString;
        _log = log;
    }

    // ---------------------------------------------------------------- INSERT
    public int Inserir(Produto produto)
    {
        const string sql =
            "INSERT INTO Produtos (Nome, Preco, Estoque, Categoria) " +
            "VALUES (@Nome, @Preco, @Estoque, @Categoria)";

        return Executar("INSERIR", conn =>
        {
            using var cmd = new SqlCommand(sql, conn);
            AdicionarParametros(cmd, produto);
            int linhas = cmd.ExecuteNonQuery();
            _log.Info($"INSERIR produto '{produto.Nome}' - {linhas} linha(s) afetada(s)");
            return linhas;
        });
    }

    // ---------------------------------------------------------------- SELECT (todos)
    public List<Produto> Listar()
    {
        const string sql =
            "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos ORDER BY Id";

        return Executar("LISTAR", conn =>
        {
            var lista = new List<Produto>();
            using var cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearProduto(reader));
            }
            _log.Info($"LISTAR produtos - {lista.Count} registro(s) retornado(s)");
            return lista;
        });
    }

    // ---------------------------------------------------------------- SELECT (por Id)
    public Produto? BuscarPorId(int id)
    {
        const string sql =
            "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos WHERE Id = @Id";

        return Executar("BUSCAR", conn =>
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            using SqlDataReader reader = cmd.ExecuteReader();

            Produto? produto = reader.Read() ? MapearProduto(reader) : null;
            _log.Info(produto is null
                ? $"BUSCAR produto Id={id} - não encontrado"
                : $"BUSCAR produto Id={id} - encontrado");
            return produto;
        });
    }

    // ---------------------------------------------------------------- UPDATE
    public bool Atualizar(Produto produto)
    {
        const string sql =
            "UPDATE Produtos SET Nome = @Nome, Preco = @Preco, Estoque = @Estoque, " +
            "Categoria = @Categoria WHERE Id = @Id";

        return Executar("ATUALIZAR", conn =>
        {
            using var cmd = new SqlCommand(sql, conn);
            AdicionarParametros(cmd, produto);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = produto.Id;
            int linhas = cmd.ExecuteNonQuery();
            _log.Info($"ATUALIZAR produto Id={produto.Id} - {linhas} linha(s) afetada(s)");
            return linhas > 0;
        });
    }

    // ---------------------------------------------------------------- DELETE
    public bool Excluir(int id)
    {
        const string sql = "DELETE FROM Produtos WHERE Id = @Id";

        return Executar("EXCLUIR", conn =>
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            int linhas = cmd.ExecuteNonQuery();
            _log.Info($"EXCLUIR produto Id={id} - {linhas} linha(s) afetada(s)");
            return linhas > 0;
        });
    }

    // ================================================================ Auxiliares

    /// <summary>
    /// Abre a conexão, executa a ação e traduz erros do SQL Server em
    /// RepositoryException (com log). O 'using' garante o fechamento da conexão.
    /// </summary>
    private T Executar<T>(string operacao, Func<SqlConnection, T> acao)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            return acao(conn);
        }
        catch (SqlException ex)
        {
            string mensagem = TraduzirErro(ex);
            _log.Erro($"{operacao} falhou (SqlException #{ex.Number}): {ex.Message}");
            throw new RepositoryException(mensagem, ex);
        }
        catch (InvalidOperationException ex)
        {
            // Ex.: connection string inválida/vazia.
            _log.Erro($"{operacao} falhou (InvalidOperationException): {ex.Message}");
            throw new RepositoryException("Configuração de conexão inválida. Verifique o appsettings.json.", ex);
        }
    }

    private static void AdicionarParametros(SqlCommand cmd, Produto p)
    {
        cmd.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = p.Nome;

        var preco = cmd.Parameters.Add("@Preco", SqlDbType.Decimal);
        preco.Precision = 10;
        preco.Scale = 2;
        preco.Value = p.Preco;

        cmd.Parameters.Add("@Estoque", SqlDbType.Int).Value = p.Estoque;
        cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 50).Value = p.Categoria;
    }

    /// <summary>Mapeamento manual DataReader -> Produto.</summary>
    private static Produto MapearProduto(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Nome = reader.GetString(reader.GetOrdinal("Nome")),
        Preco = reader.GetDecimal(reader.GetOrdinal("Preco")),
        Estoque = reader.GetInt32(reader.GetOrdinal("Estoque")),
        Categoria = reader.GetString(reader.GetOrdinal("Categoria"))
    };

    private static string TraduzirErro(SqlException ex) => ex.Number switch
    {
        -1 or 2 or 53 or 4060 or 18456 =>
            "Não foi possível conectar ao banco de dados. Verifique se o SQL Server/LocalDB está ativo, " +
            "se o script .sql foi executado e se a connection string está correta.",
        208 => "A tabela Produtos não existe. Execute o script Database/script.sql.",
        547 => "Valor inválido: preço e estoque não podem ser negativos.",
        8152 => "Algum campo excede o tamanho máximo permitido (Nome: 100, Categoria: 50).",
        _ => $"Erro no banco de dados: {ex.Message}"
    };
}
