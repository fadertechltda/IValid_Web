using DOMAIN.Model.Pedido;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Pedido
{
    public class PedidoMapeador(FirestoreDb firestoreDb) : IPedidoMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<List<PedidoModel>> ListarTodosAsync()
        {
            QuerySnapshot snapshot = await _firestoreDb.Collection("pedidos").GetSnapshotAsync();

            List<PedidoModel> lista = [.. snapshot.Documents.Select(doc =>
            {
                PedidoModel pedido = doc.ConvertTo<PedidoModel>();
                pedido.Id = doc.Id;

                return pedido;
            })];

            return lista;
        }

        public async Task<PedidoModel?> ListarPorIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("pedidos").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                PedidoModel pedido = snapshot.ConvertTo<PedidoModel>();
                pedido.Id = snapshot.Id;
                return pedido;
            }

            return null;
        }
    }
}
