using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene;

public class UBOATSOP_LeavePortButton : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;
    [Inject] private static SandboxSceneOrigin sandboxSceneOrigin;
    [Inject] private static Sandbox sandbox;

    public const string Version = UBOATSOP_LeavePortButton_Constants.Version;
    public static bool firstUpdate = true;

    public static SandboxEntity lastDockEntity = null;
    public static SandboxEntity lastLeavePortEntity = null;
    public override void Start()
    {
        try
        {
            Debug.Log($"{this} Version {Version}");
            firstUpdate = true;
            executionQueue.AddTimedUpdateListener(DoUpdate, 1.0f);
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

    private static LeavePortButtonUI GetLeavePortButton(bool create = false)
    {
        LeavePortButtonUI button = sandbox.gameObject.GetComponent<LeavePortButtonUI>();
        //if (create && button == null) sandbox.gameObject.AddComponent<LeavePortButtonUI>();

        //button.gameObject.SetActive(true);

        //Debug.Log($"UBOATSOP_LeavePortButton GetLeavePortButton CREATE {create} BUTTON {button} ACTIVE {button?.isActiveAndEnabled}");

        return button;
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

                if (lastDockEntity != null && lastLeavePortEntity != null)
                {
                    SandboxEntity shipEntity = playerShipProxy.CurrentShip.SandboxEntity;
                    SandboxEntity portEntity = lastDockEntity;

                    float dist = (shipEntity.Position - portEntity.Position).sqrMagnitude;
                    if (dist < 1.0f)
                    {
                        if (playerShipProxy.CurrentShip.CurrentQuickTravelTarget != lastLeavePortEntity)
                        {
                            Debug.Log($"UBOATSOP_LeavePortButton ShowLeavePortButton LEAVE CURRENT {playerShipProxy.CurrentShip.CurrentQuickTravelTarget?.Name} TARGET {lastLeavePortEntity.Name} GROUP {lastLeavePortEntity.Group.Name}");
                            playerShipProxy.CurrentShip.SetQuickTravelTarget(lastLeavePortEntity, QuickTravelTargetType.LeavePortArea);

                            /*
                            var button = GetLeavePortButton(true);
                            if (button != null && !button.isActiveAndEnabled)
                            {
                                Debug.Log($"UBOATSOP_LeavePortButton SETACTIVE {button}");
                                button.enabled = true;
                                button.gameObject.SetActive(true);
                            }
                            */

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
                    && playerShipProxy.CurrentShip != null
                    && playerShipProxy.CurrentShip.CurrentQuickTravelTarget != null
                    && playerShipProxy.CurrentShip.CurrentQuickTravelTargetType == QuickTravelTargetType.LeavePortArea)
            {
                Debug.Log($"UBOATSOP_LeavePortButton HideLeavePortButton CLEAR TARGET");
                playerShipProxy.CurrentShip.ClearQuickTravelTarget();


            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

}
