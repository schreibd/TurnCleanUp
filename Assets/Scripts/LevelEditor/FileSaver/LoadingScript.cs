﻿using UnityEngine;
using System.Collections;
using System.IO;

public class LoadingScript : MonoBehaviour {

    LevelConfiguration levelConfig;
    private BinaryReader m_reader;


    private GameObject m_currentGameObject;
    private Constants.FILE_OBJECT_FLAGS m_currentObjectFlag;
    private Constants.FILE_COMPONENT_FLAGS m_currentComponentFlag;


    public void loadLevel(string path)
    {
        levelConfig = LevelConfiguration.instance;
        try
        {
            m_reader = new BinaryReader(new FileStream(path, FileMode.Open));
        }
        catch (FileNotFoundException ex)
        {
            Debug.Log(ex.Message);
        }
        bool reading = readHeader();
        while (reading)
        {
            try
            {
                m_currentObjectFlag = (Constants.FILE_OBJECT_FLAGS)m_reader.ReadInt16();
                switch(m_currentObjectFlag) { 
                    case Constants.FILE_OBJECT_FLAGS.NewObject: {
                        readObject();
                        break;
                    }
                    case Constants.FILE_OBJECT_FLAGS.EndOfFile: {
                        reading = false;
                        break;
                    }
                }
            }
            catch (EndOfStreamException ex)
            {
                reading = false;
                ex.ToString();
            }
        }
        m_reader.Close();
    }

    public bool readHeader()
    {
        string beginning = m_reader.ReadString();
        if(!beginning.Equals(Constants.FILE_BEGINNING_TAG))
        {
            Debug.Log("Wrong File Start");
            //Falscher File Start
            return false;
        }
        levelConfig.defaultValues = m_reader.ReadBoolean();
        if(levelConfig.defaultValues)
        {
            levelConfig.gridWidth = Constants.DEFAULT_GRID_WIDTH;
            levelConfig.gridHeight = Constants.DEFAULT_GRID_HEIGHT;
            levelConfig.objectCount = Constants.DEFAULT_OBJECT_COUNT;
            levelConfig.cubeMaterial = Constants.DEFAULT_CUBE_MATERIAL;
        }
        else
        {
            levelConfig.gridWidth = m_reader.ReadInt32();
            levelConfig.gridHeight = m_reader.ReadInt32();
            levelConfig.objectCount = m_reader.ReadInt32();
            levelConfig.cubeMaterial = LookUpTable.materials[m_reader.ReadString()];
        }
        return true;
    }

    public void readObjectHeader()
    {
        string prefabTag = m_reader.ReadString();
        if(!prefabTag.Equals(Constants.FILE_NO_PREFAB_TAG)) {
            
        }
        m_currentGameObject.name = m_reader.ReadString();
        m_currentGameObject.tag = m_reader.ReadString();
    }

    public void readObject(GameObject parentObject = null)
    {
        readObjectHeader();
        readTransform(parentObject);
        do
        {
            m_currentComponentFlag = (Constants.FILE_COMPONENT_FLAGS) m_reader.ReadInt32();
            switch (m_currentComponentFlag)
            {
                case Constants.FILE_COMPONENT_FLAGS.ObjectComponent: {
                        readObjectComponent();
                        break;
                    }
                case Constants.FILE_COMPONENT_FLAGS.ObjectSetter: {
                        readObjectSetter();
                        break;
                    }
                /*
                //KindObjekt gefunden, sollte letztes Komponentenflag sein
                case Constants.FILE_COMPONENT_FLAGS.ChildObject: {
                        GameObject parent = m_currentGameObject;
                        m_currentGameObject = new GameObject();
                        readObject(parent);
                        m_currentGameObject = parent;
                        break;
                    }
                 */
                case Constants.FILE_COMPONENT_FLAGS.EndOfObject:
                    break;

                default:
                    break;
            }
        } while (m_currentComponentFlag != Constants.FILE_COMPONENT_FLAGS.EndOfObject);
    }

    public void readTransform(GameObject parentObject = null)
    {
        if (parentObject != null)
            m_currentGameObject.transform.SetParent(parentObject.transform);

        m_currentGameObject.transform.position = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
        m_currentGameObject.transform.rotation = new Quaternion(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
        m_currentGameObject.transform.localScale = new Vector3(m_reader.ReadSingle(), m_reader.ReadSingle(), m_reader.ReadSingle());
    }

    public void readObjectSetter()
    {
        ObjectSetter defaultObjectSetter = m_currentGameObject.GetComponent<ObjectSetter>();
        if(!defaultObjectSetter.Equals(null)) {
            defaultObjectSetter.x = m_reader.ReadInt32();
            defaultObjectSetter.z = m_reader.ReadInt32();
        }
    }

    public void readObjectComponent()
    {
        ObjectComponent defaultObjectComponent = m_currentGameObject.GetComponent<ObjectComponent>();
        if(!defaultObjectComponent.Equals(null)) {
            defaultObjectComponent.sizeX = m_reader.ReadInt32();
            defaultObjectComponent.sizeZ = m_reader.ReadInt32();
        }
    }
}
