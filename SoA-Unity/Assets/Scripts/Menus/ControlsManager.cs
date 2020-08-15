using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System;

public class ControlsManager : MonoBehaviour
{
    Inputs inputs;

    [SerializeField]
    [Tooltip("The button to rebind the move up action to another key")]
    private GameObject moveUpButton;

    [SerializeField]
    [Tooltip("The button to rebind the move left action to another key")]
    private GameObject moveLeftButton;

    [SerializeField]
    [Tooltip("The button to rebind the move down action to another key")]
    private GameObject moveDownButton;

    [SerializeField]
    [Tooltip("The button to rebind the move right action to another key")]
    private GameObject moveRightButton;

    [SerializeField]
    [Tooltip("The button to rebind the interact action to another key")]
    private GameObject interactButton;

    [SerializeField]
    [Tooltip("The button to rebind the eye protect action to another key")]
    private GameObject eyeProtectButton;

    [SerializeField]
    [Tooltip("The button to rebind the ear protect action to another key")]
    private GameObject earProtectButton;

    [SerializeField]
    [Tooltip("The button to restore the default bindings")]
    private GameObject restoreBindingsButton;

    [SerializeField]
    [Tooltip("The button to save the current bindings")]
    private GameObject saveBindingsButton;

    // Rebinding

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    private List<string> reservedPaths;

    // Navigation

    private Navigation navigationOff = new Navigation();
    private Navigation navigationAuto = new Navigation();

    // Start is called before the first frame update
    void Awake()
    {
        inputs = InputsManager.Instance.Inputs;

        navigationOff.mode = Navigation.Mode.None;
        navigationAuto.mode = Navigation.Mode.Automatic;

        // TO DO : Add a button to switch between gamepad and keyboard/mouse

        SwitchControls();

        UpdateButtons();

        reservedPaths = new List<string> {
                                      "<Keyboard>/AnyKey",
                                      "<Keyboard>/Alt",
                                      "<Keyboard>/LeftAlt",
                                      "<Keyboard>/PrintScreen",
                                      "<Pointer>/Press",
                                      inputs.Player.Interact.bindings[0].path,
                                      inputs.Player.ProtectEyes.bindings[0].path,
                                      inputs.Player.ProtectEars.bindings[0].path,
                                      inputs.Player.Walk.bindings[1].path,
                                      inputs.Player.Walk.bindings[2].path,
                                      inputs.Player.Walk.bindings[3].path,
                                      inputs.Player.Walk.bindings[4].path };
        Debug.Log("Size : " + reservedPaths.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RebindMoveUp() { RebindAction(inputs.Player.Walk, moveUpButton, 1); }

    public void RebindMoveLeft() { RebindAction(inputs.Player.Walk, moveLeftButton, 2); }

    public void RebindMoveDown() { RebindAction(inputs.Player.Walk, moveDownButton, 3); }

    public void RebindMoveRight() { RebindAction(inputs.Player.Walk, moveRightButton, 4); }

    public void RebindInteract() { RebindAction(inputs.Player.Interact, interactButton, 0); }

    public void RebindEyeProtect() { RebindAction(inputs.Player.ProtectEyes, eyeProtectButton, 0); }

    public void RebindEarsProtect() { RebindAction(inputs.Player.ProtectEars, earProtectButton, 0); }


    private void RebindAction(InputAction action, GameObject button, int index)
    {
        if (rebindOperation != null && !rebindOperation.completed)
        {
            rebindOperation.Cancel(); // in case of two successive clicks without key press
        }

        // Disable click and submit on the button
        button.GetComponent<EventTrigger>().enabled = false;

        // Disable navigation on the button
        button.GetComponent<Button>().navigation = navigationOff;

        button.transform.GetChild(0).GetComponent<Text>().text = "Press a key";

        action.Disable();

        Action<InputActionRebindingExtensions.RebindingOperation> callback = context =>
        {
            action.Enable();
            rebindOperation.Dispose();
            button.transform.GetChild(0).GetComponent<Text>().text = action.bindings[index].ToDisplayString();
            reservedPaths.Add(action.bindings[index].overridePath);
            button.GetComponent<EventTrigger>().enabled = true;
            button.GetComponent<Button>().navigation = navigationAuto;
        };

        rebindOperation = action.PerformInteractiveRebinding()
            .OnComplete(callback)
            .OnCancel(callback)
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithControlsHavingToMatchPath("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            // next two lines compulsory for a composite binding
            .WithTargetBinding(index)
            .WithExpectedControlType("Button")
            //.Start()
            ;

        // forbid utilization of a key already in use
        reservedPaths.Remove(action.bindings[index].overridePath);
        foreach (var path in reservedPaths)
        {
            Debug.Log(path);
            rebindOperation.WithControlsExcluding(path);
        }

        rebindOperation.Start();
    }

    void SwitchControls()
    {
        // Use mouse-keyboard controls scheme
        inputs.bindingMask = InputBinding.MaskByGroup(inputs.MouseKeyboardScheme.bindingGroup);

        // Use gamepad controls scheme
        //inputs.bindingMask = InputBinding.MaskByGroup(inputs.GamepadScheme.bindingGroup);
    }

    public void FinishInteractiveRebinding()
    {

    }

    public void ResetBindings()
    {
        // TO DO : Place default keys in a dictionary

        // US-Layout
        inputs.Player.Walk.ApplyBindingOverride(1, "<Keyboard>/W");
        inputs.Player.Walk.ApplyBindingOverride(2, "<Keyboard>/A");
        inputs.Player.Walk.ApplyBindingOverride(3, "<Keyboard>/S");
        inputs.Player.Walk.ApplyBindingOverride(4, "<Keyboard>/D");
        inputs.Player.Interact.ApplyBindingOverride("<Keyboard>/E");
        inputs.Player.ProtectEyes.ApplyBindingOverride("<Keyboard>/Q");
        inputs.Player.ProtectEars.ApplyBindingOverride("<Keyboard>/X");

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        moveUpButton.transform.GetChild(0).GetComponent<Text>().text     = inputs.Player.Walk.bindings[1].ToDisplayString();
        moveLeftButton.transform.GetChild(0).GetComponent<Text>().text   = inputs.Player.Walk.bindings[2].ToDisplayString();
        moveDownButton.transform.GetChild(0).GetComponent<Text>().text   = inputs.Player.Walk.bindings[3].ToDisplayString();
        moveRightButton.transform.GetChild(0).GetComponent<Text>().text  = inputs.Player.Walk.bindings[4].ToDisplayString();
        interactButton.transform.GetChild(0).GetComponent<Text>().text   = inputs.Player.Interact.GetBindingDisplayString();
        eyeProtectButton.transform.GetChild(0).GetComponent<Text>().text = inputs.Player.ProtectEyes.GetBindingDisplayString();
        earProtectButton.transform.GetChild(0).GetComponent<Text>().text = inputs.Player.ProtectEars.GetBindingDisplayString();

        restoreBindingsButton.transform.GetChild(0).GetComponent<Text>().text = "Reset Keys";
        saveBindingsButton.transform.GetChild(0).GetComponent<Text>().text = "Save Keys";
    }

    public void SaveBindings()
    {
        // TO DO : Save user preferences on the disk
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
    }
}
