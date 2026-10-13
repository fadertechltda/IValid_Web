using DOMAIN.Model.Cliente;
using DOMAIN.Model.Pedido;
using DOMAIN.Model.Produto;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Transacao
{
    public class TransacaoMapeador(FirestoreDb firestoreDb) : ITransacaoMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public string GerarIdPedido() => _firestoreDb.Collection("pedidos").Document().Id;

        public Task<T> ExecutarAsync<T>(Func<ITransacaoContexto, Task<T>> operacao)
        {
            return _firestoreDb.RunTransactionAsync(transacao => operacao(new TransacaoContexto(_firestoreDb, transacao)));
        }

        private sealed class TransacaoContexto(FirestoreDb firestoreDb, Transaction transacao) : ITransacaoContexto
        {
            private readonly FirestoreDb _firestoreDb = firestoreDb;
            private readonly Transaction _transacao = transacao;

            public async Task<ProdutoModel?> ObterProdutoAsync(string id)
            {
                DocumentSnapshot snapshot = await _transacao.GetSnapshotAsync(_firestoreDb.Collection("produtos").Document(id));
                if (!snapshot.Exists) return null;

                ProdutoModel produto = snapshot.ConvertTo<ProdutoModel>();
                produto.Id = snapshot.Id;
                return produto;
            }

            public async Task<ClienteModel?> ObterClienteAsync(string uid)
            {
                DocumentSnapshot snapshot = await _transacao.GetSnapshotAsync(_firestoreDb.Collection("users").Document(uid));
                if (!snapshot.Exists) return null;

                ClienteModel cliente = snapshot.ConvertTo<ClienteModel>();
                cliente.Id = snapshot.Id;
                return cliente;
            }

            public async Task<PedidoModel?> ObterPedidoAsync(string id)
            {
                DocumentSnapshot snapshot = await _transacao.GetSnapshotAsync(_firestoreDb.Collection("pedidos").Document(id));
                if (!snapshot.Exists) return null;

                PedidoModel pedido = snapshot.ConvertTo<PedidoModel>();
                pedido.Id = snapshot.Id;
                return pedido;
            }

            public void AtualizarProduto(string id, Dictionary<string, object> campos)
            {
                _transacao.Update(_firestoreDb.Collection("produtos").Document(id), campos);
            }

            public void MesclarCliente(string uid, Dictionary<string, object> campos)
            {
                campos["updatedAt"] = FieldValue.ServerTimestamp;
                _transacao.Set(_firestoreDb.Collection("users").Document(uid), campos, SetOptions.MergeAll);
            }

            public void CriarPedido(PedidoModel pedido)
            {
                pedido.DataPedido = DateTime.SpecifyKind(pedido.DataPedido, DateTimeKind.Utc);
                _transacao.Create(_firestoreDb.Collection("pedidos").Document(pedido.Id), pedido);
            }

            public void AtualizarPedido(string id, Dictionary<string, object> campos)
            {
                campos["atualizadoEm"] = Timestamp.GetCurrentTimestamp();
                _transacao.Update(_firestoreDb.Collection("pedidos").Document(id), campos);
            }
        }
    }
}
