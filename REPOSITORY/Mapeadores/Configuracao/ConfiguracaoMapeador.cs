using DOMAIN.Model.Configuracao;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Configuracao
{
    public class ConfiguracaoMapeador(FirestoreDb firestoreDb) : IConfiguracaoMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        private const string NomeColecao = "configuracoes";
        private const string IdDocumentoUnico = "geral";

        public async Task<ConfiguracaoModel?> ObterAsync()
        {
            DocumentReference docRef = _firestoreDb.Collection(NomeColecao).Document(IdDocumentoUnico);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            ConfiguracaoModel configuracao = snapshot.ConvertTo<ConfiguracaoModel>();
            configuracao.Id = snapshot.Id;
            return configuracao;
        }

        public async Task SalvarAsync(ConfiguracaoModel configuracao)
        {
            DocumentReference docRef = _firestoreDb.Collection(NomeColecao).Document(IdDocumentoUnico);
            await docRef.SetAsync(configuracao, SetOptions.MergeAll);
        }
    }
}
