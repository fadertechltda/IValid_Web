using DOMAIN.Model.Funcionario;
using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Funcionario
{
    public class FuncionarioMapeador(FirestoreDb firestoreDb) : IFuncionarioMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task CriarAsync(FuncionarioModel funcionario)
        {
            CollectionReference collection = _firestoreDb.Collection("funcionarios");
            await collection.AddAsync(funcionario);
        }

        public async Task AtualizarAsync(FuncionarioModel funcionario)
        {
            DocumentReference docRef = _firestoreDb.Collection("funcionarios").Document(funcionario.Id);
            await docRef.SetAsync(funcionario, SetOptions.MergeAll);
        }

        public async Task<FuncionarioModel?> ListarPorIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("funcionarios").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            FuncionarioModel funcionario = snapshot.ConvertTo<FuncionarioModel>();
            funcionario.Id = snapshot.Id;
            return funcionario;
        }

        public async Task<List<FuncionarioModel>> ListarPorSupermercadoAsync(string supermercadoId)
        {
            Query query = _firestoreDb.Collection("funcionarios").WhereEqualTo("supermercadoId", supermercadoId);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            List<FuncionarioModel> lista = [.. snapshot.Documents.Select(doc =>
            {
                FuncionarioModel funcionario = doc.ConvertTo<FuncionarioModel>();
                funcionario.Id = doc.Id;

                return funcionario;
            })];

            return lista;
        }
    }
}
