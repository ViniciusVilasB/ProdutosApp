using System.IO;

namespace ProdutosApp.Services;

/// <summary>
/// Registra em arquivo (logs/operacoes.log) as operações realizadas pela aplicação.
/// </summary>
public class LogService
{
    private static readonly object _lock = new();
    private readonly string _caminhoArquivo;

    public LogService()
    {
        string pasta = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(pasta);
        _caminhoArquivo = Path.Combine(pasta, "operacoes.log");
    }

    public void Info(string mensagem) => Escrever("INFO", mensagem);

    public void Erro(string mensagem) => Escrever("ERRO", mensagem);

    private void Escrever(string nivel, string mensagem)
    {
        string linha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{nivel}] {mensagem}{Environment.NewLine}";
        try
        {
            lock (_lock)
            {
                File.AppendAllText(_caminhoArquivo, linha);
            }
        }
        catch (IOException)
        {
            // Falha ao gravar log não deve derrubar a aplicação.
        }
    }
}
