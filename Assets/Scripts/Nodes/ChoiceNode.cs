using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

namespace VNEngine
{
    // Should we use stats to decide what this choice button does?
    public enum Choice_Stat_Requirement { No_Requirement, Has_Requirements };

    // If the stat requirement is not met, should we hide the button, or just disable it?
    public enum Requirement_Not_Met_Action { Hide_Choice, Disable_Button };

    public enum Float_Stat_Comparator { Greater_than, Less_than };

    public enum Choice_Boolean_Logic { Done, And, Or };

    public enum Choice_Condition { Bool_Stat_Requirement, Float_Stat_Requirement, String_Stat_Requirement, Object_Is_Null };

    public enum Select_Default_Choice { None, Top_Choice, Bottom_Choice };

    // Displays the choices outlined. Does not continue to the next node.
    // Each choice leads to a prescribed conversation.
    public class ChoiceNode : Node
    {
        public Dictionary<int, string> originalOrder = new Dictionary<int, string>();
        public List<int> randomizedOrder = new List<int>();
        // DO NOT CHANGE: is the maximum number of choices. Dictated by the number of ChoiceButtons listed in the UIManager. You shouldn't ever need more than 20 buttons.
        public static int max_number_of_buttons = 6;
        
        [HideInInspector]
        public string Name_Of_Choice;   // Text that appears at the top of the choices menu. Ex: I'm at a crossroads. Which way should I go?
        [HideInInspector]
        public int Number_Of_Choices = 6;
        [HideInInspector]
        public bool Hide_Dialogue_UI = true;
        [HideInInspector]
        public bool randomize_choices_order = false;        // If set to true, the choices are presented in a random order (and put back in order after the node is finished)
        [HideInInspector]
        public bool Localize_Choice_Text = false;       // If set to true, the name of choices and the text is set via localized text from .csv's put in the scenemanager

        // Arrays of values used for the buttons. Makes the code able to loop through requirements
        [HideInInspector]
        public string[] Button_Text = new string[max_number_of_buttons];
        [HideInInspector]
        public Choice_Stat_Requirement[] Has_Requirements = new Choice_Stat_Requirement[max_number_of_buttons];
        [HideInInspector]
        public Requirement_Not_Met_Action[] Requirement_Not_Met_Actions = new Requirement_Not_Met_Action[max_number_of_buttons];
        [HideInInspector]
        public string[] Disabled_Text = new string[max_number_of_buttons];
        [HideInInspector]
        public bool[] Show_Choice_Was_Selected_Before = new bool[max_number_of_buttons];
        [HideInInspector]
        public bool[] Choice_Been_Clicked_Before = new bool[max_number_of_buttons];
        [HideInInspector]
        public Select_Default_Choice default_selection = Select_Default_Choice.Top_Choice;



        // We can't serialize multidimensional arrays, so simply make a single array indexed by index = y * gridWidth + x
        [HideInInspector]
        public string[] Stat_Name = new string[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public Choice_Condition[] Requirement_Type = new Choice_Condition[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public Choice_Boolean_Logic[] Logic = new Choice_Boolean_Logic[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public Float_Stat_Comparator[] Float_Stat_Is = new Float_Stat_Comparator[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public float[] Float_Compare_Value = new float[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public bool[] Bool_Compare_Value = new bool[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public string[] String_Compare_Value = new string[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public Result[] String_Is = new Result[max_number_of_buttons * max_number_of_buttons];
        [HideInInspector]
        public GameObject[] Check_Null_Object = new GameObject[max_number_of_buttons * max_number_of_buttons];


        // The events associated with the buttons
        public Button.ButtonClickedEvent[] Button_Events = new Button.ButtonClickedEvent[max_number_of_buttons];

        [HideInInspector]
        public bool use_image_native_size_for_buttons = false;
        public Sprite[] choice_button_images = new Sprite[max_number_of_buttons];       // Set these to make each button have their own unique image
        Sprite default_button_sprite;      // Save the initial sprite so we can return to the default image after we're done
        float default_button_flexible_height;

        public void Start()
        {
            //            Debug.Log(UIManager.ui_manager.choice_buttons.Length);

            //           default_button_flexible_height = UIManager.ui_manager.choice_buttons[0].GetComponent<LayoutElement>().preferredHeight;
            default_selection = Select_Default_Choice.Top_Choice;
    }


    public override void Run_Node()
        {
            StartCoroutine(Running());
        }
        public IEnumerator Running()
        {
            // Wait a frame so we can evaluate if objects we check for requirements have been destroyed
            yield return 0;

            // Display the choices on the UI
            UIManager.ui_manager.choice_panel.SetActive(true);
            default_button_sprite = UIManager.ui_manager.choice_buttons[0].image.sprite;

            if (Localize_Choice_Text)
                UIManager.ui_manager.choice_text_banner.text = VNSceneManager.scene_manager.Get_Localized_Dialogue_Entry(Name_Of_Choice);  // Localize the name of the choice;
            else
                UIManager.ui_manager.choice_text_banner.text = Name_Of_Choice;

            if (Hide_Dialogue_UI)
                VNSceneManager.scene_manager.Show_UI(!Hide_Dialogue_UI);

            // Loop through each button
            // Make buttons that have events visible, set their text,
            // add call to Finish_Node() on the OnClick() listener and hook up the choices buttons to the events on this node
            for (int x = 0; x < Number_Of_Choices; x++)
            {
                originalOrder[x] = Button_Text[x];
                if (Button_Events[x].GetPersistentEventCount() > 0)
                {
                    // Set a button image for this button if we have one
                    if (x < choice_button_images.Length && choice_button_images[x] != null)
                    {
                        UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().sprite = choice_button_images[x];

                        if (use_image_native_size_for_buttons)
                        {
                            UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().preserveAspect = true;
                            UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().type = Image.Type.Simple;
                            UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().SetNativeSize();
                            UIManager.ui_manager.choice_buttons[x].GetComponent<LayoutElement>().preferredHeight = -1;
                        }
                    }
                    // No image to set, use reset it to the default one
                    else
                    {
                        UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().sprite = default_button_sprite;
                        UIManager.ui_manager.choice_buttons[x].GetComponent<LayoutElement>().preferredHeight = default_button_flexible_height;
                        UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().preserveAspect = false;
                        //UIManager.ui_manager.choice_buttons[x].GetComponent<Image>().SetNativeSize();
                    }

                    UIManager.ui_manager.choice_buttons[x].gameObject.SetActive(true);    // Make visible
                    UIManager.ui_manager.choice_buttons[x].interactable = true;
                    bool requirement_met = true;


                    if (Localize_Choice_Text)
                        UIManager.ui_manager.choice_buttons[x].GetComponentInChildren<Text>().text = VNSceneManager.scene_manager.Get_Localized_Dialogue_Entry(Button_Text[x]);   // Set button text, get localized version
                    else
                        UIManager.ui_manager.choice_buttons[x].GetComponentInChildren<Text>().text = Insert_Stats_into_Text(Button_Text[x]);   // Set button text, get localized version

                    //(Choice_Been_Clicked_Before[x] ||
                    if (Show_Choice_Was_Selected_Before[x] && StatsManager.Compare_Bool_Stat_To(Name_Of_Choice + ": " + Button_Text[x], true))
                        UIManager.ui_manager.choice_buttons[x].GetComponentsInChildren<Image>(true)[1].enabled = true;
                    else
                        UIManager.ui_manager.choice_buttons[x].GetComponentsInChildren<Image>(true)[1].enabled = false;


                    if (Has_Requirements[x] != Choice_Stat_Requirement.No_Requirement)
                    {
                        // Loop through each requirement
                        for (int req = 0; req < max_number_of_buttons; req++)
                        {
                            bool this_requirement_met = true;

                            // No more requirements
                            if (req > 0 && Logic[x * max_number_of_buttons + (req - 1)] == Choice_Boolean_Logic.Done)
                                break;

                            // Check stat requirements
                            switch (Requirement_Type[x * max_number_of_buttons + req])
                            {
                                case Choice_Condition.Float_Stat_Requirement:
                                    this_requirement_met = StatsManager.Compare_Float_Stat(Stat_Name[x * max_number_of_buttons + req], Float_Stat_Is[x * max_number_of_buttons + req], Float_Compare_Value[x * max_number_of_buttons + req]);
                                    break;


                                case Choice_Condition.Bool_Stat_Requirement:
                                    this_requirement_met = StatsManager.Compare_Bool_Stat_To(Stat_Name[x * max_number_of_buttons + req], Bool_Compare_Value[x * max_number_of_buttons + req]);
                                    break;


                                case Choice_Condition.String_Stat_Requirement:
                                    bool the_same = true;
                                    switch (String_Is[x * max_number_of_buttons + req])
                                    {
                                        case Result.Is:
                                            the_same = true;
                                            break;
                                        case Result.Is_Not:
                                            the_same = false;
                                            break;
                                    }
                                    this_requirement_met = StatsManager.Compare_String_Stat_To(Stat_Name[x * max_number_of_buttons + req], String_Compare_Value[x * max_number_of_buttons + req], the_same);
                                    break;


                                case Choice_Condition.Object_Is_Null:
                                    // Check if object exists
                                    // If the object doesn't exist, and the box is checked
                                    // OR  the object exists and the box is not checked
                                    this_requirement_met = Check_Null_Object[x * max_number_of_buttons + req] && !Bool_Compare_Value[x * max_number_of_buttons + req];
                                    break;
                            }

                            // Compare this requirement with previous requirement
                            if (req > 0)
                            {
                                switch (Logic[x * max_number_of_buttons + (req - 1)])
                                {
                                    case Choice_Boolean_Logic.And:
                                        requirement_met = requirement_met && this_requirement_met;
                                        break;
                                    case Choice_Boolean_Logic.Or:
                                        requirement_met = requirement_met || this_requirement_met;
                                        break;
                                }
                            }
                            else
                                requirement_met = this_requirement_met; // First requirement uses this requirement
                        }
                    }

                    // Stat requirements have been met. Display the choice
                    if (requirement_met)
                    {
                        AddListeners(x);
                    }
                    else
                    {
                        switch (Requirement_Not_Met_Actions[x])
                        {
                            case Requirement_Not_Met_Action.Disable_Button:
                                UIManager.ui_manager.choice_buttons[x].interactable = false;
                                UIManager.ui_manager.choice_buttons[x].GetComponentInChildren<Text>().text = Disabled_Text[x];   // Set button text
                                break;
                            case Requirement_Not_Met_Action.Hide_Choice:
                                UIManager.ui_manager.choice_buttons[x].gameObject.SetActive(false);    // Make inivisible
                                break;
                        }
                    }
                }
                else
                {
                    UIManager.ui_manager.choice_buttons[x].gameObject.SetActive(false);
                }
            }

            // Disable all other buttons
            for (int x = Number_Of_Choices; x < max_number_of_buttons; x++)
            {
                UIManager.ui_manager.choice_buttons[x].gameObject.SetActive(false);
            }

            if (randomize_choices_order)
                RandomizeButtonOrder();

//SET DEFAULT
            // Set which choice should be select by default
            if (default_selection == Select_Default_Choice.Top_Choice
                || default_selection == Select_Default_Choice.Bottom_Choice)
            {
                int lowest_sibling = 0;
                int topmost_sibling = 999;
                Button topmost_choice = null;
                Button lowest_choice = null;

                // Loop through each choice, finding the top and bottom
                foreach (Button b in UIManager.ui_manager.choice_buttons)
                {
                    if (b.IsActive() && b.interactable)
                    {
                        int index = b.transform.GetSiblingIndex();
                        if (index >= lowest_sibling)
                        {
                            lowest_choice = b;
                            lowest_sibling = index;
                        }
                        if (index <= topmost_sibling)
                        {
                            topmost_choice = b;
                            topmost_sibling = index;
                        }
                    }
                }


                if (default_selection == Select_Default_Choice.Top_Choice 
                    && topmost_choice != null)
                    SelectChoice(topmost_choice.gameObject);
                else if (default_selection == Select_Default_Choice.Bottom_Choice
                    && lowest_choice != null)
                    SelectChoice(lowest_choice.gameObject);
            }
            if(GetComponent<TimedChoiceNode>())
            {
                GetComponent<TimedChoiceNode>().Run_Node();
            }
            // Gather active buttons
            List<Button> activeButtons = new List<Button>();
            foreach (Button b in UIManager.ui_manager.choice_buttons)
            {
                if (b.gameObject.activeInHierarchy)
                {
                    activeButtons.Add(b);
                }
            }

// Animate them
            UIManager.ui_manager.AnimateChoiceButtons(activeButtons);
            if (activeButtons.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(activeButtons[0].gameObject);
            }

        }


        public void SelectChoice(GameObject choice)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("Setting Choice to " + choice.name);
            EventSystem.current.SetSelectedGameObject(choice);
        }

        public string Insert_Stats_into_Text(string in_text)
        {
            // Find any [ ] characters
            string[] splits = in_text.Split('[', ']');

            // Now check each split if it's legitimate
            foreach (string original_s in splits)
            {
                bool is_variable = false;
                string new_s = "";
                string modified_s;
                if (original_s.StartsWith("b:"))
                {
                    is_variable = true;
                    modified_s = original_s.Replace("b:", "");
                    new_s = StatsManager.Get_Boolean_Stat(modified_s).ToString();
                }
                else if (original_s.StartsWith("f:"))
                {
                    is_variable = true;
                    modified_s = original_s.Replace("f:", "");
                    new_s = StatsManager.Get_Numbered_Stat(modified_s) + "";
                }
                else if (original_s.StartsWith("s:"))
                {
                    is_variable = true;
                    modified_s = original_s.Replace("s:", "");
                    new_s = StatsManager.Get_String_Stat(modified_s);
                }

                if (is_variable)
                    in_text = in_text.Replace("[" + original_s + "]", new_s);
            }

            return in_text;
        }


        public void RandomizeButtonOrder()
        
        {
            Debug.Log("Randomizing choices button order");
            randomizedOrder.Clear();
            for (int x = 0; x < Number_Of_Choices; x++)
            {
                randomizedOrder.Add(x);
            }

            for (int x = 0; x < Number_Of_Choices; x++)
            {
                if (UIManager.ui_manager.choice_buttons[x].gameObject.activeSelf)
                {                  
                    int randomIndex = UnityEngine.Random.Range(0, randomizedOrder.Count);
                    int choiceIndex = randomizedOrder[randomIndex];
                    UIManager.ui_manager.choice_buttons[x].transform.SetSiblingIndex(randomIndex);

                }
            }
            Debug.Log("Randomized order: " + string.Join(", ", randomizedOrder));
        }
        public void UnrandomizeButtonOrder()
        {
            for (int x = 0; x < max_number_of_buttons; x++)
            {
                UIManager.ui_manager.choice_buttons[x].transform.SetSiblingIndex(x + 1);
            }
            Debug.Log("Unrandomizing choices button order");
        }


        public void AddListeners(int choice_number)
        {
            Button_Events[choice_number].AddListener(delegate { Choice_Clicked(choice_number); });
            Button_Events[choice_number].AddListener(Clear_Choices); // Add call to finish this node and hide UI to event listener
            UIManager.ui_manager.choice_buttons[choice_number].onClick = Button_Events[choice_number]; // Set events
        }


        public override void Button_Pressed()
        {

        }


        public void Clear_Choices()
        {
            Debug.Log("Clearing Choices");

            // Check VNSceneManager
            if (VNSceneManager.current_conversation == null)
            {
                Debug.LogError("VNSceneManager.current_conversation is null.");
                return;
            }

            if (VNSceneManager.current_conversation.Get_Current_Node() == null)
            {
                Debug.LogError("VNSceneManager.current_conversation.Get_Current_Node() returned null.");
                return;
            }

            // Check UIManager
            if (UIManager.ui_manager == null)
            {
                Debug.LogError("UIManager.ui_manager is null.");
                return;
            }

            // Check choice_buttons array
            if (UIManager.ui_manager.choice_buttons == null)
            {
                Debug.LogError("UIManager.ui_manager.choice_buttons is null.");
                return;
            }

            // Ensure choice_buttons array has the expected size
            if (UIManager.ui_manager.choice_buttons.Length < ChoiceNode.max_number_of_buttons)
            {
                Debug.LogError($"UIManager.ui_manager.choice_buttons array size is too small. Expected {ChoiceNode.max_number_of_buttons}, got {UIManager.ui_manager.choice_buttons.Length}.");
                return;
            }

            if (VNSceneManager.current_conversation.Get_Current_Node().GetType() != this.GetType()) // Don't clear the choices if the next node is a Choice node
            {
                // Loop through every button
                // Check each button in the array
                for (int x = 0; x < ChoiceNode.max_number_of_buttons; x++)
                {
                    if (UIManager.ui_manager.choice_buttons[x] == null)
                    {
                        Debug.LogError($"UIManager.ui_manager.choice_buttons[{x}] is null.");
                        continue;
                    }

                    // Remove event listeners from buttons
                    UIManager.ui_manager.choice_buttons[x].onClick.RemoveAllListeners();

                    // Set all choice buttons to inactive
                    UIManager.ui_manager.choice_buttons[x].gameObject.SetActive(false);
                }

                // Check choice_panel
                if (UIManager.ui_manager.choice_panel == null)
                {
                    Debug.LogError("UIManager.ui_manager.choice_panel is null.");
                    return;
                }

                // Hide choice UI
                UIManager.ui_manager.choice_panel.SetActive(false);
            }
            else
            {
                Debug.Log("Timed Node Choice");
                for (int x = 0; x < ChoiceNode.max_number_of_buttons; x++)
                {
                    // Remove event listeners from buttons
                    UIManager.ui_manager.choice_buttons[x].onClick.RemoveAllListeners();
                    // Set all choice buttons to inactive
                    UIManager.ui_manager.choice_buttons[x].gameObject.SetActive(false);
                }
                // Hide choice UI
                UIManager.ui_manager.choice_panel.SetActive(false);

            }
        }


        // Called when the language dropdown changes
        public void LanguageChanged()
        {
            UIManager.ui_manager.choice_text_banner.text = VNSceneManager.scene_manager.Get_Localized_Dialogue_Entry(Name_Of_Choice);  // Localize the name of the choice

            for (int x = 0; x < Number_Of_Choices; x++)
            {
                if (Button_Events[x].GetPersistentEventCount() > 0)
                {
                    UIManager.ui_manager.choice_buttons[x].GetComponentInChildren<Text>().text = VNSceneManager.scene_manager.Get_Localized_Dialogue_Entry(Button_Text[x]);   // Set button text, get localized version
                }
            }
        }


        // Record this specific choice has been clicked before
        public void Choice_Clicked(int choice_number)
        {
            try
            {
                Debug.Log($"Choice_Clicked called with choice_number: {choice_number}");

                if (randomize_choices_order)
                {
                    Debug.Log("Unrandomizing button order.");
                    UnrandomizeButtonOrder();
                }

                if (Choice_Been_Clicked_Before == null || Choice_Been_Clicked_Before.Length <= choice_number)
                {
                    Debug.LogError($"Choice_Been_Clicked_Before array is null or out of bounds. Length: {Choice_Been_Clicked_Before?.Length}, choice_number: {choice_number}");
                    return;
                }

                Choice_Been_Clicked_Before[choice_number] = true;

                if (Button_Text == null || Button_Text.Length <= choice_number)
                {
                    Debug.LogError($"Button_Text array is null or out of bounds. Length: {Button_Text?.Length}, choice_number: {choice_number}");
                    return;
                }

                StatsManager.Set_Boolean_Stat(Name_Of_Choice + ": " + Button_Text[choice_number], true);

                if (originalOrder == null || randomizedOrder == null)
                {
                    Debug.LogError($"originalOrder or randomizedOrder is null. originalOrder: {originalOrder}, randomizedOrder: {randomizedOrder}");
                    return;
                }
                Debug.Log("ORDER: " + originalOrder.ToString());
                Debug.Log("RANDOM ORDER: " + randomizedOrder.ToString());
                //OPEN GAME DATA LOGGING IS CURRENTLY OFF. UNCOMMENT TO ADD.
                if (Logging.Instance != null)
                {
                    // Logging.Instance.LogPlayerChoice(Name_Of_Choice, choice_number, originalOrder, randomizedOrder);

                }


            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred in Choice_Clicked: {ex.Message}\n{ex.StackTrace}");
            }
        }


        public override void Finish_Node()
        {
            Clear_Choices();        // Hide the UI
            base.Finish_Node();     // Continue conversation
        }
    }
}