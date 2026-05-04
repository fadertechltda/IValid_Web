using Google.Cloud.Firestore;

namespace DOMAIN.Model.Produto
{
    [FirestoreData]
    public class ProdutoModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("name")]
        public string? Nome { get; set; }

        [FirestoreProperty("quantidade")]
        public int Quantidade { get; set; }

        [FirestoreProperty("oldPrice")]
        public double Preco { get; set; }

        [FirestoreProperty("newPrice")]
        public double PrecoPromocao { get; set; }

        [FirestoreProperty("descricaoPorcentual")]
        public int DescricaoPorcentual { get; set; }

        [FirestoreProperty("daysValidity")]
        public int DiaValidade { get; set; }

        [FirestoreProperty("dateVenc")]
        public DateTime DataVencimento { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }

        [FirestoreProperty("urlImagem")]
        public string? UrlImagem { get; set; }

        public ProdutoModel()
        {

        }

        public ProdutoModel(string nome, int quantidade, double preco, double precoPromocao, int descricaoPorcentual, int diaValidade, DateTime dataVencimento, string status, string urlImagem)
        {
            Nome = nome;
            Quantidade = quantidade;
            Preco = preco;
            PrecoPromocao = precoPromocao;
            DescricaoPorcentual = descricaoPorcentual;
            DiaValidade = diaValidade;
            DataVencimento = dataVencimento;
            Status = status;
            UrlImagem = urlImagem;
        }
    }
}