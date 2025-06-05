# Field / Splines

## Definitionen

- **Spline-Container**: Ein Spline-Container ist ein Komponente der Splines API, die eine Sammlung von Splines enthält.
- **Splines**: Splines sind Teil der Unity Splines API, die dem Aufbau des Spielbretts zugrunde liegen. Sie bestehen aus Knotenpunkten, die durch Kurven verbunden sind und die Form des Spielfelds definieren.
- **Links**: Links sind Verbindungen zweier Knoten verschiedener Splines. Sie ermöglichen es, Knoten von Spline A mit Knoten von Spline B zu verbinden.
- **Knoten**: Ein Knoten ist einfach ein Punkt im Dreidimensionalen Raum, der Teil eines Splines ist. Knoten derselben Spline sind automatisch miteinander verbunden, zählen aber nicht als **Link**.

## Warum die API wrappen?
Wir zweckentfremden die Splines API gewissermaßen. Der Wrapper dient dazu, die Logik so einfach wie möglich zu halten und auf unsere Domäne anzupassen. Links bspw. werden von der Splines API so dargestellt, dass beide Knoten der Splines weiter existieren, unser Spielfeld soll aber nicht zwei Felder auf einem Knoten haben.

## Wrapper
Der Wrapper ist quasi einfach eine Linked List von **Feldern**. Wenn von einem **Feld** gesprochen wird, befinden wir uns in der Domäne des Wrappers. Ein Feld speichert folgende Informationen:

```cs
private int id;
private int splineId;
private FieldType type;
private List<Field> next;
private SplineKnotIndex splineKnotIndex;
```

Die `id` ist eine künstliche ID, die für die Identifikation des Feldes dient. Sie stimmt nicht immer mit der ID des Knotens überein!
Die `splineId` ist die ID des Splines, zu dem das Feld gehört. Diese ist gleich der Spline ID des Knotens.
Der `type` ist der Feld-Typ (NORMAL, FRAGE, EVENT, KATASTROPHE).
Die `next` Liste enthält die nächsten Felder, die von diesem Feld aus erreicht werden können.
Der `splineKnotIndex` dient zur Identifikation des Knotens in der Domäne der Splines API.