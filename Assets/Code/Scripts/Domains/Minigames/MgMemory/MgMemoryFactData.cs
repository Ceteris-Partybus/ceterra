using System;

[Serializable]
public class MemoryFactData {
    public string title;
    public string description;
    public string imagePath;

    public MemoryFactData() {
        title = "";
        description = "";
        imagePath = "";
    }

    public MemoryFactData(string title, string description, string imagePath) {
        this.title = title;
        this.description = description;
        this.imagePath = imagePath;
    }
}