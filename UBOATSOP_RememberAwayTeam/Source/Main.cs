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
using System.Collections.Generic;
using UBOAT.Game.Scene;
using UBOAT.Game.Scene.Stories;



public class UBOATSOP_RememberAwayTeam : BackgroundTaskBase
{
    [Inject] private static IExecutionQueue executionQueue;
    [Inject] private static INotificationBarUI notificationBarUI;
    [Inject] private static PlayerCrew playerCrew;
    [Inject] private static IPlayerShipProxy playerShipProxy;
    [Inject] private static PlayerCareer playerCareer;
    [Inject] private static IStoryPlayerUI storyPlayerUI;
    [Inject] private static GameUI gameUI;


    public const string Version = UBOATSOP_RememberAwayTeam_Constants.Version;
    private static bool firstUpdate = true;

    public static List<AwayTeamSlot> awayTeamSlots = new List<AwayTeamSlot>();

    [Serializable]
    public struct AwayTeamSlot
    {
        public string character;
        public string equipment;
        public AwayTeamSlot(string c, string e)
        {
            character = c;
            equipment = e;
        }
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
            AddListeners();
            firstUpdate = false;

        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return 5.0f;
    }



    private static void AddListeners()
    {
        
        try
        {
            if (storyPlayerUI != null)
            {
                storyPlayerUI.Opened -= StoryOnOpened;
                storyPlayerUI.Opened += StoryOnOpened;

                storyPlayerUI.Closed -= StoryOnClosed;
                storyPlayerUI.Closed += StoryOnClosed;

                if (storyPlayerUI.Story != null)
                {
                    //storyPlayerUI.Story.Updated -= StoryOnUpdated;
                    //storyPlayerUI.Story.Updated += StoryOnUpdated;

                    if (storyPlayerUI.Story is GenericStory)
                    {
                        ((GenericStory)storyPlayerUI.Story).Updated -= StoryOnUpdated;
                        ((GenericStory)storyPlayerUI.Story).Updated += StoryOnUpdated;
                    }
                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        

    }

    private static void StoryOnOpened()
    {
        Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnOpened");
        AddListeners();
    }
    private static void StoryOnClosed()
    {
        Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnClosed");
    }

    private static void CreateCharacterSlots(GenericStory story)
    {

        try
        {
            var ui = GetCharacterSelectionUI();
            Debug.Log($"UBOATSOP_RememberAwayTeam CreateCharacterSlots >> UI {ui} SLOTS {ui?.CharacterSlots?.Count}");

            //List<>
            //ui.CharacterSlots.

            //ui.Close(false);
            //ui.Open(story);
            //ui.transform.hasChanged = true;

            //
            //ui.transform
            //LayoutRebuilder.
            if (ui != null && ui.CharacterSlots != null)
            {
                /*
                //ui.CharacterSlots.
                Interactable selectionInteractable = playerShipProxy.CurrentShip.GroupSelectionInteractable;
                ReadOnlyCollection<Story.CharacterSlot> characterSlots = story.CharacterSlots;
                for (int index = 0; index < characterSlots.Count; ++index)
                {
                    StoryPlayerCharacterSlotUI playerCharacterSlotUi = slotPrefab.Duplicate(index, characterSlots[index], selectionInteractable.Slots[index]);
                    playerCharacterSlotUi.transform.SetParent((Transform)slotContainer, false);
                    //this.characterSlots.Add(playerCharacterSlotUi);
                    ui.CharacterSlots.AddItem<StoryPlayerCharacterSlotUI>(playerCharacterSlotUi);
                }
                */
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }

    private static void StoryOnUpdated(Story baseStory)
    {
        
        try
        {
            Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated STATE {baseStory?.State}");
            //dumpBehaviors();

            var story = (GenericStory)baseStory;
            if (story == null) return;

            dumpAwayTeamSlots();

            var state = story.State;
            Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated STATE2 {state} ISSELECT {state == Story.StoryState.GroupSelection} ISWAIT {state == Story.StoryState.Wait}");

            if (state == Story.StoryState.GroupSelection)
            {
                Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated >>SELECTION COUNT {awayTeamSlots.Count}");
                for (int i = 0; i < awayTeamSlots.Count; i++)
                {
                    //StoryPlayerGroupSelectionUI

                    var slot = awayTeamSlots[i];   
                    var characterSlot = new GenericStory.CharacterSlot(GetCharacterData(slot.character), GetEquipment(slot.equipment));
                    story.SetCharacterSlot(i, characterSlot);

                    

                    //story.OnCharacterSelected(data);

                    //Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated >>I {i} UI {ui} OLD {olddata?.Name} NEW {newdata?.Name}");
                    //ui?.OnCharacterChanged(i, olddata, newdata);
                }
                



                //story.
                CreateCharacterSlots(story);

                //var ui = GetCharacterSelectionUI();
               // Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated >> UI {ui} SLOTS ///{ui?.CharacterSlots?.Count}");


               // if (ui != null && ui.CharacterSlots != null)
               // {
                    //ui.CharacterSlots.AddItem
                    //Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated >>> SLOTS {ui?.CharacterSlots?.Count}");


                    /*
                    Interactable selectionInteractable = StoryPlayerGroupSelectionUI..GroupSelectionInteractable;
                    ReadOnlyCollection<Story.CharacterSlot> characterSlots = story.CharacterSlots;
                    for (int index = 0; index < characterSlots.Count; ++index)
                    {
                        StoryPlayerCharacterSlotUI playerCharacterSlotUi = this.slotPrefab.Duplicate(index, characterSlots[index], selectionInteractable.Slots[index]);
                        playerCharacterSlotUi.transform.SetParent((Transform)this.slotContainer, false);
                        this.characterSlots.Add(playerCharacterSlotUi);
                    }

                    Interactable selectionInteractable = StoryPlayerGroupSelectionUI.playerShip.GroupSelectionInteractable;
                    for (int i = 0; i < story.CharacterSlots.Count; i++)
                    {
                        var slotUI = new StoryPlayerCharacterSlotUI();
                        slotUI.Duplicate(i,story.CharacterSlots[i]);

                    }
                    */
                        
                    
                    //ui.CharacterSlots.AddItem<StoryPlayerCharacterSlotUI>(item);

                    //ui.Open(story);
                    //for (int i = 0; i < ui.CharacterSlots.Count; i++)
                   // {
                        //ui.OnCharacterChanged(i, null, story.CharacterSlots[i].SelectedCharacter);
                        
                   // }
                   // ui.OnItemsChanged();
               // }
                

            } else if (state == Story.StoryState.Wait)
            {
                Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated >>AWAY");
                if (story.CharacterSlots == null) return;

                awayTeamSlots.Clear();
                for (int i = 0; i < story.CharacterSlots.Count; i++)
                {
                    var slot = story.CharacterSlots[i];
                    var id = slot.SelectedCharacter == null ? null : CreateCharacterID(slot.SelectedCharacter);
                    var equipment = slot.SelectedItem == null ? null : slot.SelectedItem.Name;

                    Debug.Log($"UBOATSOP_RememberAwayTeam == EVENT StoryOnUpdated >>> ID {id} E {equipment}");
                    var awayTeamSlot = new AwayTeamSlot(id, equipment);
                    awayTeamSlots.Add(awayTeamSlot);
                }


            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }


        
    }

    private static StoryPlayerGroupSelectionUI GetCharacterSelectionUI()
    {
        //return gameUI?.GetComponentInChildren<StoryPlayerGroupSelectionUI>();
        return ((StoryPlayerUI)storyPlayerUI)?.GetComponentInChildren<StoryPlayerGroupSelectionUI>();
        //return null; 
    }


    public static void dumpBehaviors()
    {
        /*
        var instance = InjectionFramework.Instance;
        List<MonoBehaviour> behaviors = new List<MonoBehaviour>();
        instance.GetSingletons<MonoBehaviour>(behaviors);

        foreach (var behavior in behaviors)
        {
            Debug.Log($"BEHAVIOR {behavior} {behavior.GetType()}");
        }
        */
    }

    public static string CreateCharacterID(PlayableCharacterData data)
    {
        if (data == null) return null;

        string result = data.Name + data.birthDateTicks.ToString();

        return result;
    }
    public static string GetCharacterIDXX(PlayableCharacterData data)
    {
        if (data == null) return null;

        try
        {

            
            var info = data.entity.Characters;
            foreach (var character in info)
            {
                Debug.Log($"UBOATSOP_RememberAwayTeam GetCharacterID >>CHARACTERINFO C {character} ID {character.CharacterId}");
            }


            //var id = info.CharacterId;

            Debug.Log($"UBOATSOP_RememberAwayTeam GetCharacterID D {data} N {data?.Name} E {data?.entity} I {info} ID {data?.entity?.GetCharacterInfo(data).CharacterId}");

            Debug.Log(data.ToJSON());

            return data?.entity?.GetCharacterInfo(data).CharacterId;
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return null;
    }


    public static PlayableCharacterData GetCharacterData(string id)
    {

        try
        {
            if (playerCrew == null) return null;
            //Debug.Log($"UBOATSOP_RememberAwayTeam GetCharacterData SEARCH {id}");
            foreach (var character in playerCrew.Characters)
            {
                var cid = CreateCharacterID(character.Data);
                //Debug.Log($"UBOATSOP_RememberAwayTeam GetCharacterData >>{cid}");
                if (cid == id)
                {
                    //Debug.Log($"UBOATSOP_RememberAwayTeam GetCharacterData MATCH >>{character.Data.Name}");
                    return character.Data;
                }
            }


            //return playerCrew?.Skipper?.Data?.entity?.GetCharacter(id);
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return null;
    }

    public static Equipment GetEquipment(string id)
    {
        if (id == null) return null;

        try
        {
            PlayerShip ship = playerShipProxy?.CurrentShip;
            if (ship == null) return null;

            List<Equipment> equipments = ship.Equipment;
            if (equipments == null) return null;

            foreach (var equipment in equipments)
            {
                if (equipment == null) continue;

                if (equipment.Name == id)
                {
                    return equipment;
                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return null;
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

    private static void dumpAwayTeamSlots()
    {

        try
        {
            var slots = awayTeamSlots;
            if (slots != null)
            {
                for (var i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    var c = slot.character;
                    var e = slot.equipment;

                    //var id = character.entity.GetCharacterInfo().CharacterId;
                    //var character = playerCrew.Skipper.Data.entity.GetCharacter(c);
                    var character = GetCharacterData(c);
                    var equipment = GetEquipment(e);
                    Debug.Log($"UBOATSOP_RememberAwayTeam dumpAwayTeamSlots >>SLOT {i} CHARACTER {c} ITEM {e} DATA {character?.Name} EQUIP {equipment}");

                }
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }

}
