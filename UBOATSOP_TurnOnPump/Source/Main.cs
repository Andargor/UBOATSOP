using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Characters;
using UBOAT.Game.Scene.Characters.Actions;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UnityEngine;

public class UBOATSOP_TurnOnPump : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;

    public const string Version = UBOATSOP_TurnOnPump_Constants.Version;
    public const float INTERVAL_UPDATE_SECONDS = 60.0f;  // run once per minute
    public const float INTERVAL_CHECK_SECONDS = 3600.0f; // check pump every hour
    public const double MAXIMUM_WATER_LEVEL = 0.5;

    private static float secondsElapsedSinceLastCheck = 0f;
    private static bool firstUpdate = true;

    public override void Start()
    {
        try
        {
            Debug.Log($"{this} Version {Version}");
            firstUpdate = true;
            executionQueue.AddTimedUpdateListener(DoUpdate, INTERVAL_UPDATE_SECONDS);
        } catch (System.Exception ex)
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

        } catch (System.Exception ex)
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
        } catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    private static double TotalShipWaterAmount()
    {
        double result = 0.0;

        if (playerShipProxy != null && playerShipProxy.CurrentShip != null) {

        foreach (Compartment compartment in playerShipProxy.CurrentShip.Interior.Compartments)
                result += compartment.Water;
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
                double waterLevel = TotalShipWaterAmount();
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
                            UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo actionInfo = component.GetActionInfo<SwitchPowerAction>((PlayableCharacter)null, true);
                            if (actionInfo.ActionTemplate != null && actionInfo.State == ActionState.Enabled)
                            {
                                Debug.Log($"UBOATSOP_TurnOnPump ENABLE WATER LEVEL {waterLevel}");
                                playerCrew.GlobalActionQueue.Add(actionInfo.ActionTemplate.Duplicate(component));
                                secondsElapsedSinceLastCheck = 0f;
                            }
                        }
                    }
                } else
                {
                    if (equipment.enabled && !equipment.IsWaterLeft)
                    {
                        UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();
                        UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo actionInfo = component.GetActionInfo<SwitchPowerAction>((PlayableCharacter)null, true);
                        if (actionInfo.ActionTemplate != null && actionInfo.State == ActionState.Disabled)
                            playerCrew.GlobalActionQueue.Add(actionInfo.ActionTemplate.Duplicate(component));
                    }
                }
                
            }
        }
    }
}
