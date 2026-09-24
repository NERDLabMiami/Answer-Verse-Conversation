// VNAddNodeMenu.cs
// Hierarchy/GameObject context menu: centralizes adding a node to a conversation as native
// cascading submenus (GameObject > VNEngine > <Category>[/<Subcategory>] > <Node>), instead of
// dragging prefabs in from Assets/Resources/Conversation Pieces/ by hand.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VNEngine;

public static class VNAddNodeMenu
{
    private const string ConversationPiecesPath = "Assets/Resources/Conversation Pieces/";
    private const string MenuRoot = "GameObject/VNEngine/";

    // ---- Actor / Entrance (actors entering/exiting the scene) ----------------------------
    [MenuItem(MenuRoot + "Actor/Entrance/Enter Actor", false, 10)]
    private static void Add_EnterActor(MenuCommand c) => Create(c, "Enter Actor");
    [MenuItem(MenuRoot + "Actor/Entrance/Enter Multiple Actors", false, 10)]
    private static void Add_EnterMultipleActors(MenuCommand c) => Create(c, "Enter Multiple Actors");
    [MenuItem(MenuRoot + "Actor/Entrance/Exit Actor", false, 10)]
    private static void Add_ExitActor(MenuCommand c) => Create(c, "Exit Actor");
    [MenuItem(MenuRoot + "Actor/Entrance/Exit All Actors", false, 10)]
    private static void Add_ExitAllActors(MenuCommand c) => Create(c, "Exit All Actors");

    // ---- Actor / Movement (actor positioning) ----------------------------------------------
    [MenuItem(MenuRoot + "Actor/Movement/Actor Forward", false, 10)]
    private static void Add_ActorForward(MenuCommand c) => Create(c, "Actor Forward");
    [MenuItem(MenuRoot + "Actor/Movement/Actor Backward", false, 10)]
    private static void Add_ActorBackward(MenuCommand c) => Create(c, "Actor Backward");
    [MenuItem(MenuRoot + "Actor/Movement/Actor Inward", false, 10)]
    private static void Add_ActorInward(MenuCommand c) => Create(c, "Actor Inward");
    [MenuItem(MenuRoot + "Actor/Movement/Actor Outward", false, 10)]
    private static void Add_ActorOutward(MenuCommand c) => Create(c, "Actor Outward");
    [MenuItem(MenuRoot + "Actor/Movement/Pan Actors", false, 10)]
    private static void Add_PanActors(MenuCommand c) => Create(c, "Pan Actors");

    // ---- Actor / Visual (actor appearance) --------------------------------------------------
    [MenuItem(MenuRoot + "Actor/Visual/Brighten Actor", false, 10)]
    private static void Add_BrightenActor(MenuCommand c) => Create(c, "Brighten Actor");
    [MenuItem(MenuRoot + "Actor/Visual/Brighten All Actors", false, 10)]
    private static void Add_BrightenAllActors(MenuCommand c) => Create(c, "Brighten All Actors");
    [MenuItem(MenuRoot + "Actor/Visual/Darken All Actors", false, 10)]
    private static void Add_DarkenAllActors(MenuCommand c) => Create(c, "Darken All Actors");
    [MenuItem(MenuRoot + "Actor/Visual/Change Actor Image", false, 10)]
    private static void Add_ChangeActorImage(MenuCommand c) => Create(c, "Change Actor Image");
    [MenuItem(MenuRoot + "Actor/Visual/Flip Actor Face Direction", false, 10)]
    private static void Add_FlipActorFaceDirection(MenuCommand c) => Create(c, "Flip Actor Face Direction");

    // ---- Actor / Dialogue --------------------------------------------------------------------
    [MenuItem(MenuRoot + "Actor/Dialogue/Dialogue", false, 10)]
    private static void Add_Dialogue(MenuCommand c) => Create(c, "Dialogue");
    [MenuItem(MenuRoot + "Actor/Dialogue/Hide Dialogue Panel", false, 10)]
    private static void Add_HideDialoguePanel(MenuCommand c) => Create(c, "Hide Dialogue Panel");
    [MenuItem(MenuRoot + "Actor/Dialogue/Think", false, 10)]
    private static void Add_Think(MenuCommand c) => Create(c, "Think");

    // ---- Background -----------------------------------------------------------------------
    [MenuItem(MenuRoot + "Background/Set Background", false, 10)]
    private static void Add_SetBackground(MenuCommand c) => Create(c, "Set Background");
    [MenuItem(MenuRoot + "Background/Turn Off Background", false, 10)]
    private static void Add_TurnOffBackground(MenuCommand c) => Create(c, "Turn Off Background");
    [MenuItem(MenuRoot + "Background/Turn Off Foreground", false, 10)]
    private static void Add_TurnOffForeground(MenuCommand c) => Create(c, "Turn Off Foreground");
    [MenuItem(MenuRoot + "Background/Fade in from Black", false, 10)]
    private static void Add_FadeInFromBlack(MenuCommand c) => Create(c, "Fade in from Black");
    [MenuItem(MenuRoot + "Background/Fade out to Black", false, 10)]
    private static void Add_FadeOutToBlack(MenuCommand c) => Create(c, "Fade out to Black");

    // ---- Scene & System / Branching ---------------------------------------------------------
    [MenuItem(MenuRoot + "Scene & System/Branching/Show Choices", false, 10)]
    private static void Add_ShowChoices(MenuCommand c) => Create(c, "Show Choices");
    [MenuItem(MenuRoot + "Scene & System/Branching/Show Choices - Timed", false, 10)]
    private static void Add_ShowChoicesTimed(MenuCommand c) => Create(c, "Show Choices - Timed");
    [MenuItem(MenuRoot + "Scene & System/Branching/Gate Stat Node", false, 10)]
    private static void Add_GateStatNode(MenuCommand c) => Create(c, "Gate Stat Node");

    // ---- Scene & System (direct) --------------------------------------------------------------
    [MenuItem(MenuRoot + "Scene & System/Begin Scene Setup", false, 10)]
    private static void Add_BeginSceneSetup(MenuCommand c) => Create(c, "Begin Scene Setup");
    [MenuItem(MenuRoot + "Scene & System/End Scene Setup", false, 10)]
    private static void Add_EndSceneSetup(MenuCommand c) => Create(c, "End Scene Setup");
    [MenuItem(MenuRoot + "Scene & System/Change Conversation", false, 10)]
    private static void Add_ChangeConversation(MenuCommand c) => Create(c, "Change Conversation");
    [MenuItem(MenuRoot + "Scene & System/Load Scene", false, 10)]
    private static void Add_LoadScene(MenuCommand c) => Create(c, "Load Scene");
    [MenuItem(MenuRoot + "Scene & System/Save Checkpoint", false, 10)]
    private static void Add_SaveCheckpoint(MenuCommand c) => Create(c, "Save Checkpoint");
    [MenuItem(MenuRoot + "Scene & System/Play Sound Effect", false, 10)]
    private static void Add_PlaySoundEffect(MenuCommand c) => Create(c, "Play Sound Effect");
    [MenuItem(MenuRoot + "Scene & System/Set Music", false, 10)]
    private static void Add_SetMusic(MenuCommand c) => Create(c, "Set Music");
    [MenuItem(MenuRoot + "Scene & System/Keyboard Entry", false, 10)]
    private static void Add_KeyboardEntry(MenuCommand c) => Create(c, "KeyboardEntry");
    [MenuItem(MenuRoot + "Scene & System/Wait", false, 10)]
    private static void Add_Wait(MenuCommand c) => Create(c, "Wait");
    [MenuItem(MenuRoot + "Scene & System/Time Passes", false, 10)]
    private static void Add_TimePasses(MenuCommand c) => Create(c, "Time Passes");

    // ---- Logging ----------------------------------------------------------------------------
    [MenuItem(MenuRoot + "Logging/Debug Stat Set", false, 10)]
    private static void Add_DebugStatSet(MenuCommand c) => Create(c, "Debug Stat Set");
    [MenuItem(MenuRoot + "Logging/OpenGameData Event", false, 10)]
    private static void Add_OpenGameDataEvent(MenuCommand c) => Create(c, "OpenGameData Event");

    // ---- Miscellaneous ----------------------------------------------------------------------
    [MenuItem(MenuRoot + "Miscellaneous/Closer Look", false, 10)]
    private static void Add_CloserLook(MenuCommand c) => Create(c, "Closer Look");
    [MenuItem(MenuRoot + "Miscellaneous/Text Message", false, 10)]
    private static void Add_TextMessage(MenuCommand c) => Create(c, "Text Message");
    [MenuItem(MenuRoot + "Miscellaneous/New Contact", false, 10)]
    private static void Add_NewContact(MenuCommand c) => Create(c, "New Contact");
    [MenuItem(MenuRoot + "Miscellaneous/Task", false, 10)]
    private static void Add_Task(MenuCommand c) => Create(c, "Task");

    // A single shared validate function, stacked with one [MenuItem(path, true)] attribute per
    // leaf path above - the enable condition is identical for all of them (a ConversationManager
    // must be resolvable from the current selection), so there's no need for 41 copies of it.
    [MenuItem(MenuRoot + "Actor/Entrance/Enter Actor", true)]
    [MenuItem(MenuRoot + "Actor/Entrance/Enter Multiple Actors", true)]
    [MenuItem(MenuRoot + "Actor/Entrance/Exit Actor", true)]
    [MenuItem(MenuRoot + "Actor/Entrance/Exit All Actors", true)]
    [MenuItem(MenuRoot + "Actor/Movement/Actor Forward", true)]
    [MenuItem(MenuRoot + "Actor/Movement/Actor Backward", true)]
    [MenuItem(MenuRoot + "Actor/Movement/Actor Inward", true)]
    [MenuItem(MenuRoot + "Actor/Movement/Actor Outward", true)]
    [MenuItem(MenuRoot + "Actor/Movement/Pan Actors", true)]
    [MenuItem(MenuRoot + "Actor/Visual/Brighten Actor", true)]
    [MenuItem(MenuRoot + "Actor/Visual/Brighten All Actors", true)]
    [MenuItem(MenuRoot + "Actor/Visual/Darken All Actors", true)]
    [MenuItem(MenuRoot + "Actor/Visual/Change Actor Image", true)]
    [MenuItem(MenuRoot + "Actor/Visual/Flip Actor Face Direction", true)]
    [MenuItem(MenuRoot + "Actor/Dialogue/Dialogue", true)]
    [MenuItem(MenuRoot + "Actor/Dialogue/Hide Dialogue Panel", true)]
    [MenuItem(MenuRoot + "Actor/Dialogue/Think", true)]
    [MenuItem(MenuRoot + "Background/Set Background", true)]
    [MenuItem(MenuRoot + "Background/Turn Off Background", true)]
    [MenuItem(MenuRoot + "Background/Turn Off Foreground", true)]
    [MenuItem(MenuRoot + "Background/Fade in from Black", true)]
    [MenuItem(MenuRoot + "Background/Fade out to Black", true)]
    [MenuItem(MenuRoot + "Scene & System/Branching/Show Choices", true)]
    [MenuItem(MenuRoot + "Scene & System/Branching/Show Choices - Timed", true)]
    [MenuItem(MenuRoot + "Scene & System/Branching/Gate Stat Node", true)]
    [MenuItem(MenuRoot + "Scene & System/Begin Scene Setup", true)]
    [MenuItem(MenuRoot + "Scene & System/End Scene Setup", true)]
    [MenuItem(MenuRoot + "Scene & System/Change Conversation", true)]
    [MenuItem(MenuRoot + "Scene & System/Load Scene", true)]
    [MenuItem(MenuRoot + "Scene & System/Save Checkpoint", true)]
    [MenuItem(MenuRoot + "Scene & System/Play Sound Effect", true)]
    [MenuItem(MenuRoot + "Scene & System/Set Music", true)]
    [MenuItem(MenuRoot + "Scene & System/Keyboard Entry", true)]
    [MenuItem(MenuRoot + "Scene & System/Wait", true)]
    [MenuItem(MenuRoot + "Scene & System/Time Passes", true)]
    [MenuItem(MenuRoot + "Logging/Debug Stat Set", true)]
    [MenuItem(MenuRoot + "Logging/OpenGameData Event", true)]
    [MenuItem(MenuRoot + "Miscellaneous/Closer Look", true)]
    [MenuItem(MenuRoot + "Miscellaneous/Text Message", true)]
    [MenuItem(MenuRoot + "Miscellaneous/New Contact", true)]
    [MenuItem(MenuRoot + "Miscellaneous/Task", true)]
    private static bool CanAdd(MenuCommand command) => TryResolveInsertionPoint(command, out _, out _);

    private static void Create(MenuCommand command, string prefabName)
    {
        if (!TryResolveInsertionPoint(command, out GameObject root, out Transform insertAfter))
            return;
        CreateNode(root, insertAfter, prefabName);
    }

    // A valid target is either a ConversationManager itself (new node appended at the end), or
    // any object nested under one of its direct-child nodes (new node inserted right after that
    // node, keeping sibling order - which is execution order - intact).
    private static bool TryResolveInsertionPoint(MenuCommand command, out GameObject root, out Transform insertAfter)
    {
        root = null;
        insertAfter = null;

        var target = command?.context as GameObject ?? Selection.activeGameObject;
        if (target == null) return false;

        if (target.GetComponent<ConversationManager>() != null)
        {
            root = target;
            return true;
        }

        for (Transform t = target.transform; t != null; t = t.parent)
        {
            if (t.parent != null && t.parent.GetComponent<ConversationManager>() != null)
            {
                root = t.parent.gameObject;
                insertAfter = t;
                return true;
            }
        }

        return false;
    }

    private static void CreateNode(GameObject root, Transform insertAfter, string prefabName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConversationPiecesPath + prefabName + ".prefab");
        if (prefab == null)
        {
            Debug.LogError($"VNAddNodeMenu: no prefab found at {ConversationPiecesPath}{prefabName}.prefab");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
        Undo.RegisterCreatedObjectUndo(instance, "Add Node: " + prefabName);

        instance.transform.SetSiblingIndex(insertAfter != null ? insertAfter.GetSiblingIndex() + 1 : root.transform.childCount - 1);

        Selection.activeGameObject = instance;
        EditorSceneManager.MarkSceneDirty(instance.scene);
    }
}
#endif
