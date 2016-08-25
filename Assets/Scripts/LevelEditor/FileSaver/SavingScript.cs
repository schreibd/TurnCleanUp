﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class SavingScript : MonoBehaviour {

    LevelConfiguration levelConfig;
    private BinaryWriter m_writer;
    List<GameObject> savedGameObjects = new List<GameObject>();

    public void saveLevel(string path)
    {
        levelConfig = LevelConfiguration.instance;
        if(!path.EndsWith(Constants.FILE_EXTENSION))
        {
            path += Constants.FILE_EXTENSION;
        }


        m_writer = new BinaryWriter(new FileStream(path, FileMode.Create));
        savedGameObjects.Clear();
        levelConfig.objectCount = 0;


        foreach(Transform t in FindObjectsOfType<Transform>())
        {
            //Zu speichernde Elemente müssen "Wurzel"-Element sein
            //innere Kinder, werden ignoriert. Kinder werden unabhängig vom Tag
            //gespeichert, und wie Komponenten behandelt
            if (t.parent != null)
                continue;

            bool correctTag = false;
            foreach(string tag in Constants.FILE_LEVEL_ITEM_TAGS) {
                if(t.tag.Equals(tag)) {
                    correctTag = true;
                    break;
                }
            }

            if(correctTag) {
                    levelConfig.objectCount++;
                    savedGameObjects.Add(t.gameObject);
            }
        }
        writeHeader();

        foreach(GameObject currentGameObject in savedGameObjects)
        {
            //Flag, dass ein neues Objekt kommt
            m_writer.Write((short)Constants.FILE_OBJECT_FLAGS.NewObject);
            writeObject(currentGameObject);
        }
        //Flag dass Ende des Files erreicht ist
        m_writer.Write((short)Constants.FILE_OBJECT_FLAGS.EndOfFile);
        Debug.Log("Close");
        m_writer.Close();
    }

    public void writeHeader()
    {
        m_writer.Write(Constants.FILE_BEGINNING_TAG);
        m_writer.Write(levelConfig.defaultValues);
        //Default Values benutzt
        if (levelConfig.defaultValues)
            return;

        m_writer.Write(levelConfig.gridWidth);
        m_writer.Write(levelConfig.gridHeight);
        m_writer.Write(levelConfig.objectCount);
        m_writer.Write(LookUpTable.materialsInverse[levelConfig.cubeMaterial]);
        
    }

    public void writeObject(GameObject gameObject)
    {
        writeObjectHeader(gameObject);
        writeTransform(gameObject.transform);

        //Laufe alle möglichen Components durch, speichere sie, wenn keine Components mehr vorhanden sind, setze EndOfObjectFlag
        for(int i = 1; i < (int)Constants.FILE_COMPONENT_FLAGS.Count; ++i) {
            //TODO: Komponenten ausdefinieren
            Constants.FILE_COMPONENT_FLAGS componentFlag = (Constants.FILE_COMPONENT_FLAGS) i;
            switch (componentFlag) {
                case Constants.FILE_COMPONENT_FLAGS.ObjectComponent: {
                        ObjectComponent currentObjectComponent = gameObject.GetComponent<ObjectComponent>();
                        if (!currentObjectComponent.Equals(null)) {
                            m_writer.Write((int)componentFlag);
                            writeObjectComponent(currentObjectComponent);
                        }
                        break;
                    }

                case Constants.FILE_COMPONENT_FLAGS.ObjectSetter: {
                        ObjectSetter currentObjectSetter = gameObject.GetComponent<ObjectSetter>();
                        if (!currentObjectSetter.Equals(null)) {
                            m_writer.Write((int)componentFlag);
                            writeObjectSetter(currentObjectSetter);
                        }
                        break;
                    }


                default:
                    continue;
            }
        }

        /*
        //Überprüfen ob GameObjekt Kindobjekte hat, sollte als letztes passieren
        for(int i = 0; i < gameObject.transform.childCount; ++i) {
            GameObject childObject = gameObject.transform.GetChild(i).gameObject;
            m_writer.Write((int)Constants.FILE_COMPONENT_FLAGS.ChildObject);
            writeObject(childObject);
        }
        */
        m_writer.Write((int)Constants.FILE_COMPONENT_FLAGS.EndOfObject);
    }

    public void writeObjectHeader(GameObject gameObject)
    {
        ObjectComponent objComp = gameObject.GetComponent<ObjectComponent>();
        if (!objComp.Equals(null)) {
            if (!objComp.original.Equals(null)) {
                m_writer.Write(LookUpTable.prefabsInverse[objComp.original]);
            }
            else
                m_writer.Write(Constants.FILE_NO_PREFAB_TAG);
        }
        m_writer.Write(gameObject.name);
        m_writer.Write(gameObject.tag);
    }

    public void writeTransform(Transform transform)
    {
        m_writer.Write(transform.position.x);
        m_writer.Write(transform.position.y);
        m_writer.Write(transform.position.z);
        m_writer.Write(transform.rotation.x);
        m_writer.Write(transform.rotation.y);
        m_writer.Write(transform.rotation.z);
        m_writer.Write(transform.rotation.w);
        m_writer.Write(transform.localScale.x);
        m_writer.Write(transform.localScale.y);
        m_writer.Write(transform.localScale.z);
    }
    public void writeObjectSetter(ObjectSetter component)
    {
        m_writer.Write(component.x);
        m_writer.Write(component.z);
    }

    public void writeObjectComponent(ObjectComponent component)
    {
        m_writer.Write(component.sizeX);
        m_writer.Write(component.sizeZ);
    }

}
