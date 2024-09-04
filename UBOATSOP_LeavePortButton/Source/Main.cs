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

public class UBOATSOP_LeavePortButton : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;
    //[Inject] private static SandboxSceneOrigin sandboxSceneOrigin;
    //[Inject] private static Sandbox sandbox;
    [Inject] private static GameUI gameUI;
    //[Inject] private static DWS.Common.Resources.ResourceManager resourceManager;
    
    public const string Version = UBOATSOP_LeavePortButton_Constants.Version;
    public static bool firstUpdate = true;

    public static SandboxEntity lastDockEntity = null;
    public static SandboxEntity lastLeavePortEntity = null;

    private static LeavePortButtonUI leavePortButton = null;

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

                /*
                var instance = InjectionFramework.Instance;
                List<DeepMonoBehaviour> behaviors = new List<DeepMonoBehaviour>();
                instance.GetSingletons<DeepMonoBehaviour>(behaviors);

                foreach (var behavior in behaviors)
                {
                    Debug.Log($"BEHAVIOR {behavior} {behavior.GetType()} PARENT {behavior.GetParentEntity()}");
                }
                */
            }

            /*
            var button = GameObject.FindFirstObjectByType<LeavePortButtonUI>();

            if (button != null)
            {
                Debug.Log($"BUTTON {button} {button.gameObject.scene} {button.GetParentEntity()}");
            }
            */


            //Debug.Log($"UBOATSOP_LeavePortButton DoUpdate BUTTON {button} ACTIVE {button?.isActiveAndEnabled}");
            //sandboxSceneOrigin.

            //scene.GetRootGameObjects(this.gameObjectsBuffer);
            //var instance = DWS.Common.InjectionFramework.Instance;


            GetLeavePortButton();
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

            //resourceManager.ObjectInstantiated -= RMObjectInstantiated;
            //resourceManager.ObjectInstantiated += RMObjectInstantiated;

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

    public static void dumpBehaviors()
    {
        /*
        var instance = InjectionFramework.Instance;
        List<DeepMonoBehaviour> behaviors = new List<DeepMonoBehaviour>();
        instance.GetSingletons<DeepMonoBehaviour>(behaviors);

        foreach (var behavior in behaviors)
        {
            Debug.Log($"+++dumpBehaviors {behavior} {behavior.GetType()}");


            if (behavior is StoryPlayerUI)
            { 
                storyPlayerUI = (StoryPlayerUI)behavior;
                Debug.Log($"+++dumpBehaviors >> ASSIGNED storyPlayerUI {storyPlayerUI}");
            }
        }
        */
        //instance.GetInstance<DeepMonoBehaviour>();
    }

    public static void RMObjectInstantiated(UnityEngine.Object obj, InstantiationFlags flags)
    {
        Debug.Log($"== EVENT RMObjectInstantiated OBJ {obj} NAME {obj?.name} CLASS {obj.GetType()}");
    }
    public static void dumpPool()
    {
        var button = GlobalPool.FindFirstObjectByType<LeavePortButtonUI>();
        Debug.Log($"UBOATSOP_LeavePortButton dumpPool {button}");

        //var b = resourceManager?.GetObject("Leave Port Button");
        //Debug.Log($"UBOATSOP_LeavePortButton dumpPool RM 1 OBJ {b} LP {(LeavePortButtonUI)b} NAME {((LeavePortButtonUI)b)?.name}");

        //ResourceManager.
        //resourceManager.
        //GlobalPool.Instance.GetPrefabInstance(;

        //GlobalPool.Instance.
        //DynamicPrefabLink.
        //GameObjectPrefab

        //UBOAT.Game.Scene.Items.DockPoint.GetRandomDockPoint(this.targetPort.SpawnedEntity.transform, this.ship.Blueprint.Type.Category, true) : (UBOAT.Game.Scene.Items.DockPoint) null;

    }

    private static LeavePortButtonUI GetLeavePortButton()
    {
        if (leavePortButton == null) leavePortButton = gameUI?.gameObject?.GetComponentInChildren<LeavePortButtonUI>();
        return leavePortButton;
    }

    private static LeavePortButtonUI GetLeavePortButtonXXX(bool create = false)
    {
        /*
        Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton GAMEUI {gameUI} MODE {gameUI.CurrentMode}");
        Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton TARGET {playerShipProxy?.CurrentShip?.CurrentQuickTravelTarget} TYPE {playerShipProxy?.CurrentShip?.CurrentQuickTravelTargetType}");

        //gameUI.MainCanvas
        //if (create && button == null) sandbox.gameObject.AddComponent<LeavePortButtonUI>();
        //button.gameObject.SetActive(true);

        LeavePortButtonUI button = gameUI.gameObject.GetComponentInChildren<LeavePortButtonUI>();
        var scene = button?.gameObject?.scene;

        var parentEntity = button?.GetParentEntity();
        var transform = button?.gameObject?.transform;
        var parent = transform?.parent;
        var canvasGroup = button?.GetComponent<CanvasGroup>();

        //canvasGroup.
        //transform.

        Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton BUTTON {button} NAME {button?.name} OBJNAME {button?.gameObject?.name} LAYER {button?.gameObject?.layer} SCENE {button?.gameObject?.scene} ACTIVE {button?.isActiveAndEnabled} TAG {button?.gameObject?.tag} ENABLED {button?.enabled} AWAKEN {button?.IsAwaken} DESTROYED {button?.IsBeingDestroyed} CANVAS {canvasGroup} PARENTENTITY {parentEntity} TRANSFORM {transform} PARENT {parent}");


        //dumpLeavePortButtonUI();
        dumpBehaviors();

        LeavePortButtonUI button2 = storyPlayerUI?.GetComponentInChildren<LeavePortButtonUI>();
        Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton storyPlayerUI BUTTON {button2} NAME {button2?.name}");

        var button3 = InjectionFramework.Instance.GetInstance<LeavePortButtonUI>();
        Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton Instance BUTTON {button3} NAME {button3?.name}");





        dumpPool();


        */



        //var button3 = GameObject.FindFirstObjectByType<LeavePortButtonUI>();
        //var parent3 = button?.GetParentEntity();
        //Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton 3 BUTTON {button3} ACTIVE {button3?.isActiveAndEnabled} PARENT {parent3} {parent3?.Name}");

        //campaignObjectiveBannersUI.


        /*
        ReadOnlyCollection<ICampaignObjective> list = campaignObjectiveBannersUI.campaignObjectivesManager.List;
        for (int index = list.Count - 1; index >= 0; --index)
        {
            ICampaignObjective campaignObjective = list[index];
            if (!campaignObjective.HideInUI)
            {
                CampaignObjectiveBannerUI objectiveBannerUi = this.bannerPrefab.Duplicate(campaignObjective);
                objectiveBannerUi.name = campaignObjective.Id;
                objectiveBannerUi.transform.SetParent((Transform)this.bannersContainer, false);
                objectiveBannerUi.interactable = this.isInteractable;
                this.banners.Add(objectiveBannerUi);
            }
        }
        */




        return null;
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

                if (playerShipProxy.CurrentShip.Alarmed)
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
                        if (playerShipProxy.CurrentShip.CurrentQuickTravelTarget != lastLeavePortEntity)
                        {
                            Debug.Log($"UBOATSOP_LeavePortButton ShowLeavePortButton LEAVEPORT CURRENT {playerShipProxy.CurrentShip.CurrentQuickTravelTarget?.Name} TARGET {lastLeavePortEntity?.Name}");

                            playerShipProxy.CurrentShip.SetQuickTravelTarget(lastLeavePortEntity, QuickTravelTargetType.LeavePortArea);

                        }
                    } else
                    {
                        HideLeavePortButton();
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

            //var button = GetLeavePortButton();
            //button?.gameObject.SetActive(false);
            //button?.Invoke("ValidateState",0f);
            //if (button != null) button.enabled = false;
            //button?.gameObject.SetActive(false);
            
            //Debug.Log($"UBOATSOP_LeavePortButton HideLeavePortButton Q {playerShipProxy.CurrentShip.CurrentQuickTravelTarget} T {playerShipProxy.CurrentShip.CurrentQuickTravelTargetType}");
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
