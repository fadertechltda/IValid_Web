using DOMAIN.Model;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Produto
{
    public class ProdutoMapeador(FirestoreDb firestoreDb) : IProdutoMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task CadastrarAsync(ProdutoModel produto)
        {
            CollectionReference collection = _firestoreDb.Collection("produtos");
            await collection.AddAsync(produto);
        }
        public async Task AtualizarAsync(ProdutoModel produto)
        {
            DocumentReference docRef = _firestoreDb.Collection("produtos").Document(produto.Id);
            await docRef.SetAsync(produto, SetOptions.MergeAll);
        }

        public async Task DeletarAsync(ProdutoModel produto)
        {
            DocumentReference docRef = _firestoreDb.Collection("produtos").Document(produto.Id);
            await docRef.DeleteAsync();
        }

        public async Task<List<ProdutoModel>> ListarTodosAsync()
        {
            QuerySnapshot snapshot = await _firestoreDb.Collection("produtos").GetSnapshotAsync();

            List<ProdutoModel> lista = [.. snapshot.Documents.Select(doc =>
            {
                ProdutoModel produto = doc.ConvertTo<ProdutoModel>();
                produto.Id = doc.Id;

                return produto;
            })];

            return lista;
        }

        public async Task<ProdutoModel?> ListarPorIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("produtos").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                ProdutoModel produto = snapshot.ConvertTo<ProdutoModel>();
                produto.Id = snapshot.Id;
                return produto;
            }

            return null;
        }      
    }
}
