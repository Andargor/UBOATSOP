using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Characters.Actions;
using System.Collections.Generic;
using TMPro;

public class UBOATSOP_CrewPriorities : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;

    public const string Version = UBOATSOP_CrewPriorities_Constants.Version;
    private static bool firstUpdate = true;

    public struct JobInfo
    {
        public JobInfo(int idx, float p, string a)
        {
            index = idx;
            priority = p;
            actionName = a;
        }

        public int index { get; set; }
        public float priority { get; set; }
        public string actionName { get; set; }
        public override string ToString() => $"({actionName} {index}: {priority})";

    }
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
            ManageCrewPriorites();

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

    private static void dumpJobs(Dictionary<string, JobInfo> jobs)
    {
        foreach(var job in jobs)
        {
            Debug.Log($"UBOATSOP_CrewPriorities dumpJobs {job.Key} {job.Value}");
        }
    }

    private static bool HasPlayerOrders(ActionQueue actionQueue)
    {
        List<CharacterAction> actions = actionQueue.Actions;
        for (int index = actions.Count - 1; index >= 0; --index)
        {
            if (actions[index].IsForcedAction)
                return true;
        }
        return false;
    }

    private static void ManageCrewPriorites()
    {

        try
        {
            //Debug.Log("UBOATSOP_CrewPriorities ManageCrewPriorites START");

            if (playerShipProxy != null && playerShipProxy.CurrentShip != null && playerShipProxy.CurrentShip.Alarmed) return;

            var characters = playerCrew.Characters;
            var jobs = new Dictionary<string, JobInfo>();
            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];
                if (character != null && character.IsOfficer)
                {
                    //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites CREW {character.Name} ACTION {character.Action} SOURCE {character.Action?.SourceJob} JOB {character.Action?.SourceJob?.Name}");
                    if (character.Action != null && character.Action?.SourceJob?.Name != "Officer SleepAction" && character.Action?.SourceJob?.Name != "Skipper SleepAction")
                    {
                        //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites >CREW2 {character.Name} ACTION {character.Action} SOURCE {character.Action?.SourceJob} JOB {character.Action?.SourceJob?.Name}");

                        bool flag = HasPlayerOrders(character.ActionQueue);
                        if (flag) continue;

                        ICharacterRoleJob sourceJob = character.Action.SourceJob;
                        if (character.Action.SourceJob == null)
                        {
                            ActionProfitabilityData bestAction = ((CharacterRole)character.Role).GetBestAction(character.ActionQueue.Characters, !flag);
                            //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites >>PROFIT {bestAction.Action} {bestAction.Activator} SOURCE {bestAction.SourceJob?.Name} PROFIT {bestAction.Profit} BASEPRIO {bestAction.SourceJob?.BasePriority} ");

                            sourceJob = bestAction.SourceJob;
                        }

                        //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites >SOURCEJOB {sourceJob?.Name} PRIO {sourceJob?.BasePriority}");

                        string job = sourceJob?.Name;
                        if (job != null)
                        {
                            if (!jobs.ContainsKey(job))
                            {
                                jobs[job] = new JobInfo(i, sourceJob.BasePriority, sourceJob.Name);
                            }
                        } else
                        {
                            Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites *** ERROR {character.Name} NULL JOB");
                        }
                    }
                }
            }

            //dumpJobs(jobs);

            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];
                if (character != null && character.IsOfficer)
                {

                    if (character.Action == null)
                    {

                        //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites CREW3 {character.Name} STATUS {character.Data?.status} ACTION {character.Action?.ToString()} PRIO {character.Action?.SourceJob?.BasePriority} CLASS {character.Data?.characterClass}");

                        var roles = ((CharacterRole)character.Role)?.Jobs;

                        if (roles != null) foreach (var role in roles)
                            {
                                //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites role {role.Name}");

                                if (role != null && role.Name != null && jobs.ContainsKey(role.Name))
                                {

                                    var jobinfo = jobs[role.Name];
                                    var otherchar = characters[jobinfo.index];
                                    //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites -->2 {character.Name} {role?.Name} PRIO {role?.BasePriority} <=> {otherchar?.Name} PRIO {jobinfo.priority}");
                                    if (otherchar != null && otherchar.Action != null && !(otherchar.Action is SleepAction))
                                    {

                                        if (character.Name != otherchar?.Name && role?.BasePriority > jobinfo.priority)
                                        {
                                            Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites *** OVERRIDE {character.Name} {role?.Name} PRIO {role?.BasePriority} <=> {otherchar.Name} PRIO {jobinfo.priority}");

                                            if (otherchar.Action != null && !otherchar.Action.IsDirectPlayerOrder)
                                            {
                                                character.RoleOverride = (ICharacterRole)null;
                                                otherchar.RoleOverride = (ICharacterRole)null;

                                                otherchar.StopCurrentAction();
                                                character.Role?.Update();
                                                otherchar.Role?.Update();
                                                break;
                                            }
                                        }
                                    } else
                                    {
                                        //Debug.Log($"UBOATSOP_CrewPriorities ManageCrewPriorites -->2 *** OVERRIDE ABORT {character.Name} {role?.Name} PRIO {role?.BasePriority} <=> {otherchar.Name} PRIO {jobinfo.priority}");
                                    }
                                }
                            }
                    }
                }
            }

            //Debug.Log("UBOATSOP_CrewPriorities ManageCrewPriorites END");

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }

}
