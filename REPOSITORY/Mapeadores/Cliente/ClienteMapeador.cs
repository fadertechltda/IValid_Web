using DOMAIN.Model.Cliente;
using Google.Cloud.Firestore;
using Grpc.Core;

namespace REPOSITORY.Mapeadores.Cliente
{
    public class ClienteMapeador(FirestoreDb firestoreDb) : IClienteMapeador
    {
        private const string Colecao = "users";
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        private DocumentReference Documento(string uid) => _firestoreDb.Collection(Colecao).Document(uid);

        public async Task<ClienteModel?> ObterAsync(string uid)
        {
            DocumentSnapshot snapshot = await Documento(uid).GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            ClienteModel cliente = snapshot.ConvertTo<ClienteModel>();
            cliente.Id = snapshot.Id;
            return cliente;
        }

        public async Task<bool> CriarAsync(ClienteModel cliente)
        {
            var dados = new Dictionary<string, object?>
            {
                ["fullName"] = cliente.NomeCompleto,
                ["email"] = cliente.Email,
                ["phone"] = cliente.Telefone,
                ["perfil"] = cliente.Perfil,
                ["totalDonations"] = 0,
                ["availableCashback"] = 0.0,
                ["favoritos"] = new List<string>(),
                ["fcmTokens"] = new List<string>(),
                ["createdAt"] = FieldValue.ServerTimestamp,
                ["updatedAt"] = FieldValue.ServerTimestamp
            };

            try
            {
                await Documento(cliente.Id!).CreateAsync(dados);
                return true;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
            {
                return false;
            }
        }

        public async Task AtualizarCamposAsync(string uid, Dictionary<string, object?> campos)
        {
            campos["updatedAt"] = FieldValue.ServerTimestamp;
            await Documento(uid).SetAsync(campos, SetOptions.MergeAll);
        }

        public async Task AdicionarFavoritoAsync(string uid, string produtoId)
        {
            await Documento(uid).SetAsync(new Dictionary<string, object> { ["favoritos"] = FieldValue.ArrayUnion(produtoId) }, SetOptions.MergeAll);
        }

        public async Task RemoverFavoritoAsync(string uid, string produtoId)
        {
            await Documento(uid).SetAsync(new Dictionary<string, object> { ["favoritos"] = FieldValue.ArrayRemove(produtoId) }, SetOptions.MergeAll);
        }

        public async Task AdicionarTokenDispositivoAsync(string uid, string token)
        {
            await Documento(uid).SetAsync(new Dictionary<string, object> { ["fcmTokens"] = FieldValue.ArrayUnion(token) }, SetOptions.MergeAll);
        }

        public async Task RemoverTokenDispositivoAsync(string uid, string token)
        {
            await Documento(uid).SetAsync(new Dictionary<string, object> { ["fcmTokens"] = FieldValue.ArrayRemove(token) }, SetOptions.MergeAll);
        }
    }
}
