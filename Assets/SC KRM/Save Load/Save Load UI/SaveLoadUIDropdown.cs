using SCKRM.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SCKRM.SaveLoad.UI
{
    [AddComponentMenu("SC KRM/Save Load/UI/Dropdown (Save file linkage)")]
    public class SaveLoadUIDropdown : SaveLoadUIBase
    {
        [SerializeField, FieldNotNull] Dropdown _dropdown; public Dropdown dropdown { get => _dropdown; set => _dropdown = value; }
        [SerializeField] UnityEvent _onValueChanged = new UnityEvent(); public UnityEvent onValueChanged { get => _onValueChanged; set => _onValueChanged = value; }

        public virtual void OnValueChanged()
        {
            if (invokeLock)
                return;

            if (variableType == VariableType.String)
            {
                if (dropdown.value >= 0 && dropdown.value < dropdown.options.Length)
                    SaveValue(dropdown.options[dropdown.value]);
            }
            else
                SaveStringValue(dropdown.value.ToString());

            ScriptOnValueChanged();
        }

        public override void VarReset()
        {
            base.VarReset();
            onValueChanged.RemoveAllListeners();
        }

        public override void SetDefault()
        {
            base.SetDefault();
            ScriptOnValueChanged();
        }
        
        public override void ScriptOnValueChanged(bool settingInfoInvoke = true)
        {
            Update();

            if (settingInfoInvoke)
            {
                if (variableType == VariableType.String)
                {
                    if (dropdown.value >= 0 && dropdown.value < dropdown.options.Length)
                    {
                        string text;
                        if (dropdown.value < dropdown.customLabel.Length)
                        {
                            text = dropdown.customLabel[dropdown.value];
                            if (string.IsNullOrEmpty(text))
                                text = dropdown.options[dropdown.value];
                        }
                        else
                            text = dropdown.options[dropdown.value];

                        SettingInfoInvoke(text);
                    }
                }
                else
                    SettingInfoInvoke(dropdown.value.ToString());
            }

            onValueChanged.Invoke();
        }

        protected override void Update()
        {
            base.Update();

            if (!InitialLoadManager.isInitialLoadEnd || !isLoad)
                return;

            if (variableType == VariableType.String)
            {
                string value = (string)GetValue();

                invokeLock = true;
                dropdown.value = Array.IndexOf(dropdown.options, value);
                invokeLock = false;

                isDefault = (string)defaultValue == value;
            }
            else
            {
                int value = GetValueInt();

                invokeLock = true;
                dropdown.value = value;
                invokeLock = false;

                isDefault = defaultValue.ToString() == GetValue().ToString();
            }
        }
    }
}