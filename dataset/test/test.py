import json

from svqa.causal_graph import CausalGraph
from svqa.event import Event


def test__event_class():
    event_dict = json.loads("""{
         "id": 4382325240,
         "objects": [2,3],
         "step": 500,
         "type": "End"
       }""")
    event_a = Event(event_dict)
    return event_a.id == 4382325240


def test__causal_graph():
    with open("../graph.json") as graph_file:
        string = graph_file.read()
        causal_graph = CausalGraph(json.loads(string))

    print(causal_graph.outcome_events(causal_graph.events[0]))

    return True

if __name__ == "__main__":
    assert test__event_class()
    assert test__causal_graph()