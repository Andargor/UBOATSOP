using DWS.Common.InjectionFramework;
using DWS.Common.Resources;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene;
using DWS.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UBOAT.Game.Sandbox.CampaignObjectives;
using UBOAT.Game.UI.Notifications;
using DWS.Common.Pooling;
using UBOAT.Game.Serialization;
using System.Resources;
using UBOAT.Game.Core.Serialization;

public class UBOATSOP_LeavePortButton : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue = null;
    //[Inject] private static INotificationBarUI notificationBarUI = null;
    //[Inject] private static PlayerCrew playerCrew = null;
    [Inject] private static IPlayerShipProxy playerShipProxy = null;
    //[Inject] private static PlayerCareer playerCareer = null;
    //[Inject] private static SandboxSceneOrigin sandboxSceneOrigin;
    //[Inject] private static Sandbox sandbox;
    [Inject] private static GameUI gameUI = null;
    //[Inject] private static DWS.Common.Resources.ResourceManager resourceManager;
    
    public const string Version = UBOATSOP_LeavePortButton_Constants.Version;
    public static bool firstUpdate = true;

    private static LeavePortButtonUI leavePortButton = null;
    private static TraverseCanalButton traverseCanalButton = null;

    [NonSerializedInGameState]
    public static SandboxEntity lastDockEntity = null;

    [NonSerializedInGameState]
    public static SandboxEntity lastLeavePortEntity = null;

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
            if (firstUpdate)
            {
                HideLeavePortButton();
                UpdateLastDock();

            }

            GetLeavePortButton();
            GetTraverseCanalButton();
            ShowLeavePortButton();
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

            playerShipProxy.CurrentShip.DockChanged -= ShipOnDockChanged;
            playerShipProxy.CurrentShip.DockChanged += ShipOnDockChanged;

            playerShipProxy.CurrentShip.AlarmStarted -= ShipOnAlarmStarted;
            playerShipProxy.CurrentShip.AlarmStarted += ShipOnAlarmStarted;


        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    public static void ShipOnDockChanged()
    {
        try
        {
            //Debug.Log($"UBOATSOP_LeavePortButton == EVENT ShipOnDockChanged");

            UpdateLastDock();

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void ShipOnAlarmStarted()
    {
        try
        {
            //Debug.Log($"UBOATSOP_LeavePortButton == EVENT ShipOnAlarmStarted");

            HideLeavePortButton();

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void RMObjectInstantiated(UnityEngine.Object obj, InstantiationFlags flags)
    {
        Debug.Log($"== EVENT RMObjectInstantiated OBJ {obj} NAME {obj?.name} CLASS {obj.GetType()}");
    }
    public static void dumpPool()
    {
        var button = GlobalPool.FindFirstObjectByType<LeavePortButtonUI>();
        Debug.Log($"UBOATSOP_LeavePortButton dumpPool {button}");

    }

    private static LeavePortButtonUI GetLeavePortButton()
    {
        if (leavePortButton == null && gameUI != null && gameUI.gameObject != null) leavePortButton = gameUI?.gameObject?.GetComponentInChildren<LeavePortButtonUI>();

        return leavePortButton;
    }

    private static TraverseCanalButton GetTraverseCanalButton()
    {
        if (traverseCanalButton == null && gameUI != null && gameUI.gameObject != null) traverseCanalButton = gameUI?.gameObject?.GetComponentInChildren<TraverseCanalButton>();
        
        return traverseCanalButton;
    }

    private static void UpdateLastDock(bool force = false)
    {
        if (force)
        {
            lastDockEntity = null;
            lastLeavePortEntity = null;
        }

        if (playerShipProxy != null && playerShipProxy.CurrentShip != null && playerShipProxy.CurrentShip.CurrentDock != null)
        {
            lastDockEntity = playerShipProxy.CurrentShip.CurrentDock;
            lastLeavePortEntity = FindLeavePortArea(lastDockEntity);
        }
    }

    private static SandboxEntity FindLeavePortArea(SandboxEntity portEntity)
    {
        SandboxEntity result = null;

        try
        {
            if (portEntity != null)
            {
                double maxdist = double.NegativeInfinity;
                foreach (var entity in portEntity.Group.Entities)
                {
                    if (entity.Name.ToLower().Contains("leave port"))
                    {
                        result = entity;
                        break;
                    }
                }

                if (result == null)
                {
                    foreach (var entity in portEntity.Group.Entities)
                    {
                        if (entity.Name.ToLower().Contains("undock"))
                        {
                            var dist = (entity.Position - portEntity.Position).sqrMagnitude;
                            if (dist > maxdist && dist > 400.0f)
                            {
                                result = entity;
                                maxdist = dist;
                            }
                        }
                    }
                }
            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return result;
    }

    private static void ShowLeavePortButton()
    {
        try
        {

            if (playerShipProxy != null && playerShipProxy.CurrentShip != null)
            {
                bool leavePortButtonActive = (leavePortButton != null && leavePortButton.isActiveAndEnabled);
                bool traverseCanalActive = (traverseCanalButton != null && traverseCanalButton.isActiveAndEnabled);

                if (playerShipProxy.CurrentShip.Alarmed || traverseCanalActive)
                {
                    HideLeavePortButton();
                    return;
                }
                //Debug.Log($"UBOATSOP_LeavePortButton ShowLeavePortButton lastDockEntity {lastDockEntity?.Name} lastLeavePortEntity {lastLeavePortEntity?.Name} Quick {playerShipProxy.CurrentShip.CurrentQuickTravelTarget?.Name}");

                if (lastDockEntity != null && lastLeavePortEntity != null)
                {
                    SandboxEntity shipEntity = playerShipProxy.CurrentShip.SandboxEntity;
                    SandboxEntity portEntity = lastDockEntity;

                    float dist = (shipEntity.Position - portEntity.Position).sqrMagnitude;
                    //Debug.Log($"UBOATSOP_LeavePortButton ShowLeavePortButton shipEntity {shipEntity?.Name} portEntity {portEntity?.Name} dist {dist}");
                    if (dist < 25.0f)
                    {
                        //if (playerShipProxy.CurrentShip.CurrentQuickTravelTarget != lastLeavePortEntity)
                        if (!leavePortButtonActive)
                        {
                            //Debug.Log($"UBOATSOP_LeavePortButton ShowLeavePortButton LEAVEPORT CURRENT {playerShipProxy.CurrentShip.CurrentQuickTravelTarget?.Name} TARGET {lastLeavePortEntity?.Name}");

                            playerShipProxy.CurrentShip.SetQuickTravelTarget(lastLeavePortEntity, QuickTravelTargetType.LeavePortArea);

                        }
                    } else
                    {
                        HideLeavePortButton();
                        UpdateLastDock();
                    }

                }
            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void HideLeavePortButton()
    {
        try
        {

            if (playerShipProxy != null
                    && playerShipProxy.CurrentShip != null)
            {

                if (playerShipProxy.CurrentShip.CurrentQuickTravelTarget != null && playerShipProxy.CurrentShip.CurrentQuickTravelTargetType != QuickTravelTargetType.LeavePortArea) return;

                //Debug.Log($"UBOATSOP_LeavePortButton HideLeavePortButton CLEAR TARGET");
                if (playerShipProxy.CurrentShip.CurrentQuickTravelTarget != null)playerShipProxy.CurrentShip.ClearQuickTravelTarget();
                
                leavePortButton?.gameObject?.SetActive(false);
            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

}
