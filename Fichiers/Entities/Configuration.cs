namespace Administrator.Entities
{
    public class Administrator_Configuration : ModKit.Helper.JsonHelper.JsonEntity<Administrator_Configuration>
    {
        public string Nom = "Administrator - <i>By noah_fournier</i>";
        public string Description = "Administration du serveur";
        public int Permissions_Commande = 1;
        public string Webhook = "https://discord.com/api/webhooks/...";

        public int Permissions_GestionJoueurs_Téléportation = 5;
        public int Permissions_GestionJoueurs_Identité = 5;
        public int Permissions_GestionJoueurs_Santé = 5;
        public int Permissions_GestionJoueurs_Autre = 5;

        public int Permissions_GestionVehicules_Déplacement = 5;
        public int Permissions_GestionVehicules_Réparation = 5;
        public int Permissions_GestionVehicules_Remplissage = 5;
        public int Permissions_GestionVehicules_Rangement = 5;

        public int Permissions_GestionEvenements_Megaphone = 5;
        public int Permissions_GestionEvenements_MuteAll = 5;
        public int Permissions_GestionEvenements_Reanimation = 5;
    }
}