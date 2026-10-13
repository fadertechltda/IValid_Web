using DOMAIN.Model.Perfil;
using Google.Cloud.Firestore;
using Grpc.Core;

namespace REPOSITORY.Mapeadores.Perfil
{
    public class PerfilMapeador(FirestoreDb firestoreDb) : IPerfilMapeador
    {
        private const string Colecao = "perfis";
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<List<PerfilModel>> ListarPorSupermercadoAsync(string supermercadoId)
        {
            QuerySnapshot snapshot = await _firestoreDb.Collection(Colecao).WhereEqualTo("supermercadoId", supermercadoId).GetSnapshotAsync();

            return [.. snapshot.Documents.Select(doc =>
            {
                PerfilModel perfil = doc.ConvertTo<PerfilModel>();
                perfil.Id = doc.Id;
                return perfil;
            })];
        }

        public async Task<PerfilModel?> ObterPorIdAsync(string id)
        {
            DocumentSnapshot snapshot = await _firestoreDb.Collection(Colecao).Document(id).GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            PerfilModel perfil = snapshot.ConvertTo<PerfilModel>();
            perfil.Id = snapshot.Id;
            return perfil;
        }

        public async Task<bool> CriarSeNaoExistirAsync(PerfilModel perfil)
        {
            try
            {
                await _firestoreDb.Collection(Colecao).Document(perfil.Id).CreateAsync(perfil);
                return true;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
            {
                return false;
            }
        }

        public async Task<string> CriarAsync(PerfilModel perfil)
        {
            DocumentReference docRef = await _firestoreDb.Collection(Colecao).AddAsync(perfil);
            return docRef.Id;
        }

        public async Task AtualizarAsync(PerfilModel perfil)
        {
            await _firestoreDb.Collection(Colecao).Document(perfil.Id).UpdateAsync(new Dictionary<string, object>
            {
                ["nome"] = perfil.Nome ?? string.Empty,
                ["permissoes"] = perfil.Permissoes
            });
        }

        public async Task DeletarAsync(string id)
        {
            await _firestoreDb.Collection(Colecao).Document(id).DeleteAsync();
        }
    }
}
