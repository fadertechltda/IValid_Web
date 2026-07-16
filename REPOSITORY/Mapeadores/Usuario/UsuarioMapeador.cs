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

            return usuario;
        }

        public async Task CriarAsync(UsuarioModel usuario)
        {
            CollectionReference collection = _firestoreDb.Collection("users");
            await collection.AddAsync(usuario);
        }
    }
}
