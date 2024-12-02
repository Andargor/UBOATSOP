using HarmonyLib;
using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Core.Data;
using UBOAT.Game.Scene.Items;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Characters.Actions;


public class UBOATSOP_TurnOnPump : BackgroundTaskBase, IUserMod
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;
    [Inject] private static UserSettings userSettings;

    public const string Version = UBOATSOP_TurnOnPump_Constants.Version;
    public const float INTERVAL_UPDATE_SECONDS = 60.0f;  // run once per minute
    public const float INTERVAL_CHECK_SECONDS = 3600.0f; // check pump every hour
    public const double MAXIMUM_WATER_LEVEL = 0.5;

    private static float secondsElapsedSinceLastCheck = 0f;
    private static bool firstUpdate = true;

    [NonSerializedInGameState]
    public void OnLoaded()
    {
        var harmony = new Harmony("com.andargor.UBOATSOP_TurnOnPump");
        harmony.PatchAll();
        Debug.Log($"{this} Version {Version} Harmony Loaded");
    }

    public override void Start()
    {
        try
        {
            Debug.Log($"{this} Version {Version}");
            firstUpdate = true;
            executionQueue.AddTimedUpdateListener(DoUpdate, INTERVAL_UPDATE_SECONDS);
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static float DoUpdate()
    {
        try
        {
            TurnOnPump();
            firstUpdate = false;

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return 5.0f;
    }

    protected override void OnFinished()
    {

        try
        {
            executionQueue.RemoveTimedUpdateListener(DoUpdate);
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    private static double TrimPumpWaterAmount(TrimPump pump)
    {
        double result = 0.0;

        try
        {
            if (pump)
            {
                TrimPump.ConnectedTank[] tanks = (TrimPump.ConnectedTank[])Traverse.Create(pump)?.Field("tanks")?.GetValue();

                if (tanks != null)
                {
                    for (int i = 0; i < tanks.Length; i++)
                    {
                        result += tanks[i].Tank.Water;
                    }
                }
                //Debug.Log($"UBOATSOP_TurnOnPump TrimPumpWaterAmount PUMP {pump} TANKS {tanks} AMOUNT {result}");
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return result;
    }

    private static void TurnOnPump()
    {

        secondsElapsedSinceLastCheck += INTERVAL_UPDATE_SECONDS;
        if (secondsElapsedSinceLastCheck > float.MaxValue) secondsElapsedSinceLastCheck = 0;

        if (playerShipProxy != null && playerShipProxy.CurrentShip != null)
        {
            TrimPump equipment = playerShipProxy.CurrentShip.GetEquipment<TrimPump>("Trim Pump");
            if ((bool)(UnityEngine.Object)equipment)
            {
                double waterLevel = TrimPumpWaterAmount(equipment);
                //Debug.Log($"UBOATSOP_TurnOnPump WATER LEVEL {waterLevel}");
                
                if (waterLevel >= MAXIMUM_WATER_LEVEL)
                {
                    if (!equipment.enabled 
                        && !playerShipProxy.CurrentShip.Alarmed
                        && !playerShipProxy.CurrentShip.SubmergedOrGoingToSubmerge)
                    {
                        if (secondsElapsedSinceLastCheck > INTERVAL_CHECK_SECONDS)
                        {
                            UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();
                            UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo action = component.GetAction<SwitchPowerAction>((PlayableCharacter)null, true);
                            if (action.Action != null && action.State == ActionState.Enabled)
                            {
                                Debug.Log($"UBOATSOP_TurnOnPump ENABLE WATER LEVEL {waterLevel}");
                                playerCrew.GlobalActionQueue.Add(action.Action.Duplicate(component));
                                secondsElapsedSinceLastCheck = 0f;
                            }
                        }
                    }
                } else
                {
                    if (equipment.enabled && !equipment.IsWaterLeft)
                    {
                        UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();
                        UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo action = component.GetAction<SwitchPowerAction>((PlayableCharacter)null, true);
                        if (action.Action != null && action.State == ActionState.Disabled)
                            playerCrew.GlobalActionQueue.Add(action.Action.Duplicate(component));
                    }
                }
                
            }
        }
    }
}
