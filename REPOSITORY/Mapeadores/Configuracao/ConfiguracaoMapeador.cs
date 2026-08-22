using DOMAIN.Model.Configuracao;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Configuracao
{
    public class ConfiguracaoMapeador(FirestoreDb firestoreDb) : IConfiguracaoMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        private const string NomeColecao = "configuracoes";

        public async Task<ConfiguracaoModel?> ObterAsync(string supermercadoId)
        {
            if (string.IsNullOrEmpty(supermercadoId))
                return null;

            DocumentReference docRef = _firestoreDb.Collection(NomeColecao).Document(supermercadoId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            ConfiguracaoModel configuracao = snapshot.ConvertTo<ConfiguracaoModel>();
            configuracao.Id = snapshot.Id;
            return configuracao;
        }

        public async Task SalvarAsync(ConfiguracaoModel configuracao)
        {
            if (string.IsNullOrEmpty(configuracao.SupermercadoId))
                throw new ArgumentException("Não é possível salvar configurações sem um supermercado associado.");

            DocumentReference docRef = _firestoreDb.Collection(NomeColecao).Document(configuracao.SupermercadoId);
            await docRef.SetAsync(configuracao, SetOptions.MergeAll);
        }
    }
}
