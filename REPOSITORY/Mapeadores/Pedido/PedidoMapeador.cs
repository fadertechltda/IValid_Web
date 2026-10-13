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

        public Task<List<PedidoModel>> ListarPorSupermercadoAsync(string supermercadoId)
        {
            return ListarAsync(_firestoreDb.Collection("pedidos").WhereEqualTo("supermercadoId", supermercadoId));
        }

        public async Task<List<PedidoModel>> ListarSemSupermercadoAsync()
        {
            var todos = await ListarTodosAsync();
            return [.. todos.Where(p => string.IsNullOrEmpty(p.SupermercadoId))];
        }

        public Task<List<PedidoModel>> ListarPorUsuarioAsync(string usuarioId)
        {
            return ListarAsync(_firestoreDb.Collection("pedidos").WhereEqualTo("userId", usuarioId));
        }

        public Task<List<PedidoModel>> ListarPorStatusAsync(string status)
        {
            return ListarAsync(_firestoreDb.Collection("pedidos").WhereEqualTo("status", status));
        }

        private static async Task<List<PedidoModel>> ListarAsync(Query query)
        {
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return [.. snapshot.Documents.Select(doc =>
            {
                PedidoModel pedido = doc.ConvertTo<PedidoModel>();
                pedido.Id = doc.Id;
                return pedido;
            })];
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
