using DOMAIN.Model.Ong;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Ong
{
    public class OngMapeador(FirestoreDb firestoreDb) : IOngMapeador
    {
        private const string Colecao = "ongs";
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<List<OngModel>> ListarTodasAsync()
        {
            QuerySnapshot snapshot = await _firestoreDb.Collection(Colecao).GetSnapshotAsync();

            return [.. snapshot.Documents.Select(doc =>
            {
                OngModel ong = doc.ConvertTo<OngModel>();
                ong.Id = doc.Id;
                return ong;
            })];
        }

        public async Task<OngModel?> ObterPorIdAsync(string id)
        {
            DocumentSnapshot snapshot = await _firestoreDb.Collection(Colecao).Document(id).GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            OngModel ong = snapshot.ConvertTo<OngModel>();
            ong.Id = snapshot.Id;
            return ong;
        }

        public async Task<string> CriarAsync(OngModel ong)
        {
            DocumentReference docRef = await _firestoreDb.Collection(Colecao).AddAsync(ong);
            return docRef.Id;
        }

        public async Task AtualizarAsync(OngModel ong)
        {
            await _firestoreDb.Collection(Colecao).Document(ong.Id).SetAsync(ong, SetOptions.MergeAll);
        }

        public async Task DeletarAsync(string id)
        {
            await _firestoreDb.Collection(Colecao).Document(id).DeleteAsync();
        }
    }
}
