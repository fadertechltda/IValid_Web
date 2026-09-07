using Google.Cloud.Firestore;

namespace DOMAIN.Model.Configuracao
{
    [FirestoreData]
    public class ConfiguracaoModel
    {
        public const int DiasLimiteVermelho = 10;
        public const int DiasLimiteAmarelo = 20;

        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("supermercadoId")]
        public string? SupermercadoId { get; set; }

        [FirestoreProperty("percentualDescontoAmarelo")]
        public int PercentualDescontoAmarelo { get; set; } = 20;

        [FirestoreProperty("percentualDescontoVermelho")]
        public int PercentualDescontoVermelho { get; set; } = 40;
    }
}
