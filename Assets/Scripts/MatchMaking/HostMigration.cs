using UnityEngine.Networking;

namespace Nexon.Networking
{
    public class HostMigration : NetworkMigrationManager
    {
        protected override void OnClientDisconnectedFromHost( NetworkConnection conn , out SceneChangeOption sceneChange )
        {
            base.OnClientDisconnectedFromHost( conn , out sceneChange );

            sceneChange = SceneChangeOption.StayInOnlineScene;
        }
    }
}
