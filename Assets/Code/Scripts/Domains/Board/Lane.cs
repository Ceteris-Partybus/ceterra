using System;
using System.Collections.Generic;

public class Lane {
    private int id;
    private List<Field> fields;

    public Lane(int id) {
        this.id = id;
        fields = new List<Field>();
    }

    public void AddField(Field field) {
        if (field != null && !fields.Contains(field)) {
            fields.Add(field);
        }
    }

    public int Id {
        get {
            return id;
        }
    }

    public IReadOnlyList<Field> Fields {
        get {
            return fields.AsReadOnly();
        }
    }
}