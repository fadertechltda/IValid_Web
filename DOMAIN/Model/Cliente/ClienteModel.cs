using Google.Cloud.Firestore;

namespace DOMAIN.Model.Cliente
{
    [FirestoreData]
    public class ClienteModel
    {
        public const string PerfilCliente = "Cliente";

        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("fullName")]
        public string? NomeCompleto { get; set; }

        [FirestoreProperty("email")]
        public string? Email { get; set; }

        [FirestoreProperty("phone")]
        public string? Telefone { get; set; }

        [FirestoreProperty("perfil")]
        public string? Perfil { get; set; }

        [FirestoreProperty("supermercadoId")]
        public string? SupermercadoId { get; set; }

        [FirestoreProperty("address")]
        public EnderecoClienteModel? Endereco { get; set; }

        [FirestoreProperty("totalDonations")]
        public int TotalDoacoes { get; set; }

        [FirestoreProperty("availableCashback")]
        public double CashbackDisponivel { get; set; }

        [FirestoreProperty("favoritos")]
        public List<string> Favoritos { get; set; } = [];

        [FirestoreProperty("fcmTokens")]
        public List<string> TokensDispositivo { get; set; } = [];

        [FirestoreProperty("impact")]
        public ImpactoModel? Impacto { get; set; }

        public bool EhFuncionarioDoPainel => !string.IsNullOrEmpty(SupermercadoId);
    }

    [FirestoreData]
    public class EnderecoClienteModel
    {
        [FirestoreProperty("cep")]
        public string? Cep { get; set; }

        [FirestoreProperty("street")]
        public string? Logradouro { get; set; }

        [FirestoreProperty("number")]
        public string? Numero { get; set; }

        [FirestoreProperty("complement")]
        public string? Complemento { get; set; }

        [FirestoreProperty("neighborhood")]
        public string? Bairro { get; set; }

        [FirestoreProperty("city")]
        public string? Cidade { get; set; }

        [FirestoreProperty("state")]
        public string? Estado { get; set; }

        public string Formatado()
        {
            var partes = new[]
            {
                string.Join(", ", new[] { Logradouro, Numero }.Where(p => !string.IsNullOrWhiteSpace(p))),
                Complemento,
                Bairro,
                string.Join("/", new[] { Cidade, Estado }.Where(p => !string.IsNullOrWhiteSpace(p))),
                Cep
            };

            return string.Join(" - ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
    }
}
