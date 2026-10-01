using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using ProdutosApp.Data;
using ProdutosApp.Models;
using ProdutosApp.Services;

namespace ProdutosApp;

/// <summary>
/// Interface: apenas lê/valida os campos e chama o ProdutoRepository.
/// Nenhum SQL é escrito aqui.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private readonly ProdutoRepository? _repo;
    private readonly LogService _log = new();

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string? cs = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection não encontrada.");

            _repo = new ProdutoRepository(cs, _log);
            _log.Info("Aplicação iniciada.");
        }
        catch (Exception ex)
        {
            _log.Erro($"Falha ao carregar configuração: {ex.Message}");
            MessageBox.Show($"Erro ao ler o appsettings.json:\n{ex.Message}", "Erro de configuração",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Loaded += (_, _) => CarregarGrade();
    }

    // ------------------------------------------------------------ Menu
    private void BtnInserir_Click(object sender, RoutedEventArgs e)
    {
        if (_repo is null || !TryLerProduto(out Produto produto, exigirId: false)) return;

        try
        {
            _repo.Inserir(produto);
            Status("Produto inserido com sucesso.");
            LimparCampos();
            CarregarGrade();
        }
        catch (RepositoryException ex) { MostrarErro(ex); }
    }

    private void BtnListar_Click(object sender, RoutedEventArgs e) => CarregarGrade();

    private void BtnBuscar_Click(object sender, RoutedEventArgs e)
    {
        if (_repo is null || !TryLerId(out int id)) return;

        try
        {
            Produto? produto = _repo.BuscarPorId(id);
            if (produto is null)
            {
                dgProdutos.ItemsSource = new List<Produto>();
                Status($"Nenhum produto encontrado com Id {id}.");
                MessageBox.Show($"Produto com Id {id} não encontrado.", "Busca",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            dgProdutos.ItemsSource = new List<Produto> { produto };
            PreencherCampos(produto);
            Status($"Produto {id} encontrado. Use 'Listar' para ver todos novamente.");
        }
        catch (RepositoryException ex) { MostrarErro(ex); }
    }

    private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
    {
        if (_repo is null || !TryLerProduto(out Produto produto, exigirId: true)) return;

        try
        {
            if (_repo.Atualizar(produto))
            {
                Status($"Produto {produto.Id} atualizado com sucesso.");
                CarregarGrade();
            }
            else
            {
                MessageBox.Show($"Produto com Id {produto.Id} não encontrado.", "Atualizar",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (RepositoryException ex) { MostrarErro(ex); }
    }

    private void BtnExcluir_Click(object sender, RoutedEventArgs e)
    {
        if (_repo is null || !TryLerId(out int id)) return;

        var confirmacao = MessageBox.Show(
            $"Deseja realmente excluir o produto {id}? Esta ação é irreversível.",
            "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmacao != MessageBoxResult.Yes) return;

        try
        {
            if (_repo.Excluir(id))
            {
                Status($"Produto {id} excluído com sucesso.");
                LimparCampos();
                CarregarGrade();
            }
            else
            {
                MessageBox.Show($"Produto com Id {id} não encontrado.", "Excluir",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (RepositoryException ex) { MostrarErro(ex); }
    }

    private void BtnSair_Click(object sender, RoutedEventArgs e)
    {
        _log.Info("Aplicação encerrada.");
        Close();
    }

    private void BtnLimpar_Click(object sender, RoutedEventArgs e) => LimparCampos();

    private void DgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgProdutos.SelectedItem is Produto p) PreencherCampos(p);
    }

    // ------------------------------------------------------------ Auxiliares
    private void CarregarGrade()
    {
        if (_repo is null) return;

        try
        {
            List<Produto> produtos = _repo.Listar();
            dgProdutos.ItemsSource = produtos;
            Status($"{produtos.Count} produto(s) listado(s).");
        }
        catch (RepositoryException ex) { MostrarErro(ex); }
    }

    private bool TryLerId(out int id)
    {
        if (int.TryParse(txtId.Text.Trim(), out id) && id > 0) return true;

        MessageBox.Show("Informe um Id válido (número inteiro maior que zero).", "Validação",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        txtId.Focus();
        return false;
    }

    private bool TryLerProduto(out Produto produto, bool exigirId)
    {
        produto = new Produto();

        if (exigirId)
        {
            if (!TryLerId(out int id)) return false;
            produto.Id = id;
        }

        string nome = txtNome.Text.Trim();
        string categoria = txtCategoria.Text.Trim();

        if (nome.Length == 0)
            return Invalido("O nome é obrigatório.", txtNome);

        if (!decimal.TryParse(txtPreco.Text.Trim(), NumberStyles.Number, PtBr, out decimal preco) || preco < 0)
            return Invalido("Informe um preço válido (ex.: 19,90) maior ou igual a zero.", txtPreco);

        if (!int.TryParse(txtEstoque.Text.Trim(), out int estoque) || estoque < 0)
            return Invalido("Informe um estoque válido (número inteiro maior ou igual a zero).", txtEstoque);

        if (categoria.Length == 0)
            return Invalido("A categoria é obrigatória.", txtCategoria);

        produto.Nome = nome;
        produto.Preco = preco;
        produto.Estoque = estoque;
        produto.Categoria = categoria;
        return true;
    }

    private static bool Invalido(string mensagem, Control campo)
    {
        MessageBox.Show(mensagem, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
        campo.Focus();
        return false;
    }

    private void PreencherCampos(Produto p)
    {
        txtId.Text = p.Id.ToString();
        txtNome.Text = p.Nome;
        txtPreco.Text = p.Preco.ToString("F2", PtBr);
        txtEstoque.Text = p.Estoque.ToString();
        txtCategoria.Text = p.Categoria;
    }

    private void LimparCampos()
    {
        txtId.Clear();
        txtNome.Clear();
        txtPreco.Clear();
        txtEstoque.Clear();
        txtCategoria.Clear();
        dgProdutos.SelectedItem = null;
    }

    private void Status(string mensagem) => txtStatus.Text = mensagem;

    private void MostrarErro(RepositoryException ex)
    {
        Status("Erro: " + ex.Message);
        MessageBox.Show(ex.Message, "Erro de banco de dados", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
