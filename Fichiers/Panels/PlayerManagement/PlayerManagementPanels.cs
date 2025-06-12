using Life.Network;
using ModKit.Helper;
using SQLite;
using t = Administrator.Helpers.TextHelper;
using c = Administrator.Helpers.Colors;
using playerHelper = Administrator.Helpers.PlayerHelper;
using Life;
using System.Collections.Generic;
using UnityEngine;
using Crosstales;
using Administrator.Entities;
using ModKit.Helper.DiscordHelper;
using System;

namespace Administrator.Panels.PlayerManagement
{
    public class PlayerManagementPanels
    {
        #region Context

        /// <summary>
        /// Contexte global du plugin.
        /// </summary>
        [Ignore] public static ModKit.ModKit Context { get; set; }

        /// <summary>
        /// Initialise les panels de gestion des joueurs avec le contexte du plugin.
        /// </summary>
        /// <param name="context">Instance du plugin permettant d'accéder à ModKit.</param>
        public PlayerManagementPanels(ModKit.ModKit context)
        {
            Context = context;
        }

        #endregion

        #region Player Management

        /// <summary>
        /// Dictionnaire contenant la précédente position des joueurs ayant été téléportés.
        /// </summary>
        private static Dictionary<Player, Vector3> previousPositions = new Dictionary<Player, Vector3>();

        /// <summary>
        /// Ouvre un panel permettant la gestion des joueurs du serveur.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public async void PlayerManagementPanel(Player player)
        {
            await Administrator_Configuration.Reload();
            var configuration = Administrator_Configuration.Data;

            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Faites un choix"), Life.UI.UIPanel.PanelType.Tab, player, () => PlayerManagementPanel(player));

            panel.AddTabLine("Téléporation", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionJoueurs_Téléportation) GetPlayerTeleportationPanel(player);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Identité", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionJoueurs_Identité) IdentityPanel(player);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Santé", ui => 
            { 
                if (player.account.adminLevel >= configuration.Permissions_GestionJoueurs_Santé) HealthPanel(player);
                else 
                { 
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error); 
                    panel.Refresh(); 
                } 
            });
            panel.AddTabLine("Autre", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionJoueurs_Autre) OtherPlayerActionsPanel(player);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        #endregion

        #region Teleportation

        /// <summary>
        /// Ouvre un panel permettant de rechercher le joueur à téléporter.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public void GetPlayerTeleportationPanel(Player player)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Téléportation", c.BlueColor), Life.UI.UIPanel.PanelType.Input, player, () => GetPlayerTeleportationPanel(player));

            panel.SetText("Quel joueur souhaitez-vous téléporter ?");
            panel.SetInputPlaceholder("<i>Prénom - Nom - ID ...");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Suivant"), () =>
            {
                var targetPlayer = playerHelper.GetPlayer(panel.inputText);
                if (targetPlayer != null) TeleportationPanel(player, targetPlayer);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Joueur introuvable"), "Le joueur recherché est introuvable !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de téléporter un joueur.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void TeleportationPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Téléportation", c.BlueColor), Life.UI.UIPanel.PanelType.Tab, player, () => TeleportationPanel(player, targetPlayer));

            panel.AddTabLine($"Joueur : {targetPlayer.FullName}", ui => panel.Refresh());
            panel.AddTabLine("Se téléporter au joueur", async ui =>
            {
                player.setup.TargetSetPosition(targetPlayer.setup.transform.position);
                player.Notify(c.Format(c.GreenColor, "Téléporté au joueur"), $"Vous avez été téléporté avec succès à {targetPlayer.FullName} !", NotificationManager.Type.Success); 
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Administrateur téléporté**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter le joueur à moi", async ui =>
            {
                previousPositions[targetPlayer] = targetPlayer.setup.transform.position;
                targetPlayer.setup.TargetSetPosition(player.setup.transform.position);
                player.Notify(c.Format(c.GreenColor, "Téléporté à moi"), $"Vous avez téléporté avec succès {targetPlayer.FullName} à votre position !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à l'administrateur**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter le joueur à sa position d'origine", async ui =>
            {
                if (previousPositions.TryGetValue(targetPlayer, out Vector3 previousPosition))
                {
                    targetPlayer.setup.TargetSetPosition(previousPosition);
                    previousPositions.Remove(targetPlayer);
                    player.Notify(c.Format(c.GreenColor, "TP à l'origine"), $"Vous avez téléporté {targetPlayer.FullName} à sa position d'origine !", NotificationManager.Type.Success);
                    await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à sa position d'origine**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, false, true, $"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:R>");
                    panel.Refresh();
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Erreur"), $"Aucune position d'origine enregistrée pour {targetPlayer.FullName}.", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Téléporter à un lieux", ui => LocationTeleportationPanel(player, targetPlayer));

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de téléporter le joueur cible à un lieu.
        /// </summary>
        /// <param name="player">Le joueru ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void LocationTeleportationPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Téléportation", c.BlueColor), Life.UI.UIPanel.PanelType.Tab, player, () => LocationTeleportationPanel(player, targetPlayer));

            panel.AddTabLine("Téléporter à la mairie", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(1).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à la mairie !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à la mairie**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter à l'auto école", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(5).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à l'auto école !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à l'auto école**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter au concessionnaire", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(170).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} au concessionnaire !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté au concessionnaire**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter au dépôt presto", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(6).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} au dépôt presto !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté au dépôt presto**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter à la gare routière", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(505).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à la gare routière !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à la gare routière**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter à l'hopital", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(185).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à l'hopital !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à l'hopital**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter au pôle travail", async ui =>
            {
                PlayerTeleportation(targetPlayer, new Vector3(502.8427f, 50.00305f, 350.2008f));
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} au pôle travail !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté au pôle travail**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter au camp de bûcheron", async ui =>
            {
                PlayerTeleportation(targetPlayer, new Vector3(748.0873f, 50.00305f, 394.6979f));
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} au camp de bûcheron !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté au camp de bûcheron**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter à la fourrière", async ui =>
            {
                PlayerTeleportation(targetPlayer, Nova.a.GetAreaById(376).instance.spawn);
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à la fourrière !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à la fourrière**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter à la fuye", async ui =>
            {
                PlayerTeleportation(targetPlayer, new Vector3(188.1446f, 41.34118f, -431.4476f));
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à la fuye !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à la fuye**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });
            panel.AddTabLine("Téléporter à la reigneire", async ui =>
            {
                PlayerTeleportation(targetPlayer, new Vector3(291.6136f, 44.98566f, -1273.993f));
                player.Notify(c.Format(c.GreenColor, "Joueur téléporté"), $"Vous avez téléporté {targetPlayer.FullName} à la reigneire !", NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📍 Joueur téléporté à la reigneire**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
                panel.Refresh();
            });

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        /// <summary>
        /// Téléporte le joueur cible à la position donnée.
        /// </summary>
        /// <param name="targetPlayer">Le joueur cible.</param>
        /// <param name="position">La position vers laquelle le joueur cible sera téléporté.</param>
        public void PlayerTeleportation(Player targetPlayer, Vector3 position)
        {
            targetPlayer.setup.TargetSetPosition(position);
        }

        #endregion

        #region Identity

        /// <summary>
        /// Ouvre un panel permettant la gestion de l'identité du joueur cible ou de soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public void IdentityPanel(Player player)
        {
            Player targetPlayer = player.GetClosestPlayer() ?? player;

            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Identité", c.BlueColor), Life.UI.UIPanel.PanelType.Tab, player, () => IdentityPanel(player));

            panel.AddTabLine("Modifier le prénom du joueur", ui => PlayerFirstNameModificationPanel(player, targetPlayer));
            panel.AddTabLine("Modifier le nom du joueur", ui => PlayerLastNameModificationPanel(player, targetPlayer));
            panel.AddTabLine("Modifier le numéro du joueur", ui => PlayerPhoneNumberModificationPanel(player, targetPlayer));
            panel.AddTabLine("Donner le permis au joueur", ui => PlayerGiveDrivingLicensePanel(player, targetPlayer));
            panel.AddTabLine("Donner des points de permis au joueur", ui => PlayerGiveDrivingLicensePointsPanel(player, targetPlayer));

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de modifier le prénom du joueur cible ou de soi-même sinon.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerFirstNameModificationPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Identité", c.BlueColor), Life.UI.UIPanel.PanelType.Input, player, () => PlayerFirstNameModificationPanel(player, targetPlayer));

            panel.SetText($"Définir le prénom de {targetPlayer.FullName}");
            panel.SetInputPlaceholder("<i>Nouveau prénom ...");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                if (string.IsNullOrWhiteSpace(panel.inputText))
                {
                    player.Notify(c.Format(c.RedColor, "Champ vide"), "Veuillez entrer un prénom valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                string previousFirstName = targetPlayer.character.Firstname;

                targetPlayer.character.Firstname = panel.inputText.ToLower().CTToTitleCase();

                player.SendText(c.Format(c.GreenColor, $"Le prénom de {previousFirstName} est désormais {targetPlayer.character.Firstname}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**👤 Prénom modifié**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID", "⏱️ Ancien prénom", "🆕 Nouveau prénom" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId, previousFirstName, targetPlayer.character.Firstname }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de modifier le nom du joueur cible ou de soi-même sinon.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerLastNameModificationPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Identité", c.BlueColor), Life.UI.UIPanel.PanelType.Input, player, () => PlayerLastNameModificationPanel(player, targetPlayer));

            panel.SetText($"Définir le nom de {targetPlayer.FullName}");
            panel.SetInputPlaceholder("<i>Nouveau nom ...");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                if (string.IsNullOrWhiteSpace(panel.inputText))
                {
                    player.Notify(c.Format(c.RedColor, "Champ vide"), "Veuillez entrer un nom valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                string previousLastname = targetPlayer.character.Lastname;

                targetPlayer.character.Lastname = panel.inputText.ToLower().CTToTitleCase();

                player.SendText(c.Format(c.GreenColor, $"Le nom de {previousLastname} est désormais {targetPlayer.character.Lastname}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**👤 Nom modifié**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID", "⏱️ Ancien nom", "🆕 Nouveau nom" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId, previousLastname, targetPlayer.character.Lastname }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de modifier le numéro de téléphone du joueur cible ou de soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerPhoneNumberModificationPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Identité", c.BlueColor), Life.UI.UIPanel.PanelType.Input, player, () => PlayerPhoneNumberModificationPanel(player, targetPlayer));

            panel.SetText($"Définir le numéro de {targetPlayer.FullName}");
            panel.SetInputPlaceholder("<i>Nouveau numéro ...");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                if (string.IsNullOrWhiteSpace(panel.inputText))
                {
                    player.Notify(c.Format(c.RedColor, "Champ vide"), "Veuillez entrer un numéro valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                if (!int.TryParse(panel.inputText, out int number))
                {
                    player.Notify(c.Format(c.RedColor, "Numéro invalide"), "Veuillez entrer un numéro valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }
                
                string previousNumero = targetPlayer.character.PhoneNumber;

                targetPlayer.character.PhoneNumber = number.ToString();

                player.SendText(c.Format(c.GreenColor, $"Le numéro {previousNumero} est désormais {targetPlayer.character.PhoneNumber}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📱 Numéro modifié**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID", "⏱️ Ancien numéro", "🆕 Nouveau numéro" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId, previousNumero, targetPlayer.character.PhoneNumber }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de donner le permis de conduire au joueur cible ou à soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerGiveDrivingLicensePanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Identité", c.BlueColor), Life.UI.UIPanel.PanelType.Text, player, () => PlayerGiveDrivingLicensePanel(player, targetPlayer));

            panel.TextLines.Add($"Donner le permis à {targetPlayer.FullName}");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                targetPlayer.character.PermisB = true;
                targetPlayer.character.PermisPoints = 12;

                player.SendText(c.Format(c.GreenColor, $"Le permis a bien été donné à {targetPlayer.FullName}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🪪 Permis donné**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de donner des points sur le permis du joueur cible ou à soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerGiveDrivingLicensePointsPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Identité", c.BlueColor), Life.UI.UIPanel.PanelType.Text, player, () => PlayerGiveDrivingLicensePointsPanel(player, targetPlayer));

            panel.TextLines.Add($"Donner des points de permis à {targetPlayer.FullName}");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                if (string.IsNullOrWhiteSpace(panel.inputText))
                {
                    player.Notify(c.Format(c.RedColor, "Champ vide"), "Veuillez entrer un nombre valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                if (!int.TryParse(panel.inputText, out int number))
                {
                    player.Notify(c.Format(c.RedColor, "Nombre invalide"), "Veuillez entrer un nombre valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                targetPlayer.character.PermisPoints += number;

                player.SendText(c.Format(c.GreenColor, $"{number} points ont bien été ajoutés au permis de {targetPlayer.FullName}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**➕ Points ajoutés**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID", "🔢 Nombre de points" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId, number.ToString() }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        #endregion

        #region Health

        /// <summary>
        /// Ouvre un panel permettant la gestion de la santé du joueur cible ou de soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public void HealthPanel(Player player)
        {
            Player targetPlayer = player.GetClosestPlayer() ?? player;

            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Santé", c.BlueColor), Life.UI.UIPanel.PanelType.Tab, player, () => HealthPanel(player));

            panel.AddTabLine("Soigner le joueur", ui => PlayerCarePlayer(player, targetPlayer));
            panel.AddTabLine("Soigner le joueur de la grippe", ui => PlayerFluCarePlayer(player, targetPlayer));

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de soigner le joueur cible ou soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerCarePlayer(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Santé", c.BlueColor), Life.UI.UIPanel.PanelType.Text, player, () => PlayerCarePlayer(player, targetPlayer));

            panel.TextLines.Add($"Soigner le joueur {targetPlayer.FullName}");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                targetPlayer.Health = 100;

                player.SendText(c.Format(c.GreenColor, $"Le joueur {targetPlayer.FullName} a été soigné."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**❤️‍ Joueur soigné**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de soigner de la grippe le joueur cible ou soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerFluCarePlayer(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Santé", c.BlueColor), Life.UI.UIPanel.PanelType.Text, player, () => PlayerFluCarePlayer(player, targetPlayer));

            panel.TextLines.Add($"Soigner le joueur {targetPlayer.FullName} de la grippe");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                targetPlayer.isSick = false;

                player.SendText(c.Format(c.GreenColor, $"Le joueur {targetPlayer.FullName} a été soigné de la grippe."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**💊 Joueur soigné de la grippe**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        #endregion

        #region Other Player Actions

        /// <summary>
        /// Ouvre un panel permettant d'effectuer toutes les autres actions liées à la gestion des joueurs.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public void OtherPlayerActionsPanel(Player player)
        {
            Player targetPlayer = player.GetClosestPlayer() ?? player;

            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Autre", c.BlueColor), Life.UI.UIPanel.PanelType.Tab, player, () => OtherPlayerActionsPanel(player));

            panel.AddTabLine("Donner de l'XP au joueur", ui => PlayerGiveXpPanel(player, targetPlayer));
            panel.AddTabLine("Donner du niveau au joueur", ui => PlayerGiveLevelPanel(player, targetPlayer));

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de donner de l'XP au joueur cible ou à soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerGiveXpPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Autre", c.BlueColor), Life.UI.UIPanel.PanelType.Input, player, () => PlayerGiveXpPanel(player, targetPlayer));

            panel.SetText($"Donner de l'XP à {targetPlayer.FullName}");
            panel.SetInputPlaceholder("<i>Nombre d'XP ...");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                if (string.IsNullOrWhiteSpace(panel.inputText))
                {
                    player.Notify(c.Format(c.RedColor, "Champ vide"), "Veuillez entrer un nombre valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                if (!int.TryParse(panel.inputText, out int number))
                {
                    player.Notify(c.Format(c.RedColor, "Nombre invalide"), "Veuillez entrer un nombre valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                string previousXP = targetPlayer.character.XP.ToString();

                targetPlayer.character.XP += number;

                player.SendText(c.Format(c.GreenColor, $"L'XP {previousXP} est désormais {targetPlayer.character.XP}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**▶️ XP Donné**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID", "🔢 Nombre d'XP" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId, number.ToString() }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        /// <summary>
        /// Ouvre un panel permettant de donner du niveau au joueur cible ou à soi-même.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        /// <param name="targetPlayer">Le joueur cible.</param>
        public void PlayerGiveLevelPanel(Player player, Player targetPlayer)
        {
            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des joueurs", "Autre", c.BlueColor), Life.UI.UIPanel.PanelType.Input, player, () => PlayerGiveLevelPanel(player, targetPlayer));

            panel.SetText($"Donner du level à {targetPlayer.FullName}");
            panel.SetInputPlaceholder("<i>Nombre de level ...");

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Valider"), async () =>
            {
                if (string.IsNullOrWhiteSpace(panel.inputText))
                {
                    player.Notify(c.Format(c.RedColor, "Champ vide"), "Veuillez entrer un nombre valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                if (!int.TryParse(panel.inputText, out int number))
                {
                    player.Notify(c.Format(c.RedColor, "Nombre invalide"), "Veuillez entrer un nombre valide.", NotificationManager.Type.Error);
                    panel.Refresh();
                    return;
                }

                string previousLevel = targetPlayer.character.Level.ToString();

                targetPlayer.character.Level += number;

                player.SendText(c.Format(c.GreenColor, $"Le level {previousLevel} est désormais {targetPlayer.character.Level}."));
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**💪 Level donné**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🕹️ Joueur cible", "🆔 ID Joueur", "🎮 Steam ID", "🔢 Nombre de level" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, targetPlayer.FullName, targetPlayer.character.Id.ToString(), targetPlayer.account.steamId, number.ToString() }, true, true, "Administrator - By noah_fournier");
            });

            panel.Display();
        }

        #endregion
    }
}
