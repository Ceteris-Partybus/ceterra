using System;
using System.Collections.Generic;

public class Board {
    private List<Lane> lanes;

    public Board() {
        lanes = new List<Lane>();
    }

    public void AddLane(Lane lane) {
        lanes.Add(lane);
    }

    public IReadOnlyList<Lane> Lanes {
        get {
            return lanes.AsReadOnly();
        }
    }
}