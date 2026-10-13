using DOMAIN.Model.Categoria;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Categoria
{
    public class CategoriaMapeador(FirestoreDb firestoreDb) : ICategoriaMapeador
    {
        private const string Colecao = "categories";
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<List<CategoriaModel>> ListarTodasAsync()
        {
            QuerySnapshot snapshot = await _firestoreDb.Collection(Colecao).GetSnapshotAsync();

            return [.. snapshot.Documents.Select(doc =>
            {
                CategoriaModel categoria = doc.ConvertTo<CategoriaModel>();
                categoria.Id = doc.Id;
                return categoria;
            })];
        }

        public async Task<CategoriaModel?> ObterPorIdAsync(string id)
        {
            DocumentSnapshot snapshot = await _firestoreDb.Collection(Colecao).Document(id).GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            CategoriaModel categoria = snapshot.ConvertTo<CategoriaModel>();
            categoria.Id = snapshot.Id;
            return categoria;
        }

        public async Task<string> CriarAsync(CategoriaModel categoria)
        {
            DocumentReference docRef = string.IsNullOrWhiteSpace(categoria.Id)
                ? _firestoreDb.Collection(Colecao).Document()
                : _firestoreDb.Collection(Colecao).Document(categoria.Id);

            await docRef.CreateAsync(categoria);
            return docRef.Id;
        }

        public async Task AtualizarAsync(CategoriaModel categoria)
        {
            await _firestoreDb.Collection(Colecao).Document(categoria.Id).SetAsync(categoria, SetOptions.MergeAll);
        }

        public async Task DeletarAsync(string id)
        {
            await _firestoreDb.Collection(Colecao).Document(id).DeleteAsync();
        }
    }
}
