using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;  //Use this if WWW is obsolete in Unity version

//This handles commands in the form of:
//OJBECT ACTION PARAMETERS  (noun-verb-modifiers)
public class MyGameManager3 : MonoBehaviour
{
    public string [] CommandFiles; //contains list of Scenario files

    //array of hardcoded commands for testing
    string [] mycommands = {
        "/Room1/Enemy jump",
        "GM Sleep 1",
        "/Room1/Door rotateY 1.2",
        "GM Sleep 1",
        "/RoomLight off",
        "GM Sleep 1",
        "/RoomLight on",
        "Room1/Room scale 5.0,7.5"

    };

    // Start is called before the first frame update
    // This starts the 
    void Start()
    {
       
        StartCoroutine(executeScenarioFiles(CommandFiles));
        //StartCoroutine(ExecuteCommands(commands));

    }

 
    //loads local or remote scenario files
    IEnumerator executeScenarioFiles(string [] fileNames)
    {

        foreach (string fileName in fileNames){
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

            string result;

            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                //WWW www = new WWW(filePath);
                //yield return www;
                //result = www.text;
                UnityWebRequest webRequest = UnityWebRequest.Get(filePath); //use this if WWW is obsolete
                 yield return webRequest.SendWebRequest();
                 if (webRequest.isNetworkError){
                     print("Web request error:"+ webRequest.error + " for " + filePath);
                     continue;
                 }else{
                     result = webRequest.downloadHandler.text;
                 }

            }
            else
            {
                result = System.IO.File.ReadAllText(filePath);
            }

            Debug.Log("Loaded file: " + fileNames);

                   
            string[] linesInFile = result.Split('\n');
            yield return StartCoroutine(ExecuteCommands(linesInFile));
        }
    }

    private GameObject go;
    private int commandNum=0;
    // Update is called once per frame
    enum IFState {False, Condition, Then, Else};
    IFState IFstate = IFState.False;
    bool IFresult = true;

    //Execute each line in scenario file
    //If it starts with a GM, the GameManager handles the command
    //If it doesn't, find the object the command belongs to and send 
    //it the command.
    IEnumerator ExecuteCommands(string [] commands)
    {
        for (int i = 0; i < commands.Length; i++)
        {
            string objName,command,param;
            command=null;
            param=null;
            commandNum=i;

            //Get next command
            string commandLine = commands[commandNum];
            commandLine = commandLine.Trim(); //remove extra newlines or whitespace at beginning and end
            print("GM:Execute Command["+ commandNum+ "] = " + commandLine);
    
            //split up command into parts (OBJ, ACTION, PARAMS)
            string [] splitArray = commandLine.Split(' ');
            objName = splitArray[0];
            if (splitArray.Length>1)
                command = splitArray[1];
            if (splitArray.Length>2)
                param = splitArray[2];

            //skip comments and blank lines
            if (objName.Length<1 || objName[0]=='#')  //skip comments
                continue;

            //execute GameManager commands
            if (objName=="GM"){  //special Game Manager command 
                if (IfStateSwitch(command)) //handle IF-THEN-ELSE-ENDIF commands
                    continue;

                //Handle Game Manager commands
                //Add additional commands below.
                if (command == "Sleep"){    //Sleep for a given time in seconds
                    string paramStr = splitArray[2];
                    print("Sleep for "+ paramStr);
                    float delay = float.Parse(paramStr);
                    yield return new WaitForSeconds(delay);
                }
                if (command == "Goto"){     //GOTO a line number in the Scenario file
                    string paramStr = splitArray[2];
                    i= int.Parse(paramStr) -2;  //array starts at 0, and i increments, so must minus 2
                    yield return null;  //make sure we don't get caught in infinite loop which hangs unity
                    continue;
                }



            }else //send commands to other game objects
            {   
                //check if in conditional blocks (THEN or ELSE)
                if (IFstate== IFState.Then && !IFresult)  //skip then if false
                    continue;
                if (IFstate== IFState.Else && IFresult)  //skip else if true
                    continue;

                bool result = processObjectsCommand(objName,command,param);

                //get return value if in conditional statement (last statement was IF)
                if (IFstate == IFState.Condition){
                    IFresult = result;
                    print("Conditional = "+ result);
                }

            }
        }
    }

    //Find the GameObject(s) to send the command to and
    //send the command to each of them.  Pattern matching is through
    //i.e. Room1/Lights/ALL should find:
    // Room1/Lights/Light1 and Room1/Lights/Light2
    bool processObjectsCommand(string objName,string command,string param)
    {
        GameObject[] Objects;
        bool retval;

        go=GameObject.Find(objName);
        if ((objName == "ALL") || (objName == "ANY"))
        {
                print(" Sending command " + command + " to all");
                Objects = FindGameObjects(objName);

                if (objName == "ANY"){
                    //TODO: ADD CODE TO HANDLE "ANY" 
                    retval = true;

                }else{  //handle ALL
                    retval = true;

                    foreach (GameObject gameObj in Objects)
                    {
                        retval = processObjectCommand(gameObj,command,param);
                    }
                }
        }else{  //only call for named GameObject
            GameObject go=GameObject.Find(objName);
            if (!go){
                print("Object " + objName + " not found.");
                retval = false;
            }
            retval = processObjectCommand(go,command,param);
        }
        return retval;
    }

    //Send a GameObject command to appropriate GameObject
    bool processObjectCommand(GameObject go,string command,string param)
    {
        //get GameObject name to send command to    

        print("GM: Sending command " + command + "to " + go.name);

        //get our message handler component from the Game Object
        ObjectHandler mhand = go.GetComponent<ObjectHandler>();
        if (!mhand){
            print("Object" + go.name + " missing message handler.");
            return false;
        }
        //pass command and parameters to message handler
        bool result = mhand.HandleMessage(command,param); //commands return BOOL for IF statements
        return result;
    }
   


    /////////////////////////////////////////////////////////
    //HELPER FUNCTIONS
    /////////////////////////////////////////////////////////

    //keep track of where we are in an IF-THEN-ELSE block
    bool IfStateSwitch(string command)
    {                    
                //Handle IF-THEN-ELSE-ENDIF statements
                //IFState keeps track of where we are in the IF statement
                if (IFstate == IFState.Condition){  //If we are in conditional part (IF)
                    if (command == "Then"){         //IF switches us to next state
                        IFstate = IFState.Then;
                        return true;
                    }
                }
                if (IFstate == IFState.Then){       //If we are in THEN statements
                    if (command == "Else"){         //switch state when ELSE statement
                        IFstate = IFState.Else;
                        return true;
                    }
                }
                if (command == "Endif"){            //Leave IFstate when ENDIF is reached
                        IFstate = IFState.False;
                        return true;
                }
                if (command == "If"){       //IF command
                    IFstate = IFState.Condition;
                        return true;
                }
                
                if (IFstate== IFState.Then && !IFresult)  //skip THEN if conditiional is false
                        return true;
                if (IFstate== IFState.Else && IFresult)  //skip ELSE if conditional is true
                        return true;
        return false;
    }

    private GameObject[] m_gameObjects;

    //
    //Find children objects of objName for commands meant for multiple objects
    //Supports ANY and ALL patterns
    //e.g.: Lights/ALL
    //Returns list of children
    GameObject [] FindGameObjects(string objName)
    {
  
        print("FindGameObjects for "+ objName);
        int start = objName.Length-1;
        int count = objName.Length;
        int at = objName.LastIndexOf("/", start, count);  
        print("LastIndexOf / = "+ at);
        objName = objName.Remove(at);
        print("Multiple Objects under " + objName + "");

        GameObject go=GameObject.Find(objName);  //   /Room1/Enemy/ANY   or /Room/ANY/Enemy/ANY
        m_gameObjects = new GameObject[go.transform.childCount];
        for (int i = 0; i < go.transform.childCount; i++)
        {
            m_gameObjects[i] = go.transform.GetChild(i).gameObject;
        }
        //TODO 
        return m_gameObjects;
//        return null;
    }
 

    void Update()
    {

    }



}
