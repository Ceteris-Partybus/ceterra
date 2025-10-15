
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Event {
    private static int nextId = 1;

    [SerializeField] public int id;
    [SerializeField] public long title;
    [SerializeField] public long description;
    [SerializeField] public List<EventModifier> modifier;
    [SerializeField] public int maxOccurrences;
    [SerializeField] public int weight;
    [SerializeField] public int occurrences;
    [SerializeField] public bool canOccur;

    public Event() { }

    [JsonConstructor]
    private Event(long title, long description, List<EventModifier> modifier, int maxOccurrences, int weight) {
        this.id = nextId++;
        this.title = title;
        this.description = description;
        this.modifier = modifier;
        this.maxOccurrences = maxOccurrences;
        this.weight = weight;
        this.occurrences = 0;
        this.canOccur = true;
    }

    public void MarkOccurrence() {
        occurrences++;
        if (occurrences >= maxOccurrences) {
            canOccur = false;
        }
    }

    public void ResetOccurrences() {
        occurrences = 0;
        canOccur = true;
    }

    public static List<Event> LoadEventsFromResources() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Domains/Event/EventList");
        List<Event> eventList = JsonConvert.DeserializeObject<List<Event>>(jsonFile.text);
        return eventList;
    }
}