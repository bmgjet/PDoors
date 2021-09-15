using UnityEngine;
using Random = System.Random;

namespace Oxide.Plugins
{
    [Info("PDoors", "bmgjet", "1.0.0")]
    [Description("Adds code locks to prison doors and hides door manipulators")]

    public class PDoors : RustPlugin
    {
        const string SeeCodeLocks = "PDoors.SeeCLock";
        const string SeeDoorMan = "PDoors.SeeDMan";
        private void Init()
        {
            permission.RegisterPermission(SeeCodeLocks, this);
            permission.RegisterPermission(SeeDoorMan, this);
        }

        void OnServerInitialized()
        {
            int pg = 0;
            //Delays adding of locks so everything can be fully spawned in.
            timer.Once(5f, () =>
             {
                 foreach (BaseEntity ServerEnt in UnityEngine.Object.FindObjectsOfType<BaseEntity>())
                 {
                     switch (ServerEnt.PrefabName)
                     {
                         case "assets/prefabs/building/wall.frame.cell/wall.frame.cell.gate.prefab":
                             Door ServerDoors = ServerEnt as Door;
                             if (ServerDoors == null || ServerDoors.OwnerID != 0)
                             {
                                 break;
                             }
                             var lockSlot = ServerDoors.GetSlot(BaseEntity.Slot.Lock);
                             if (lockSlot == null)
                             {
                                 AddLock(ServerDoors);
                                 pg++;
                             }
                             break;
                     }
                 }
             });
            Puts(pg.ToString() + " Unlocked Cell Gates Locked");
        }

        //Hides RE placed DoorManipulator to all players with out permission
        object CanNetworkTo(DoorManipulator DM, BasePlayer player)
        {
            if (DM.OwnerID == 0 && !player.IPlayer.HasPermission(SeeDoorMan))
            {
                return false;
            }
            return null;
        }

        //Hides server spawned code locks to all players with out permission
        object CanNetworkTo(CodeLock CL, BasePlayer player)
        {
            if (CL.OwnerID == 0 && !player.IPlayer.HasPermission(SeeCodeLocks))
            {
                return false;
            }
            return null;
        }

        //Adds code lock to entity
        void AddLock(BaseEntity ent)
        {
            CodeLock alock = GameManager.server.CreateEntity("assets/prefabs/locks/keypad/lock.code.prefab", ent.transform.position, ent.transform.rotation) as CodeLock;
            alock.Spawn();
            alock.OwnerID = 0;
            //Sets Random Code
            Random rand = new Random();
            alock.code = rand.Next(1111, 9999).ToString();

            alock.SetParent(ent, ent.GetSlotAnchorName(BaseEntity.Slot.Lock));
            alock.transform.position = ent.transform.position + new Vector3(0,1,0);
            ent.SetSlot(BaseEntity.Slot.Lock, alock);
            alock.SetFlag(BaseEntity.Flags.Locked, true);
            alock.enableSaving = true;
            alock.SendNetworkUpdateImmediate(true);
        }
    }
}