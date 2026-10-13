using Google.Cloud.Firestore;

namespace REPOSITORY.Mapeadores.Rotina
{
    public class RotinaMapeador(FirestoreDb firestoreDb) : IRotinaMapeador
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public async Task<string?> ObterUltimaExecucaoAsync(string rotina)
        {
            DocumentSnapshot snapshot = await _firestoreDb.Collection("rotinas").Document(rotina).GetSnapshotAsync();
            return snapshot.Exists && snapshot.TryGetValue<string>("ultimaExecucao", out var marca) ? marca : null;
        }

        public async Task RegistrarExecucaoAsync(string rotina, string marca)
        {
            await _firestoreDb.Collection("rotinas").Document(rotina).SetAsync(new Dictionary<string, object>
            {
                ["ultimaExecucao"] = marca,
                ["atualizadoEm"] = FieldValue.ServerTimestamp
            }, SetOptions.MergeAll);
        }
    }
}
