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
using UBOAT.Game.Scene.Characters.Actions;
using UBOAT.Game.Scene.Items;

public class UBOATSOP_TurnOnPump : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;
    [Inject] private static UserSettings userSettings;

    public const string Version = UBOATSOP_TurnOnPump_Constants.Version;
    private static bool firstUpdate = true;
    
    public override void Start()
    {
        try
        {
            Debug.Log($"{this} Version {Version}");
            firstUpdate = true;
            executionQueue.AddTimedUpdateListener(DoUpdate, 5.0f);
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


    private static void TurnOnPump()
    {
        if (playerShipProxy != null && playerShipProxy.CurrentShip != null
                )
        {
            TrimPump equipment = playerShipProxy.CurrentShip.GetEquipment<TrimPump>("Trim Pump");
            if ((bool)(UnityEngine.Object)equipment)
            {
                if (equipment.IsWaterLeft)
                {
                    if (!equipment.enabled 
                        && !playerShipProxy.CurrentShip.Alarmed
                        && !playerShipProxy.CurrentShip.SubmergedOrGoingToSubmerge)
                    {
                        UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();
                        UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo action = component.GetAction<SwitchPowerAction>((PlayableCharacter)null, true);
                        if (action.Action != null && action.State == ActionState.Enabled)
                            playerCrew.GlobalActionQueue.Add(action.Action.Duplicate(component));
                    }
                } else
                {
                    if (equipment.enabled)
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
