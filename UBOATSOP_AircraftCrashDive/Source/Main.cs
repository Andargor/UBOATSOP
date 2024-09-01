using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;


public class UBOATSOP_AircraftCrashDive : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;

    public const string Version = UBOATSOP_AircraftCrashDive_Constants.Version;
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
            AddListeners();

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

    private static void AddListeners()
    {
        try
        {

            if (playerShipProxy != null && playerShipProxy.CurrentShip != null)
            {


                playerShipProxy.ObservationAdded -= ShipOnObservationAdded;
                playerShipProxy.ObservationAdded += ShipOnObservationAdded;

            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    public static void ShipOnObservationAdded(DirectObservationAddedEvent e)
    {
        try
        {
            var entity = e.Observation?.Entity;

            //Debug.Log($"UBOATSOP_AircraftCrashDive ShipOnObservationAdded NAME {entity?.Name} COUNTRY {entity?.Country} RELATION {entity?.Country?.GetRelationWith(playerShipProxy?.Country)}");

            if (entity != null 
                    && entity is Aircraft
                    && playerShipProxy != null
                    && playerShipProxy.CurrentShip != null
                    && entity.Country.GetRelationWith(playerShipProxy.Country) == Country.Relation.Enemy
                    )
            {
                //bool isAircraft = (e.Observation?.Entity is Aircraft);
                Debug.Log($"== EVENT ShipOnObservationAdded OBS {e.Observator?.Name} ENT {e.Observation?.Entity?.Name} PREV {e.PreviousLostObservation?.PerceivedName} AIRCRAFT");

                var aircraft = (Aircraft)entity;
                //var id = aircraft.GetInstanceID();
                Debug.Log($"UBOATSOP_AircraftCrashDive ShipOnObservationAdded *** AIRCRAFT {aircraft.Name} ActiveEngines {aircraft.ActiveEngines} enabled {aircraft.enabled} FoldedUp {aircraft.FoldedUp} HasWorkingPropellers {aircraft.HasWorkingPropellers} isActiveAndEnabled {aircraft.isActiveAndEnabled} IsAwaken {aircraft.IsAwaken}");
                if (!playerShipProxy.CurrentShip.Docked && !playerShipProxy.CurrentShip.SubmergedOrGoingToSubmerge && !aircraft.FoldedUp)
                {
                    CrashDive(DepthPreset.MaxSafeDepth);
                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void CrashDive(DepthPreset preset)
    {
        if (playerShipProxy != null && playerShipProxy.CurrentShip != null)
        {
            //Debug.Log($"++ EVENT ShipOnObservationAdded Diving!");
            notificationBarUI.OpenNow("Icons/Notification Bar/Notification - 40 - Danger", "Dive! Dive! Dive!");

            playerShipProxy.CurrentShip.StartAlarm("Crash Diving");
            playerShipProxy.CurrentShip.FollowDiveSchedule = false; ;
            playerShipProxy.CurrentShip.SetDepthPreset(preset, false);
            playerShipProxy.CurrentShip.OrderEngineGear(5, false);
        }
    }
}
