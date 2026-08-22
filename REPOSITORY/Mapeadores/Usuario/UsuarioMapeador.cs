using DOMAIN.Model.Usuario;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Usuario
{
    public class UsuarioMapeador(FirestoreDb firestoreDb) : IUsuarioMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<UsuarioModel?> ObterPorEmailAsync(string email)
        {
            Query query = _firestoreDb.Collection("users").WhereEqualTo("email", email);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count == 0)
                return null;

            DocumentSnapshot doc = snapshot.Documents[0];
            UsuarioModel usuario = doc.ConvertTo<UsuarioModel>();
            usuario.Id = doc.Id;
            usuario.DataCriacao = ExtrairDataCriacao(doc);

            return usuario;
        }

        public async Task<UsuarioModel?> ObterPorIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("users").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            UsuarioModel usuario = snapshot.ConvertTo<UsuarioModel>();
            usuario.Id = snapshot.Id;
            usuario.DataCriacao = ExtrairDataCriacao(snapshot);

            return usuario;
        }

        public async Task<UsuarioModel?> ObterPorSupermercadoIdAsync(string supermercadoId)
        {
            Query query = _firestoreDb.Collection("users").WhereEqualTo("supermercadoId", supermercadoId);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count == 0)
                return null;

            DocumentSnapshot doc = snapshot.Documents[0];
            UsuarioModel usuario = doc.ConvertTo<UsuarioModel>();
            usuario.Id = doc.Id;
            usuario.DataCriacao = ExtrairDataCriacao(doc);

            return usuario;
        }

        public async Task CriarAsync(UsuarioModel usuario)
        {
            CollectionReference collection = _firestoreDb.Collection("users");
            DocumentReference docRef = await collection.AddAsync(usuario);
            await docRef.UpdateAsync("createdAt", Timestamp.FromDateTime(DateTime.UtcNow));
        }

        private static long ExtrairDataCriacao(DocumentSnapshot snapshot)
        {
            if (!snapshot.TryGetValue<object>("createdAt", out var valor) || valor == null)
                return 0;

            return valor switch
            {
                Timestamp timestamp => timestamp.ToDateTimeOffset().ToUnixTimeMilliseconds(),
                long milissegundos => milissegundos,
                _ => 0
            };
        }
    }
}
