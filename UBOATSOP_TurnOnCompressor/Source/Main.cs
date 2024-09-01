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

public class UBOATSOP_TurnOnCompressor : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;

    public const string Version = UBOATSOP_TurnOnCompressor_Constants.Version;
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
            TurnOnCompressor();
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


    private static void TurnOnCompressor(float minFillRatio = 0.95f)
    {
        if (playerShipProxy != null && playerShipProxy.CurrentShip != null && !playerShipProxy.CurrentShip.Alarmed)
        {
            bool freshAir = false;
            Parameter gain = playerShipProxy.CurrentShip.GetResource("Oxygen")?.Gain;
            if (gain != null)
            {
                Modifier deltaModifier = gain.GetDeltaModifier("Fresh Air");
                if ((double)deltaModifier.Value > 0.0) freshAir = true;
            }

            if (freshAir)
            {
                AirCompressor equipment = playerShipProxy.CurrentShip.GetEquipment<AirCompressor>("Junkers Diesel Compressor");
                if (!(bool)(UnityEngine.Object)equipment)
                {
                    equipment = playerShipProxy.CurrentShip.GetEquipment<AirCompressor>("Electric Compressor");
                }
                if ((bool)(UnityEngine.Object)equipment && !equipment.enabled && equipment.TotalFillRatio < minFillRatio)
                {
                    UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();
                    UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo action = component.GetAction<SwitchPowerAction>((PlayableCharacter)null, true);
                    if (action.Action != null && action.State == ActionState.Enabled)
                        playerCrew.GlobalActionQueue.Add(action.Action.Duplicate(component));
                }
            }
        }
    }
}
