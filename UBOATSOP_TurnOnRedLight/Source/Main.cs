using DWS.Common.InjectionFramework;
using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.UI;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Characters;
using UnityEngine;
using System;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UBOAT.Game.Scene.Characters.Actions;
using System.Collections.Generic;
using UBOAT.Game.Scene.Characters.Activators;
using UBOAT.Game.Scene.Environment;
using UBOAT.Game.Scene.Effects;


public class UBOATSOP_TurnOnRedLight : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;
    [Inject] private static Weather weather;
    //[Inject] private static PlayerShipInteriorLighting lighting;

    public const string Version = UBOATSOP_TurnOnRedLight_Constants.Version;
    private static bool firstUpdate = true;

    private static PlayerShipInteriorLighting playerShipInteriorLighting = null;
    
    private const int NUM_UPDATE_ITERATIONS = 6;
    private static int lightingIterationsRemaining = 0;
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
            if (weather.IsNight) TurnOnRedLight();
            else TurnOnWhiteLight();

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

            if (playerCareer != null)
            {
                weather.DarkWeatherStarted -= WeatherOnDarkWeatherStarted;
                weather.DarkWeatherStarted += WeatherOnDarkWeatherStarted;

                weather.DarkWeatherEnded -= WeatherOnDarkWeatherEnded;
                weather.DarkWeatherEnded += WeatherOnDarkWeatherEnded;
            }



        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    private static void WeatherOnDarkWeatherStarted()
    {
        Debug.Log($"UBOATSOP_TurnOnRedLight == EVENT WeatherOnDarkWeatherStarted");
        TurnOnRedLight();
    }

    private static void WeatherOnDarkWeatherEnded()
    {
        Debug.Log($"UBOATSOP_TurnOnRedLight == EVENT WeatherOnDarkWeatherEnded");
        TurnOnWhiteLight();
    }

    private static void SimpleRedLight()
    {

        try
        {
            LightSwitch equipment = playerShipProxy.CurrentShip.GetEquipment<LightSwitch>();
            if ((bool)(UnityEngine.Object)equipment)
            {
                equipment.SetColor(UBOAT.Game.Scene.Effects.LightController.Preset.Alarm);
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }

    private static PlayerShipInteriorLighting GetShipInteriorLighting()
    {
        try
        {
            if (playerShipInteriorLighting == null)
            {
                var instance = InjectionFramework.Instance;
                List<PlayerShipInteriorLighting> shipInteriorLightings = new List<PlayerShipInteriorLighting>();
                instance.GetSingletons<PlayerShipInteriorLighting>(shipInteriorLightings);

                foreach (var lighting in shipInteriorLightings)
                {
                    Debug.Log($"UBOATSOP_TurnOnRedLight TurnOnRedLight >LIGHTING CURRENT {lighting.CurrentPreset} DOMINANT {lighting.DominantPreset}");

                    playerShipInteriorLighting = lighting;
                    break;
                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return playerShipInteriorLighting;
    }
    private static void TurnOnRedLight()
    {
        try
        {

            if (lightingIterationsRemaining-- > 0) return;

            lightingIterationsRemaining = 0;

            GetShipInteriorLighting();

            //Debug.Log($"UBOATSOP_TurnOnRedLight TurnOnRedLight WEATHER {weather} LIGHTING {playerShipInteriorLighting?.DominantPreset} NORMAL {playerShipInteriorLighting?.DominantPreset == PlayerShipInteriorLighting.LightSet.Normal}");
            
            if (playerShipProxy != null
                && playerShipProxy.CurrentShip != null
                && !playerShipProxy.CurrentShip.Alarmed
                && weather.IsNight
                && playerShipInteriorLighting?.DominantPreset == PlayerShipInteriorLighting.LightSet.Normal
                )
            {

                LightSwitch equipment = playerShipProxy.CurrentShip.GetEquipment<LightSwitch>();
                if ((bool)(UnityEngine.Object)equipment)
                {

                    UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();

                    List<UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo> actionsList = component.GetActionsList((PlayableCharacter)null, ActionContext.Normal, true);
                    List<SwitchLightAction> actionBuffer = new List<SwitchLightAction>();

                    for (int index1 = actionsList.Count - 1; index1 >= 0; --index1)
                    {
                        if (actionsList[index1].Action is SwitchLightAction action)
                        {
                            if (action.Preset == LightController.Preset.Alarm)
                            {
                                try
                                {
                                    List<CharacterAction> actions = playerCrew.GlobalActionQueue.Actions;
                                    for (int index2 = actions.Count - 1; index2 >= 0; --index2)
                                    {
                                        if (actions[index2] is SwitchLightAction switchLightAction)
                                        {
                                            if (switchLightAction.Preset != LightController.Preset.Alarm) actionBuffer.Add(switchLightAction);
                                        }
                                    }
                                    for (int index3 = actionBuffer.Count - 1; index3 >= 0; --index3)
                                        playerCrew.GlobalActionQueue.Remove((CharacterAction)actionBuffer[index3]);
                                } catch (Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                                playerCrew.GlobalActionQueue.Add(actionsList[index1].Action.Duplicate(component));
                                lightingIterationsRemaining = NUM_UPDATE_ITERATIONS;
                            }
                        }
                    }
                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void TurnOnWhiteLight()
    {
        try
        {

            if (lightingIterationsRemaining-- > 0) return;

            lightingIterationsRemaining = 0;

            GetShipInteriorLighting();

            //Debug.Log($"UBOATSOP_TurnOnRedLight TurnOnWhiteLight WEATHER {weather} LIGHTING {playerShipInteriorLighting?.DominantPreset} NORMAL {playerShipInteriorLighting?.DominantPreset == PlayerShipInteriorLighting.LightSet.Normal}");

            if (playerShipProxy != null
                && playerShipProxy.CurrentShip != null
                && !playerShipProxy.CurrentShip.Alarmed
                && !weather.IsNight
                && playerShipInteriorLighting?.DominantPreset == PlayerShipInteriorLighting.LightSet.Red
                )
            {

                LightSwitch equipment = playerShipProxy.CurrentShip.GetEquipment<LightSwitch>();
                if ((bool)(UnityEngine.Object)equipment)
                {

                    UBOAT.Game.Scene.Characters.Activators.Activator component = equipment.GetComponent<UBOAT.Game.Scene.Characters.Activators.Activator>();

                    List<UBOAT.Game.Scene.Characters.Skills.CharacterActionInfo> actionsList = component.GetActionsList((PlayableCharacter)null, ActionContext.Normal, true);
                    List<SwitchLightAction> actionBuffer = new List<SwitchLightAction>();

                    for (int index1 = actionsList.Count - 1; index1 >= 0; --index1)
                    {
                        if (actionsList[index1].Action is SwitchLightAction action)
                        {
                            if (action.Preset == LightController.Preset.Surface)
                            {
                                try
                                {
                                    List<CharacterAction> actions = playerCrew.GlobalActionQueue.Actions;
                                    for (int index2 = actions.Count - 1; index2 >= 0; --index2)
                                    {
                                        if (actions[index2] is SwitchLightAction switchLightAction)
                                        {
                                            if (switchLightAction.Preset != LightController.Preset.Surface) actionBuffer.Add(switchLightAction);
                                        }
                                    }
                                    for (int index3 = actionBuffer.Count - 1; index3 >= 0; --index3)
                                        playerCrew.GlobalActionQueue.Remove((CharacterAction)actionBuffer[index3]);
                                } catch (Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                                playerCrew.GlobalActionQueue.Add(actionsList[index1].Action.Duplicate(component));
                                lightingIterationsRemaining = NUM_UPDATE_ITERATIONS;
                            }
                        }
                    }
                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
