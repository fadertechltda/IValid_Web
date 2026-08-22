using DOMAIN.Model.Supermercado;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Supermercado
{
    public class SupermercadoMapeador(FirestoreDb firestoreDb) : ISupermercadoMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<string> CriarAsync(SupermercadoModel supermercado)
        {
            CollectionReference collection = _firestoreDb.Collection("supermercados");
            DocumentReference docRef = await collection.AddAsync(supermercado);
            return docRef.Id;
        }

        public async Task<SupermercadoModel?> ListarPorIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("supermercados").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            SupermercadoModel supermercado = snapshot.ConvertTo<SupermercadoModel>();
            supermercado.Id = snapshot.Id;
            return supermercado;
        }

        public async Task<SupermercadoModel?> ObterPorCodigoAcessoAsync(string codigoAcesso)
        {
            Query query = _firestoreDb.Collection("supermercados").WhereEqualTo("codigoAcesso", codigoAcesso);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count == 0)
                return null;

            DocumentSnapshot doc = snapshot.Documents[0];
            SupermercadoModel supermercado = doc.ConvertTo<SupermercadoModel>();
            supermercado.Id = doc.Id;
            return supermercado;
        }
    }
}
