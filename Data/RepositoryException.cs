namespace ProdutosApp.Data;

/// <summary>
/// Exceção da camada de dados com mensagem amigável para a interface.
/// </summary>
public class RepositoryException : Exception
{
    public RepositoryException(string mensagem, Exception? inner = null)
        : base(mensagem, inner) { }
}
