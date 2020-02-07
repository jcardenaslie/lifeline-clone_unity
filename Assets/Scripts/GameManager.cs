using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Xml;

[System.Serializable]
public class Path {
    public string pathName;
    public List<string> dialogs;
    public List<string> decisions;

    //public List<Path> nextPaths;

    public string NextDialog(int index) {
        string dialog = dialogs[index];
        return dialog;
    }
}

public class GameManager : MonoBehaviour {

    public GameObject m_startButton;
    public GameObject m_scrollPanel;
    public GameObject m_verticalGrid;
    public GameObject m_dialogPanelPrefab;
    public GameObject m_choicesPanelPrefab;
    public float m_timeBetweenDialogs = 1f;


    private List<Path> m_story = new List<Path>();
    private Path m_currentPath;
    private int m_currentPathIndex = 0;
    private string m_currentPathName;


    private List<string> m_dialogs = new List<string>();
    private int m_currentDialogIndex = 0;
    private string m_currentDialog;

    private List<string> m_choices = new List<string>();
    private string m_currentChoices;

    private bool m_waitReaction = false;
    private float m_timePassed = 0;
    private int m_reactionPathIndex = 0;

    private Button[] m_lastChoiceButtons = new Button[2];

    public bool playing { get; private set; }

    // Use this for initialization
    void Start () {
        LoadStory();
        //m_currentPathName = m_story[m_currentPathIndex].pathName;
        playing = false;
        SetPathNDialogs();
    }

    private void SetPathNDialogs() {
        m_currentPathIndex = 0;
        m_currentDialogIndex = 0;

        m_currentPath = m_story[m_currentPathIndex];
        m_dialogs = m_currentPath.dialogs;
        m_currentDialog = m_dialogs[m_currentDialogIndex];
    }
    // Update is called once per frame
    void Update() {

        if (playing && (m_timePassed >= m_timeBetweenDialogs))
        {
            int tmp_maxDialogs = m_dialogs.Count - 1;
            if (m_currentPath != null)
            {
                //Dialog
                if (m_currentDialog != null && !m_waitReaction)
                {
                    GameObject tmp_dialogPanel = CreateDialogPanel();
                    tmp_dialogPanel.transform.SetParent(m_verticalGrid.transform);
                    m_currentDialog = GetNextDialog();
                    IncreaseVertcialGridHeight(100);
                    FocusBottom();
                }
                // Choice Path Buttons
                else if(!m_waitReaction)
                {
                    if (m_currentPath.decisions.Count > 0)
                    {
                        m_currentDialogIndex = 0;
                        m_waitReaction = true;
                        GameObject tmp_choicePanel = CreateChoicePanel();
                        tmp_choicePanel.transform.SetParent(m_verticalGrid.transform);
                    }
                    else {
                        //Remplazar por un panel
                        Debug.Log("No more story to tell");
                        playing = false;
                    }

                    IncreaseVertcialGridHeight(100);
                    FocusBottom();
                }
            }
            //Debug.Log(m_scrollPanel.GetComponent<ScrollRect>().verticalNormalizedPosition);
            m_timePassed = 0.0f;
        }
        m_timePassed += Time.deltaTime;
	}

    void FocusBottom()
    {
        m_scrollPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }

    void IncreaseVertcialGridHeight(int increaseValue) {
        RectTransform gridTransform = (RectTransform)m_verticalGrid.transform;
        float spacing = m_verticalGrid.GetComponent<VerticalLayoutGroup>().spacing;
        float width = gridTransform.rect.width;
        float height = gridTransform.rect.height;

        gridTransform.sizeDelta = new Vector2(width, height + increaseValue + spacing);
    }

    public void StartGame() {
        m_startButton.SetActive(false);
        Debug.Log("Start Game");
        playing = true;
    }

    void LoadStory() {
        XmlDocument levelDoc = new XmlDocument();
        XmlNodeList LevelList;

        TextAsset xmlFile = Resources.Load("StoryData", typeof(TextAsset)) as TextAsset;
        levelDoc.LoadXml(xmlFile.text);
        LevelList = levelDoc.GetElementsByTagName("path");
        foreach (XmlNode path in LevelList) {
            Path newPath = new Path();
            newPath.decisions = new List<string>();
            newPath.dialogs = new List<string>();
            //Debug.Log("Path name: " + path.Name + path.Attributes[0].Value);
            newPath.pathName = path.Attributes[0].Value;
            foreach (XmlNode child in path)
            {
                if (child.Name.Equals("dialogs")) {
                    foreach (XmlNode dialog in child) {
                        //Debug.Log("dialog: " + dialog.InnerText);
                        newPath.dialogs.Add(dialog.InnerText);
                    }
                }
                else if(child.Name.Equals("nextpaths")){
                    foreach (XmlNode nextpath in child)
                    {
                        //Debug.Log("nextpath: " + nextpath.InnerText);
                        newPath.decisions.Add(nextpath.InnerText);
                    }
                }
            }
            m_story.Add(newPath);
        }
    }

    private void SetNewPath(int index) {
        m_currentPath = GetNextPath(index);
        m_dialogs = m_currentPath.dialogs;
        m_currentDialog = m_dialogs[0];
        m_choices = m_currentPath.decisions;
    }

    // DEJAR COMO UNA FUNCION QUE RECIBE PARAMETRO
    public void DesicionLeft() {
        string tmp_decision =m_currentPath.decisions[0];
        foreach (Path tmp_path in m_story) {
            if (tmp_path.pathName.Equals(tmp_decision)) {
                int tmp_nextPathIndex = m_story.IndexOf(tmp_path);
                SetNewPath(tmp_nextPathIndex);
            }
        }
        m_waitReaction = false;
    }

    public void DesicionRight() {
        string tmp_decision = m_currentPath.decisions[1];
        foreach (Path tmp_path in m_story)
        {
            if (tmp_path.pathName.Equals(tmp_decision))
            {
                int tmp_nextPathIndex = m_story.IndexOf(tmp_path);
                SetNewPath(tmp_nextPathIndex);
            }
        }
        m_waitReaction = false;
    }


    private GameObject CreateDialogPanel() {

        GameObject tmp_dialogPanel = (GameObject)
                        Instantiate(m_dialogPanelPrefab, m_verticalGrid.transform.position, m_verticalGrid.transform.rotation);

        Text tmp_text = tmp_dialogPanel.transform.GetChild(0).GetComponent<Text>();
        tmp_text.text = m_currentDialog;

        return tmp_dialogPanel;
    }

    private GameObject CreateChoicePanel() {
        GameObject tmp_choicePanel = (GameObject)
                        Instantiate(m_choicesPanelPrefab, m_verticalGrid.transform.position, m_verticalGrid.transform.rotation);
        tmp_choicePanel.SetActive(true);

        m_lastChoiceButtons[0] = tmp_choicePanel.transform.GetChild(0).GetComponent<Button>();
        m_lastChoiceButtons[1] = tmp_choicePanel.transform.GetChild(1).GetComponent<Button>();

        m_lastChoiceButtons[0].onClick.AddListener(() => DesicionLeft());
        m_lastChoiceButtons[1].onClick.AddListener(() => DesicionRight());

        if (m_story[m_currentPathIndex].decisions[0] != "" && m_story[m_currentPathIndex].decisions[1] != "")
        {
            Text tmp_choiceButtonText = m_lastChoiceButtons[0].transform.GetChild(0).GetComponent<Text>();
            tmp_choiceButtonText.text = m_currentPath.decisions[0];

            tmp_choiceButtonText = m_lastChoiceButtons[1].transform.GetChild(0).GetComponent<Text>();
            tmp_choiceButtonText.text = m_currentPath.decisions[1];
        }
        else
        {
            Debug.Log("error");
        }
        return tmp_choicePanel;
    }

    private Path GetNextPath(int index) {

        Path tmp_path = m_story[index];

        if (tmp_path != null)
        {
            m_dialogs = tmp_path.dialogs;
            return tmp_path;
        }
        else {
            return null;
        }
    }

    private string GetNextDialog()
    {
        int tmp_dialogIndex = m_dialogs.IndexOf(m_currentDialog);
        int tmp_dialogsCunt = m_dialogs.Count - 1;
        if (tmp_dialogIndex < tmp_dialogsCunt)
        {
            Debug.Log(m_dialogs[tmp_dialogIndex + 1]);
            return m_dialogs[tmp_dialogIndex + 1];
        }
        else
        {
            return null;
        }
    }
}
