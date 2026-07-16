using Google.Cloud.Firestore;
using System;

namespace DOMAIN.Model.Usuario
{
    [FirestoreData]
    public class UsuarioModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("email")]
        public string? Email { get; set; }

        [FirestoreProperty("fullName")]
        public string? NomeCompleto { get; set; }

        [FirestoreProperty("createdAt")]
        public long DataCriacao { get; set; }

        [FirestoreProperty("perfil")]
        public string? PerfilString { get; set; }

        public TipoUsuario Perfil
        {
            get
            {
                if (Enum.TryParse<TipoUsuario>(PerfilString, true, out var result))
                {
                    return result;
                }
                return TipoUsuario.Cliente;
            }
            set
            {
                PerfilString = value.ToString().ToUpper();
            }
        }
    }
}
