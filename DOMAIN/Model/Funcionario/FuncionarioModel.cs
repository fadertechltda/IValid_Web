using Google.Cloud.Firestore;

namespace DOMAIN.Model.Funcionario
{
    [FirestoreData]
    public class FuncionarioModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("nome")]
        public string? Nome { get; set; }

        [FirestoreProperty("senhaHash")]
        public string? SenhaHash { get; set; }

        [FirestoreProperty("supermercadoId")]
        public string? SupermercadoId { get; set; }

        [FirestoreProperty("perfil")]
        public string? PerfilString { get; set; }

        [FirestoreProperty("ativo")]
        public bool Ativo { get; set; } = true;

        public PerfilFuncionario Perfil
        {
            get
            {
                if (Enum.TryParse<PerfilFuncionario>(PerfilString, true, out var resultado))
                {
                    return resultado;
                }
                return PerfilFuncionario.Atendente;
            }
            set
            {
                PerfilString = value.ToString();
            }
        }
    }
}
