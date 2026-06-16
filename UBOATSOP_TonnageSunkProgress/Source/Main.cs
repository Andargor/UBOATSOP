using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Sandbox.Messages;

public class UBOATSOP_TonnageSunkProgress : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;

    public const string Version = UBOATSOP_TonnageSunkProgress_Constants.Version;
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

            /*
            var progress = new LocalizedString("UI/Progress");
            var tonnage = new LocalizedString("Messages/Destroyed", 6666 + " GRT");

            Debug.Log($"UBOATSOP_TonnageSunkProgress MESSAGE {progress}: {tonnage}");
            */

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

            if (playerCareer != null)
            {
                playerCareer.TonnageSunkChanged -= CareerOnTonnageSunkChanged;
                playerCareer.TonnageSunkChanged += CareerOnTonnageSunkChanged;
            }


        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    public static void CareerOnTonnageSunkChanged()
    {
        try
        {
            if (playerCareer != null)
            {
                var progress = new LocalizedString("UI/Progress");
                var tonnage = new LocalizedString("Messages/Destroyed", playerCareer.TonnageSunk.ToString() + " GRT");
                //AddJournalEntry($"Progress: {playerCareer.TonnageSunk} GRT sunk.");
                AddJournalEntry($"{progress}: {tonnage}");
            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }
    private static void AddJournalEntry(string text)
    {
        try
        {
            if (playerShipProxy != null && playerShipProxy.CurrentShip != null && text != null)
            {
                playerShipProxy.CurrentShip.SandboxEntity.Journal.AddMessage((IMessage)new BasicMessage(text, playerCareer.CurrentAssignment?.SubjectTitle ?? new LocalizedString("Misc Communications"), playerShipProxy.CurrentShip.SandboxEntity));
            }

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

}
