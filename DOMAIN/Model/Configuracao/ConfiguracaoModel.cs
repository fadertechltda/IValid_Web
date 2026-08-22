using Google.Cloud.Firestore;

namespace DOMAIN.Model.Configuracao
{
    [FirestoreData]
    public class ConfiguracaoModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("diasAlertaAmarelo")]
        public int DiasAlertaAmarelo { get; set; } = 20;

        [FirestoreProperty("diasAlertaVermelho")]
        public int DiasAlertaVermelho { get; set; } = 10;

        [FirestoreProperty("percentualDescontoAmarelo")]
        public int PercentualDescontoAmarelo { get; set; } = 20;

        [FirestoreProperty("percentualDescontoVermelho")]
        public int PercentualDescontoVermelho { get; set; } = 40;
    }
}
