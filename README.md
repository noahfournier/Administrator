![Administrator - Github](https://github.com/user-attachments/assets/548851c7-1236-4503-b3e8-25f451a988d5)
# 🚀 Administrator - Plugin d'administration pour Nova-Life

Administrator est une solution tout-en-un pour la gestion avancée des serveurs Nova-Life, offrant aux administrateurs un contrôle complet sur les joueurs, les véhicules et les événements en jeu.

## 🌐 Plugin Open Source
Le code de ce plugin est disponible librement dans [`Fichiers`](https://github.com/noahfournier/Administrator/tree/main/Fichiers). Ainsi, vous pouvez voir ce que vous mettez sur votre serveur, aucun élément caché !
Pour les développeurs ou apprentis développeurs, vous pouvez comprendre comment coder un plugin dans son global.

## 🌟 Fonctionnalités principales

### 🧑‍💻 Gestion des joueurs
- **Téléportation avancée**
  - TP vers un joueur
  - TP d'un joueur vers vous
  - TP à des lieux prédéfinis (mairie, hôpital, etc.)
  - Sauvegarde/restauration des positions
- **Gestion d'identité**
  - Modification du prénom/nom
  - Changement de numéro de téléphone
  - Gestion des permis (attribution/points)
- **Santé**
  - Soin complet
  - Guérison de la grippe
- **Progression**
  - Attribution d'XP
  - Ajout de niveaux

### 🚗 Gestion des véhicules
- **Déplacement précis**
  - Avant/arrière
  - Gauche/droite
  - Haut/bas
- **Actions techniques**
  - Retournement du véhicule
  - Réparation complète
  - Remplissage du réservoir
  - Rangement du véhicule

### 🎉 Gestion des événements
- **Communication**
  - Activation/désactivation du mégaphone
- **Contrôle serveur**
  - Mute/unmute global des joueurs
  - Réanimation de tous les joueurs

## 📊 Tableau des permissions

| Fonctionnalité                  | Permission par défaut |
|---------------------------------|-----------------------|
| Accès au menu principal         | 1                     |
| Gestion des joueurs             | 5                     |
| Gestion des véhicules           | 5                     |
| Gestion des événements          | 5                     |

## 📖 Comment l'utiliser

- La commande `/a` / `/admin` / `/administrator` (permission 1 minimum) permet d'ouvrir le menu d'administration.
- Mettez-vous en service admin -> Appuyez sur la touche de ModKit (`P` par défaut) -> Cliquez sur "Administration" -> Cliquez sur "Administrator"

## 📥 Installation

1. Téléchargez la dernière version depuis les [Releases](https://github.com/noahfournier/Administrator/releases)
2. Placez `Administrator.dll` dans le dossier `Plugins` de votre serveur
3. Lancez votre serveur, le fichier `Configuration.json` va alors se créer dans `ModKit/Configuration/Administrator_Configuration.json`
4. Configurez les permissions, le Webhook et d'autres éléments dans `Configuration.json`
5. Redémarrez le serveur

## 📌 Dépendances

- [ModKit + AAMenu](https://github.com/Aarnow/NovaLife_ModKit-Releases/releases/latest)

## ⚖️ Licence

Ce projet est sous **licence open source**. Vous pouvez consulter, modifier et utiliser le code source dans le cadre de vos propres projets, mais vous n'êtes pas autorisé à vendre le code ou à l'utiliser dans des projets commerciaux sans une autorisation explicite de l'auteur.

## 📞 Contact
Pour toute question, problème ou proposition lié au plugin, contactez-moi sur Discord : @noah_fournier.

## 💸 Dons
Si vous estimez que ce plugin mérite une rémunération, vous pouvez retrouver mon paypal ci-contre : [PayPal](https://www.paypal.com/paypalme/noahfournierpro)
