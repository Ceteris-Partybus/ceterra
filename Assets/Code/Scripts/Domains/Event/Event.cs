
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Event {
    private static int nextId = 1;

    [SerializeField] public int id;
    [SerializeField] public string title;
    [SerializeField] public string description;
    [SerializeField] public List<EventModifier> modifiers;
    [SerializeField] public int maxOccurrences;
    [SerializeField] public int weight;
    [SerializeField] public int occurrences;
    [SerializeField] public bool canOccur;

    public Event() { }

    [JsonConstructor]
    private Event(string title, string description, List<EventModifier> modifiers, int maxOccurrences, int weight) {
        this.id = nextId++;
        this.title = title;
        this.description = description;
        this.modifiers = modifiers;
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

    public static List<Event> LoadEventsFromResources() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Domains/Event/EventList");
        List<Event> eventList = JsonConvert.DeserializeObject<List<Event>>(jsonFile.text);
        return eventList;
    }
}