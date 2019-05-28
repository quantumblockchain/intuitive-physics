import sys

from enum import Enum
import json
import subprocess
import os
import ast
import random
import matplotlib
matplotlib.use('PS')
import pylab as plt
import matplotlib.image as mpimg
import numpy as np
from PIL import Image

class SimulationState(Enum):
    CREATE_SCENE = 0
    REMOVE_OBJECT = 1

SimulationControllerState = {
  'simulationID': 0,
  'simulationState': 0,
  'imageWidth': 512,
  'imageHeight': 512,
  'removedObjectIndex': 0,
  'noObjects': 0,
  'initialBigObjects': 10,
  'throwMinX': -3,
  'throwMaxX': 3,
  'throwMinY': 25,
  'throwMaxY': 25,
  'throwMinZ': -3,
  'throwMaxZ': 3,
  'stopWaitFrame': 0,
  'maxFramesToWaitPerObject': 450,
  'inputSceneJSON': "InitialStable.json"
}

def writeControllerAsJSON(controller, filepath):
    controllerJSON = json.dumps(controller, sort_keys=False)
    with open(filepath, 'w') as outfile:
        outfile.write(controllerJSON)

def readInitialStableConfigurationObjectCount(filePath):
    sceneJSONStr = ''
    with open(filePath, 'r') as inFile:
        sceneJSONStr = inFile.read()

    sceneDict = ast.literal_eval(sceneJSONStr)
    return len(sceneDict['objectStates'])


def convertSegmentationImageColorsToIndices(img, noObjects):
    thresholds = []
    for i in range(1, noObjects+1):
        thresholds.append(int(i * 255.0 / noObjects))

    ret = np.zeros([img.shape[0], img.shape[1], 3], dtype=np.uint8)

    threshConfidence = 3
    for i in range(0, len(thresholds)):
        lower = thresholds[i]-threshConfidence
        upper = thresholds[i]+threshConfidence
        mask = ((img >= [lower, lower, lower]) & (img <= [upper, upper, upper])).all(axis=2)

        id = i+1

        ret[mask] = [id, id, id]

    return ret

def convertSegmentationImageIndicesToColors(img, noObjects):
    inc = int(255.0 / noObjects)
    ret = inc * img
    return ret


simulation_count = 100

unity_call_str = './what-if-test.app/Contents/MacOS/what-if-test -batchmode'

controller_json_filepath = 'controller.json'
initial_stable_json_path = 'InitialStable.json'

dataBaseFolder = 'Data'


minNoObjects = 15
maxNoObjects = 25

for i in range(simulation_count):
    simulationFolder = os.path.join(dataBaseFolder, str(i).zfill(4))

    while(not os.path.exists(simulationFolder)):
        noObjects = random.randint(minNoObjects, maxNoObjects)

        #Create initial stable configuration
        SimulationControllerState['simulationID'] = i
        SimulationControllerState['simulationState'] = 0
        SimulationControllerState['noObjects'] = noObjects

        writeControllerAsJSON(SimulationControllerState, controller_json_filepath)

        p1 = subprocess.Popen([unity_call_str], shell=True, stdout=subprocess.PIPE)
        p1.wait()

        print(noObjects, " objects are created")

        #Create segmentation map for the initial configuration
        SimulationControllerState['simulationState'] = 2

        writeControllerAsJSON(SimulationControllerState, controller_json_filepath)

        p1 = subprocess.Popen([unity_call_str], shell=True, stdout=subprocess.PIPE)
        p1.wait()

        segmentationImageNameVisible = os.path.join(simulationFolder, (os.path.splitext(initial_stable_json_path)[0]+"_VisibleSeg.png"))
        initialSegmentationImageVisible = np.uint8(mpimg.imread(segmentationImageNameVisible) * 255)

        initialSegmentationImageNONVisible = convertSegmentationImageColorsToIndices(initialSegmentationImageVisible, noObjects)
        segmentationImageNameNONVisible = os.path.join(simulationFolder, (os.path.splitext(initial_stable_json_path)[0]+"_NONVisibleSeg.png"))
        im = Image.fromarray(initialSegmentationImageNONVisible)
        im.save(segmentationImageNameNONVisible)

        #Start creating final stable configurations
        if (os.path.exists(simulationFolder)):
            for j in range(noObjects):
                SimulationControllerState['simulationState'] = 1
                SimulationControllerState['inputSceneJSON'] = 'InitialStable.json'
                SimulationControllerState['removedObjectIndex'] = j

                writeControllerAsJSON(SimulationControllerState, controller_json_filepath)

                p2 = subprocess.Popen([unity_call_str], shell=True, stdout=subprocess.PIPE)
                p2.wait()

                #Create segmentation maps for the final stable configurations
                SimulationControllerState['simulationState'] = 2
                SimulationControllerState['inputSceneJSON'] = "FinalStable_" + str(j).zfill(4) + ".json"

                writeControllerAsJSON(SimulationControllerState, controller_json_filepath)

                p2 = subprocess.Popen([unity_call_str], shell=True, stdout=subprocess.PIPE)
                p2.wait()

                segmentationImageNameVisible = os.path.join(simulationFolder, (os.path.splitext(SimulationControllerState['inputSceneJSON'])[0] + "_VisibleSeg.png"))
                finalSegmentationImageVisible = np.uint8(mpimg.imread(segmentationImageNameVisible) * 255)

                finalSegmentationImageNONVisible = convertSegmentationImageColorsToIndices(finalSegmentationImageVisible, noObjects)
                segmentationImageNameNONVisible = os.path.join(simulationFolder, (os.path.splitext(SimulationControllerState['inputSceneJSON'])[0] + "_NONVisibleSeg.png"))
                im = Image.fromarray(finalSegmentationImageNONVisible)
                im.save(segmentationImageNameNONVisible)




