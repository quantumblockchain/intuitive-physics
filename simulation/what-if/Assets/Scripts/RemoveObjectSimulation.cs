﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RemoveObjectSimulation : PhysicsSimulationsBase
{
    enum SimulationState
    {
        CREATE_SCENE = 0,
        REMOVE_OBJECT = 1 
    }

    enum CreateSceneState
    {
        THROW = 0,
        WAIT_WITH_PIPE = 1,
        WAIT_WITHOUT_PIPE = 2,
        STOP = 3
    }

    enum RemoveObjectState
    {
        WAIT = 0,
        STOP = 1
    }


    public string simulationFolder = "";

    public int frameRate = 30;
    private int noObjects = 0;  //Randomly calculated

    private int objectThrownFrameInterval = 100;

    //State of the simulation, whether it is being run 
    private CreateSceneState createSceneState = CreateSceneState.THROW;
    private RemoveObjectState removeObjectState = RemoveObjectState.WAIT;

    private int numberOfDistinctObjectUsed = 4; //Rubber/Metal/Small/Big Cubes
    private int numberOfDistinctColorsUsed = 8;

    private int maxSimulationFrames = 0;

    private GameObject[] pipeObjects = null;

    private SimulationControllerState controllerState = null;

    protected override void Start()
    {
        base.Start();
        // Set the playback framerate (real time will not relate to game time after this).
        Time.captureFramerate = frameRate;

        string constrollerJSON = File.ReadAllText("controller.json");
        controllerState = SimulationControllerState.fromJSON(constrollerJSON);


        simulationFolder = "Data/";
        string simulationIDString = controllerState.simulationID.ToString().PadLeft(4, '0');
        simulationFolder = string.Concat(simulationFolder, simulationIDString);

        pipeObjects = GameObject.FindGameObjectsWithTag("Pipe");
        if ((SimulationState)controllerState.simulationState == SimulationState.CREATE_SCENE)
        {
            noObjects = controllerState.noObjects;

            // Create the folder
            System.IO.Directory.CreateDirectory(simulationFolder);
        }
        else if((SimulationState)controllerState.simulationState == SimulationState.REMOVE_OBJECT)
        {
            foreach (GameObject obj in pipeObjects)
            {
                obj.SetActive(false);
            }

            CreateSceneFromJSON();
            noObjects = createdSimulationObjects.Count;
        }

        maxSimulationFrames = controllerState.maxFramesToWaitPerObject * noObjects;
    }

    void Update()
    {
        if((SimulationState)controllerState.simulationState == SimulationState.CREATE_SCENE)
        {
            if (createSceneState == CreateSceneState.THROW)
            {
                if (Time.frameCount % objectThrownFrameInterval == 0)
                {
                    AddRandomSimulationObject();
                    noObjects--;
                }

                if (noObjects <= 0)
                {
                    createSceneState = CreateSceneState.WAIT_WITH_PIPE;
                }
            }
            else if (createSceneState == CreateSceneState.WAIT_WITH_PIPE)
            {
                if (isSceneStable())
                {
                    foreach (GameObject obj in pipeObjects)
                    {
                        obj.SetActive(false);
                    }
                    createSceneState = CreateSceneState.WAIT_WITHOUT_PIPE;
                }
            }
            else if(createSceneState == CreateSceneState.WAIT_WITHOUT_PIPE)
            {
                if(isSceneStable())
                {
                    // Append filename to folder name (format is '0005 shot.png"')
                    string imageFileName = string.Format("{0}/InitialStable.png", simulationFolder);
                    string jsonFileName = string.Format("{0}/InitialStable.json", simulationFolder);
                    WriteSceneToJSON(jsonFileName);


                    // Capture the screenshot to the specified file.
                    StartCoroutine(captureScreenshot(imageFileName, controllerState.imageWidth, controllerState.imageHeight));
                    createSceneState = CreateSceneState.STOP;
                }
            }
            else if (createSceneState == CreateSceneState.STOP)
            {
                if(controllerState.stopWaitFrame<=0)
                {
                    stop();
                }
                controllerState.stopWaitFrame--;
            }

            if(maxSimulationFrames != 0 && Time.frameCount >= maxSimulationFrames)
            {
                //Reset simulation by deleting the directory
                System.IO.Directory.Delete(simulationFolder);
                stop();
            }
        }

        else if ((SimulationState)controllerState.simulationState == SimulationState.REMOVE_OBJECT)
        {
            if(removeObjectState == RemoveObjectState.WAIT)
            {
                if (isSceneStable())
                { 
                    string imageFileName = string.Format("{0}/FinaleStable_{1:D04}.png", simulationFolder, controllerState.removedObjectIndex);
                    string jsonFileName = string.Format("{0}/FinaleStable_{1:D04}.json", simulationFolder, controllerState.removedObjectIndex);
                    WriteSceneToJSON(jsonFileName);

                    StartCoroutine(captureScreenshot(imageFileName, controllerState.imageWidth, controllerState.imageHeight));
                    removeObjectState = RemoveObjectState.STOP;
                }
            }
            else if (removeObjectState == RemoveObjectState.STOP)
            {
                if (controllerState.stopWaitFrame <= 0)
                {
                    stop();
                }
                controllerState.stopWaitFrame--;
            }

            if (maxSimulationFrames != 0 && Time.frameCount >= maxSimulationFrames)
            {
                //Reset simulation by deleting the directory
                System.IO.Directory.Delete(simulationFolder);
                stop();
            }
        }


        /*
        else if(state == SimulationState.REMOVE_OBJECT)
        {
            if(currentRemovedObjectIndex < gameObjectTemplates.Length)
            {
                gameObjectTemplates[currentRemovedObjectIndex].SetActive(false);
                state = SimulationState.FINAL_UNSTABLE;
            }
            else {
                // Append filename to folder name (format is '0005 shot.png"')
                string name = string.Format("{0}/{1:D04}.png", folder, Time.frameCount);

                // Capture the screenshot to the specified file.
                StartCoroutine(captureScreenshot(name, imageWidth, imageHeight));
                stop();
            }
        }

        else if(state == SimulationState.FINAL_UNSTABLE)
        {
            if (isSceneStable())
            {
                state = SimulationState.FINAL_STABLE;
            }
        }
        else if(state == SimulationState.FINAL_STABLE)
        {
            if (sceneStateJSON != null)
            {
                SimulationSceneState.setState(sceneStateJSON, gameObjectTemplates);
                state = SimulationState.INITIAL_STABLE;
                gameObjectTemplates[currentRemovedObjectIndex].SetActive(true);
            }
            currentRemovedObjectIndex++;
        }
        */
    }

    protected virtual void CreateSceneFromJSON()
    {
        string jsonFileName = string.Format("{0}/InitialStable.json", simulationFolder);
        string sceneJSON = File.ReadAllText(jsonFileName);
        createdSimulationObjects = SimulationSceneState.fromJSON(sceneJSON, gameObjectTemplates, controllerState.removedObjectIndex);
    }

    protected virtual void AddRandomSimulationObject()
    {
        int templateIndex = 0;

        /*
        if (controllerState.noObjects - noObjects < controllerState.initialBigObjects)
        {
            templateIndex = Random.Range(0, numberOfDistinctObjectUsed / 2);
            templateIndex = templateIndex * 2 + 1;
        }
        else
        {
            templateIndex = Random.Range(0, numberOfDistinctObjectUsed);
        }
        */

        if (controllerState.noObjects - noObjects < controllerState.initialBigObjects)
        {
            templateIndex = 3;
        }
        else
        {
            templateIndex = Random.Range(2, numberOfDistinctObjectUsed);
        }

        GameObject refObject = gameObjectTemplates[templateIndex];
        GameObject obj = Object.Instantiate(refObject);
        obj.SetActive(true);

        float x_pos = Random.Range(controllerState.throwMinX, controllerState.throwMaxX);
        float y_pos = Random.Range(controllerState.throwMinY, controllerState.throwMaxY);
        float z_pos = Random.Range(controllerState.throwMinZ, controllerState.throwMaxZ);

        obj.GetComponent<Rigidbody>().position = new Vector3(x_pos, y_pos, z_pos);

        SimulationObjectState objState = new SimulationObjectState();
        objState.material = (templateIndex < 2) ? 0 : 1;
        objState.size = (templateIndex % 2 == 0) ? 0 : 1;

        int colorIndex = Random.Range(0, numberOfDistinctColorsUsed);
        SimulationColor col = new SimulationColor((SimulationColor.TYPE)colorIndex);
        Material newMat = Instantiate(refObject.GetComponent<Renderer>().material);
        newMat.SetColor("_BaseColor", col.GetColor((SimulationMaterial.TYPE)objState.material));
        obj.GetComponent<Renderer>().material = newMat;


        objState.SetGameObject(obj);
        objState.color = colorIndex;
        objState.templateIndex = templateIndex;

        createdSimulationObjects.Add(objState);
    }

    protected virtual void WriteSceneToJSON(string filePath)
    {
        string sceneJSON = SimulationSceneState.toJSON(createdSimulationObjects);

        StreamWriter sw = File.CreateText(filePath); // if file doesnt exist, make the file in the specified path
        sw.Close();

        File.WriteAllText(filePath, sceneJSON); // fill the file with the data(json)
    }
}