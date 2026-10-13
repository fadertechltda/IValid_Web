using Google.Cloud.Firestore;

namespace DOMAIN.Model.Cliente
{
    [FirestoreData]
    public class ImpactoModel
    {
        public const int VersaoMetodologia = 1;

        [FirestoreProperty("itemsRescued")]
        public int ItensResgatados { get; set; }

        [FirestoreProperty("itemsDonated")]
        public int ItensDoados { get; set; }

        [FirestoreProperty("savedReais")]
        public double EconomiaReais { get; set; }

        [FirestoreProperty("donatedReais")]
        public double DoadoReais { get; set; }

        [FirestoreProperty("methodologyVersion")]
        public int Versao { get; set; } = VersaoMetodologia;
    }
}
