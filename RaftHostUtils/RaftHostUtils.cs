using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AzureSky;

public class RaftHostUtils : Mod
{
    Harmony harmonyInstance;
    private string harmonyId = "cafe.kirameki.rafthostutils";

    private bool isEnabled = false;
    private Network_Player hostPlayer = null;
    private float originalDayLength = 20; // Default value of raft, 20 minutes

    public void Start()
    {
        harmonyInstance = new Harmony(harmonyId);
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        setWorldVariables();

        Debug.Log("Mod RaftHostUtils has been loaded!");
    }

    public void OnModUnload()
    {
        harmonyInstance.UnpatchAll(harmonyId);

        if (isEnabled)
        {
            anchorRaft(false);
            setDayCycle(originalDayLength);
            disablePassiveMobs();
            Debug.Log("Host mode disabled");
        }

        Debug.Log("Mod RaftHostUtils has been unloaded!");
    }

    public override void WorldEvent_WorldLoaded()
    {
        setWorldVariables();
    }

    public void Update()
    {
        if (!LoadSceneManager.IsGameSceneLoaded) {
            if (isEnabled)
            {
                isEnabled = false;
                originalDayLength = 20;
            }
            return;
        }

        if (hostPlayer)
        {
            hostPlayer.Stats.stat_thirst.Normal.Value = 100;
            hostPlayer.Stats.stat_hunger.Normal.Value = 100;
            hostPlayer.Stats.stat_health.Value = 100;
            hostPlayer.Stats.stat_oxygen.Value = 100;
        }

        int onlinePlayers = getOnlinePlayers();
        if (onlinePlayers == 0)
        {
            if (isEnabled) return;

            anchorRaft(true);
            setDayCycle(0);
            enablePassiveMobs();
            isEnabled = true;
            Debug.Log("Host mode enabled");
        }
        else {
            if (!isEnabled) return;

            anchorRaft(false);
            setDayCycle(originalDayLength);
            disablePassiveMobs();
            isEnabled = false;
            Debug.Log("Host mode disabled");
        } 
    }

    [HarmonyPatch(typeof(AI_State_Attack_Block), "FindBlockToAttack")]
    public static class AI_State_Attack_Block_Patch
    {
        private static void Postfix(ref Block __result)
        {
            if (getOnlinePlayers() == 0)
                __result = null;
        }
    }

    [HarmonyPatch(typeof(AI_State_Attack_Block_Shark), "FindBlockToAttack")]
    public static class AI_State_Attack_Block_Shark_Patch
    {
        private static void Postfix(ref Block __result)
        {
            if (getOnlinePlayers() == 0)
                __result = null;
        }
    }

    public void setWorldVariables() {
        AzureSkyController skyController = FindObjectOfType<AzureSkyController>();
        if (skyController)
            originalDayLength = skyController.timeOfDay.dayCycle;

        Network_Player player = RAPI.GetLocalPlayer();
        if (player)
            hostPlayer = player;
    }

    public static int getOnlinePlayers()
    {
        if (RAPI.GetLocalPlayer() == null)
            return 0;

        List<Network_Player> currentPlayers = new List<Network_Player>();

        var players = FindObjectsOfType<Network_Player>();
        var localPlayer = RAPI.GetLocalPlayer();
        foreach (var player in players)
        {
            if (player != localPlayer)
                currentPlayers.Add(player);
        }

        return currentPlayers.Count;
    }

    public static void anchorRaft(bool status)
    {
        Raft raft = ComponentManager<Raft>.Value;
        if (!raft) return;

        if (status)
            raft.AddAnchor(false, null);
        else
            raft.RemoveAnchor(10);
    }

    public static void setDayCycle(float cycle)
    {
        AzureSkyController skyController = FindObjectOfType<AzureSkyController>();
        if (skyController) {
            skyController.timeOfDay.dayCycle = cycle;
            Traverse.Create(skyController).Field<float>("m_timeProgression").Value = skyController.timeOfDay.GetDayLength();
        }
    }

    public static void enablePassiveMobs()
    {
        SO_GameModeValue gameModeValue = GameModeValueManager.GetCurrentGameModeValue();
        if (!gameModeValue) return;

        bool isCreative = GameModeValueManager.GetCurrentGameModeValue().Equals(GameMode.Creative);
        if (isCreative) return;

        gameModeValue.sharkVariables.isTame = true;

        gameModeValue.seagullVariables.isTame = true;
        gameModeValue.seagullVariables.attacksCrops = false;
        gameModeValue.stonebirdVariables.isTame = true;

        gameModeValue.bearVariables.isTame = true;
        gameModeValue.boarVariables.isTame = true;

        gameModeValue.pigVariables.isTame = true;
        gameModeValue.ratVariables.isTame = true;
        gameModeValue.hyenaVariables.isTame = true;

        gameModeValue.pufferfishVariables.isTame = true;
        gameModeValue.anglerFishVariables.isTame = true;

        gameModeValue.butlerBotVariables.isTame = true;

        gameModeValue.varunaBossVariables.isTame = true;
        gameModeValue.utopiaBossVariables.isTame = true;
    }

    public static void disablePassiveMobs()
    {
        SO_GameModeValue gameModeValue = GameModeValueManager.GetCurrentGameModeValue();
        if (!gameModeValue) return;

        bool isCreative = GameModeValueManager.GetCurrentGameModeValue().Equals(GameMode.Creative);
        gameModeValue.sharkVariables.isTame = isCreative;

        gameModeValue.seagullVariables.isTame = isCreative;
        gameModeValue.seagullVariables.attacksCrops = isCreative;
        gameModeValue.stonebirdVariables.isTame = isCreative;

        gameModeValue.bearVariables.isTame = isCreative;
        gameModeValue.boarVariables.isTame = isCreative;

        gameModeValue.pigVariables.isTame = isCreative;
        gameModeValue.ratVariables.isTame = isCreative;
        gameModeValue.hyenaVariables.isTame = isCreative;

        gameModeValue.pufferfishVariables.isTame = isCreative;
        gameModeValue.anglerFishVariables.isTame = isCreative;

        gameModeValue.butlerBotVariables.isTame = isCreative;

        gameModeValue.varunaBossVariables.isTame = isCreative;
        gameModeValue.utopiaBossVariables.isTame = isCreative;
    }
}